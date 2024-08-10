using Equipos.DS18B20;
using Equipos.SX127X;
using nanoFramework.Device.OneWire;
using nanoFramework.Hardware.Esp32;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Adc;
using System.Device.Gpio;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.MedidorLora;

        // ---------------------------------------------------------------
        private const int segundosSleep = 5;
        // ---------------------------------------------------------------
        private LoRaDevice lora;
        // ---------------------------------------------------------------
        private GpioController gpio;
        private GpioPin led;
        private const int pinLedOnboard = 2;
        // ---------------------------------------------------------------
        private OneWireHost oneWire;
        private Ds18b20 ds18b20;
        private const int pinUART_RX = 16;
        private const int pinUART_TX = 17;
        private AdcController adc;
        private AdcChannel humedadAdc;
        private AdcChannel bateriaAdcSensor;
        private AdcChannel bateriaAdcAp;
        // ---------------------------------------------------------------
        private MedicionesNodoDto dto;
        private Random random = new Random();

        private readonly byte[] bufferLora = new byte[128];

        private readonly string[] codigos = new string[]{
            "b2c40a98-5534-11ef-92ae-0242ac140004",
            "282a2047-5668-11ef-92ae-0242ac140004",
            "2cbf5e3f-5668-11ef-92ae-0242ac140004" };

        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(pinLedOnboard, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            Hilo.Intentar(() =>
            {
                lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora", accionException: () => { lora.Dispose(); });


            //TODO: simulamos un nodo random, tiene que venir configurado
            Config.NumeroSerie = codigos[random.Next(3)];
            dto = new MedicionesNodoDto() { serial_number = Config.NumeroSerie};

            // ------------------------------------------------------------------
            // Sensores
            // ------------------------------------------------------------------

            adc = new AdcController();

            /* HUMEDAD          -----> ADC Channel 4 - GPIO 32 */
            humedadAdc = adc.OpenChannel(4);

            /* BATERIA SENSOR   -----> ADC Channel 6 - GPIO 34 */
            bateriaAdcSensor = adc.OpenChannel(6);

            Configuration.SetPinFunction(pinUART_RX, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(pinUART_TX, DeviceFunction.COM3_TX);

            ConfigurarSensorTemperatura();

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        private void ConfigurarSensorTemperatura()
        {
            OneWireHost oneWire = new OneWireHost();

            ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

            ds18b20.IsAlarmSearchCommandEnabled = false;
            Hilo.Intentar(() =>
            {
                if (ds18b20.Initialize())
                {
                    Console.WriteLine($"Is sensor parasite powered?:{ds18b20.IsParasitePowered}");
                    string devAddrStr = "";
                    foreach (var addrByte in ds18b20.Address)
                    {
                        devAddrStr += addrByte.ToString("X2");
                    }

                    Console.WriteLine($"Sensor address:{devAddrStr}");
                }
                else
                {
                    throw new Exception("No se pudo conectar al sensor de temperatura");
                }
            });
        }

        public override void Loop(ref bool activo)
        {
            try
            {
                dto.AgregarMedicion(MedirBateria(), TiposMediciones.Bateria);
                dto.AgregarMedicion(MedirTemperatura(), TiposMediciones.Temperatura);
                dto.AgregarMedicion(MedirHumedad(), TiposMediciones.Humedad);
                dto.last_updated = DateTime.UtcNow;

                long length = dto.ToBytes(bufferLora);
                lora.Enviar(bufferLora, 0, (int)length);

                Logger.Debug($"Paquete {dto.last_updated.Ticks} enviado, {length} bytes");
                Blink();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                dto.measurements.Clear();

                LimpiarMemoria();

                aySleep.DeepSleepSegundos(segundosSleep);
            }
        }

        private float MedirHumedad()
        {
            /*
           * La matematica del sensor es la siguiente
           * Vsensor = (analogread/1023)*5
           * y = -5,9732x^3 + 63,948x^2 - 232,8x + 308,98 
           */
            int analogValue = humedadAdc.ReadValue();

            float vSensor = (analogValue / 4095f * 3.3f);
            double humidityPercentage = (-5.9732 * vSensor * vSensor * vSensor) + (63.948 * vSensor * vSensor) - 232.8 * vSensor + 308.98;

            //Ver la funcion Pow
            //double humidityPercentage = (-5.9732*Pow(vSensor,3)) + (63.948*Pow(vSensor,2)) - 232.8*vSensor + 308.98;

            Logger.Debug($"Humedad: {humidityPercentage}");
            return analogValue;
        }

        private float MedirTemperatura()
        {
            if (!ds18b20.TryReadTemperature(out var currentTemperature))
            {
                Logger.Debug("Can't read!");
                return -1;
            }
            else
            {
                Logger.Debug($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                return (float)currentTemperature.DegreesCelsius;
            }
        }

        private float MedirBateria()
        {
            int analogValue = bateriaAdcSensor.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;

            // Cuenta de la bateria, mapeando las cotas con el ADC
            // y = a x + b
            // 0 = a * 2.52 V + b
            // 100 = a * 3.3 V + b
            // y = 128.21 * x − 323.06
            double bateriaPorcentaje = 128.21 * vSensor - 323.06;
            if (bateriaPorcentaje > 100) bateriaPorcentaje = 100;
            if (bateriaPorcentaje < 0) bateriaPorcentaje = 0;

            /*definir la matematica para devilver porcentaje*/
            Logger.Debug($"Bateria: {bateriaPorcentaje}");
            return analogValue;
        }

        private void Blink(int milis = 100)
        {
#if DEBUG
            led.Write(PinValue.High);
            Thread.Sleep(milis);
            led.Write(PinValue.Low);
#endif
        }
    }
}
