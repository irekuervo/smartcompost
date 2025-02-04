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

        private const int segundosSleep = 1;

        // -----LORA--------------------------------------------------------
        private LoRaDevice lora;
        private const double LORA_FREQ = 433e6; //920_000_000; //Banda libre (915 – 928) MHz Resolución N° 4653/19:
        private const int LORA_PIN_MISO = 19;
        private const int LORA_PIN_MOSI = 23;
        private const int LORA_PIN_CLK = 18;
        private const int LORA_PIN_NSS = 5;
        private const int LORA_PIN_DIO0 = 25;
        private const int LORA_PIN_RESET = 14;
        // -----LED---------------------------------------------------------
        private GpioController gpio;
        private GpioPin led;
        private const int PIN_LED_ONBOARD = 2;
        // -----SENSORES----------------------------------------------------
        private OneWireHost oneWire;
        private Ds18b20 ds18b20;
        private GpioPin vccSensores;
        private AdcController adc;
        private AdcChannel humedadAdc;
        private AdcChannel bateriaAdcSensor;
        private AdcChannel bateriaAdcAp;

        private const int ONE_WIRE_RX = 16;     // importante puentear RX y TX
        private const int ONE_WIRE_TX = 17;
        // https://docs.nanoframework.net/content/esp32/esp32_pin_out.html
        private const int ADC_HUMEDAD = 7;  //pin 36        // ADC Channel 4 - GPIO 32
        private const int ADC_BATERIA = 6;  //pin 39        // ADC Channel 6 - GPIO 34
        private const float ERROR_TEMP = -100;
        //private const int PIN_VCC_SENSORES = 22; // Pin digial, para alimentar sensores
        // -----VARS--------------------------------------------------------
        private MedicionesNodoDto dto;
        private readonly byte[] bufferLora = new byte[LoRaDevice.MAX_LORA_PAYLOAD_BYTES];

        public override void Setup()
        {
            // TODO: Esto deberia hacerse con el deploy, no hardcodearse
            //Config.NumeroSerie = "1a352781-7dc2-11ef-abe5-0242ac160002"; // nodo 1
            Config.NumeroSerie = "1a56a612-7dc2-11ef-abe5-0242ac160002"; // nodo 2

            // -----LED---------------------------------------------------------
            gpio = new GpioController();
            led = gpio.OpenPin(PIN_LED_ONBOARD, PinMode.Output);
            led.Write(PinValue.High);   // Prendemos el led para avisar que estamos configurando

            // -----LORA--------------------------------------------------------
            Hilo.Intentar(() =>
            {
                lora = new LoRaDevice(
                    pinMISO: LORA_PIN_MISO,
                    pinMOSI: LORA_PIN_MOSI,
                    pinSCK: LORA_PIN_CLK,
                    pinNSS: LORA_PIN_NSS,
                    pinDIO0: LORA_PIN_DIO0,
                    pinReset: LORA_PIN_RESET);
                lora.Iniciar(LORA_FREQ);
            }, "Lora", accionException: () => { lora?.Dispose(); });

            // -----SENSORES----------------------------------------------------
            adc = new AdcController();
            humedadAdc = adc.OpenChannel(ADC_HUMEDAD);
            bateriaAdcSensor = adc.OpenChannel(ADC_BATERIA);

            // TODO: Hay q pensarlo bien
            //vccSensores = gpio.OpenPin(PIN_VCC_SENSORES, PinMode.Output);

            Configuration.SetPinFunction(ONE_WIRE_RX, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(ONE_WIRE_TX, DeviceFunction.COM3_TX);

            ds18b20 = new Ds18b20(new OneWireHost(), null, false, TemperatureResolution.VeryHigh);
            ds18b20.IsAlarmSearchCommandEnabled = false;

            Hilo.Intentar(() =>
            {
                if (!ds18b20.Initialize())
                    throw new Exception("No se pudo conectar al sensor de temperatura");
            }, intentos: 3);

            // -----DTO---------------------------------------------------------
            dto = new MedicionesNodoDto() { serial_number = Config.NumeroSerie };

            led.Write(PinValue.Low);  // Avisamos que terminamos de configurar
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

                lora.ModoSleep();

#if DEBUG
                Logger.Debug($"Sleep por {segundosSleep}seg");
                Thread.Sleep((int)(segundosSleep * 1000));
#else
                aySleep.DeepSleepSegundos(segundosSleep);
#endif

            }
        }

        private float MedirHumedad()
        {
            /*
             * La matematica del sensor es la siguiente
             * f(x) = 757.17 - 910.85x + 368.01x² - 48.95x³ 
             */
            int analogValue = humedadAdc.ReadValue();

            float vSensor = (analogValue / 4095f * 3.3f);
            float humidityPercentage = (-48.95f * vSensor * vSensor * vSensor) + (368.10f * vSensor * vSensor) - 910.85f * vSensor + 757.17f;

            humidityPercentage = humidityPercentage.ToPorcentaje();

            Logger.Debug($"Humedad: {analogValue}/{humidityPercentage}%");

            return (float)humidityPercentage;
        }

        private float MedirTemperatura()
        {
            if (!ds18b20.TryReadTemperature(out var currentTemperature))
            {
                Logger.Error("Temperatura: Error de lectura!");
                return ERROR_TEMP;
            }
            else
            {
                Logger.Debug($"Temperatura: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
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
            // y = 138,9 * x − 358.33
            float bateriaPorcentaje = 138.89f * vSensor - 358.33f;
            bateriaPorcentaje = bateriaPorcentaje.ToPorcentaje();

            /*definir la matematica para devilver porcentaje*/
            Logger.Debug($"Tension Bateria: {analogValue}");
            Logger.Debug($"-----Bateria: {bateriaPorcentaje}");
            return bateriaPorcentaje;
        }

        private void Blink(int milis = 100)
        {
            led.Write(PinValue.High);
            Thread.Sleep(milis);
            led.Write(PinValue.Low);
        }
    }
}
