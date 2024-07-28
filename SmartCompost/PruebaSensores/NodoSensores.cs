using Iot.Device.Ds18b20;
using nanoFramework.Device.OneWire;
using NanoKernel.Nodos;
using System.Threading;
using System;
using nanoFramework.Hardware.Esp32;
using System.Device.Adc;

namespace NodoAP
{
    public class NodoSensores : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;
        OneWireHost oneWire;
        Ds18b20 ds18b20;
        AdcController adc;
        AdcChannel humedadAdc;
        AdcChannel bateriaAdc;

        double temperatura;
        int humedad;
        int bateria;


        public override void Setup()
        {

            Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

            oneWire = new OneWireHost();
            ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);


            /*https://docs.nanoframework.net/content/esp32/esp32_pin_out.html*/
            adc = new AdcController();

            /* ADC Channel 9 - GPIO 39 */
            humedadAdc = adc.OpenChannel(9);

            /* ADC Channel 6 - GPIO 34 */
            bateriaAdc = adc.OpenChannel(6);

            ds18b20.IsAlarmSearchCommandEnabled = false;
            /*if (ds18b20.Initialize())
            {
                Console.WriteLine($"Is sensor parasite powered?:{ds18b20.IsParasitePowered}");
                string devAddrStr = "";
                foreach (var addrByte in ds18b20.Address)
                {
                    devAddrStr += addrByte.ToString("X2");
                }

                Console.WriteLine($"Sensor address:{devAddrStr}");

            }
            else {
                throw new Exception("No anduvo viejo");
            }
            */
        }

        public override void Loop(ref bool activo)
        {
            //temperatura = MedirTemperatura();
            humedad = MedirHumedad();
            bateria = MedirBateria();

            Thread.Sleep(1000);

        }

        private int MedirHumedad()
        {
            int analogValue = humedadAdc.ReadValue();

            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Humedad: {analogValue}");
            return analogValue;

        }

        private int MedirBateria()
        {
            int analogValue = bateriaAdc.ReadValue();

            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Bateria: {analogValue}");
            return analogValue;
        }

        private double MedirTemperatura()
        {
            if (!ds18b20.TryReadTemperature(out var currentTemperature))
            {
                Console.WriteLine("Can't read!");
                return 0;
            }
            else
            {
                Console.WriteLine($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                return currentTemperature.DegreesCelsius;
            }
        }
    }

}

