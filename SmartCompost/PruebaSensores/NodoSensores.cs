using Iot.Device.Ds18b20;
using nanoFramework.Device.OneWire;
using nanoFramework.Hardware.Esp32;
using NanoKernel.Dominio;
using NanoKernel.Nodos;
using System;
using System.Device.Adc;
using System.Threading;

namespace NodoAP
{
    public class NodoSensores : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;
        OneWireHost oneWire;
        Ds18b20 ds18b20;
        AdcController adc;
        AdcChannel humedadAdc;
        AdcChannel bateriaAdcSensor;
        AdcChannel bateriaAdcAp;


        double temperatura;
        int humedad;
        int bateriaAp;
        int bateriaSensor;


        public override void Setup()
        {
            /* DOCUMENTACION PARA VER PINOUT
          * https://docs.nanoframework.net/content/esp32/esp32_pin_out.html 
          */
            //adc = new AdcController();

            /* HUMEDAD          -----> ADC Channel 4 - GPIO 32 */
            //humedadAdc = adc.OpenChannel(4);

            /* BATERIA SENSOR   -----> ADC Channel 6 - GPIO 34 */
            //bateriaAdcSensor = adc.OpenChannel(6);

            /* BATERIA AP       -----> ADC Channel 7 - GPIO 35 */
            // bateriaAdcAp = adc.OpenChannel(7);

            ConfigurarSensorTemperatura();
        }

        private static void ConfigurarSensorTemperatura()
        {
            Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

            OneWireHost oneWire = new OneWireHost();


            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

            ds18b20.IsAlarmSearchCommandEnabled = false;
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
        }

        public override void Loop(ref bool activo)
        {
            MedirTemperatura();

            Thread.Sleep(1000);
        }

        private double MedirTemperatura()
        {
            if (!ds18b20.TryReadTemperature(out var currentTemperature))
            {
                Console.WriteLine("Can't read!");
                return -1;
            }
            else
            {
                Console.WriteLine($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                return currentTemperature.DegreesCelsius;
            }
        }

        // FUNCIONES
        private int MedirHumedad()
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

            Console.WriteLine($"Humedad: {humidityPercentage}");
            return analogValue;

        }

        private int MedirBateriaSensor()
        {
            int analogValue = bateriaAdcSensor.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;
            double bateriaPorcentaje = 90.9 * vSensor - 354.5;

            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Bateria: {analogValue}");
            return analogValue;
        }

        private int MedirBateriaAp()
        {
            int analogValue = bateriaAdcAp.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;
            double bateriaPorcentaje = 100 * vSensor - 400;


            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Bateria: {analogValue}");
            return analogValue;
        }

    }

}

