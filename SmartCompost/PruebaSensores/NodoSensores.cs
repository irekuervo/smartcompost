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
            adc = new AdcController();

            /* HUMEDAD          -----> ADC Channel 7 - GPIO 35 */
            humedadAdc = adc.OpenChannel(7);

            /* BATERIA SENSOR   -----> ADC Channel 6 - GPIO 34 */
            bateriaAdcSensor = adc.OpenChannel(6);

            /* BATERIA AP       -----> ADC Channel 4 - GPIO 32 */
            // bateriaAdcAp = adc.OpenChannel(4);

            ConfigurarSensorTemperatura();
        }

        private void ConfigurarSensorTemperatura()
        {
            Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

            OneWireHost oneWire = new OneWireHost();


            ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

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
            //MedirTemperatura();
            MedirHumedad();
            //MedirBateriaSensor();

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
            * f(x) = 757.17 - 910.85x + 368.01x² - 48.95x³ 
            */
            int analogValue = humedadAdc.ReadValue();

            float vSensor = (analogValue / 4095f * 3.3f);
            double humidityPercentage = (-48.95 * vSensor * vSensor * vSensor) + (368.10 * vSensor * vSensor) - 910.85 * vSensor + 757.17;

            Console.WriteLine($"Analog Humedad: {analogValue}");
            Console.WriteLine($" tension: {vSensor}");
            if(humidityPercentage>100) humidityPercentage=100;
            if(humidityPercentage<0) humidityPercentage=0;

            return humidityPercentage;

        }

        private int MedirBateriaSensor()
        {
            int analogValue = bateriaAdcSensor.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;

            // Cuenta de la bateria, mapeando las cotas con el ADC
            // y = a x + b
            // 0 = a * 2.52 V + b
            // 100 = a * 3.3 V + b
            // y = 128.21 * x − 323.06
            double bateriaPorcentaje = 138.89 * vSensor - 358.33;
            if (bateriaPorcentaje > 100) bateriaPorcentaje = 100;
            if (bateriaPorcentaje < 0) bateriaPorcentaje = 0;

            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Bateria: {bateriaPorcentaje}");
            return analogValue;
        }

        private int MedirBateriaAp()
        {
            int analogValue = bateriaAdcAp.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;
            // Cuenta de la bateria, mapeando las cotas con el ADC
            // y = a x + b
            // 0 = a * 2.58 V + b
            // 100 = a * 3.3 V + b
            // y = 151.52 * x − 400
            double bateriaPorcentaje = 151.52 * vSensor - 400;
            if (bateriaPorcentaje > 100) bateriaPorcentaje = 100;
            if (bateriaPorcentaje < 0) bateriaPorcentaje = 0;

            /*definir la matematica para devilver porcentaje*/
            Console.WriteLine($"Bateria: {bateriaPorcentaje}");
            return analogValue;
        }

    }

}

