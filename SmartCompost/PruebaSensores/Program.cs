using Iot.Device.Ds18b20;
using nanoFramework.Device.OneWire;
using nanoFramework.Hardware.Esp32;
using NodoAP;
using System;
using System.Threading;

namespace PruebaSensores
{
    public class Program
    {
        public static void Main()
        {
            // new NodoSensores().Iniciar();

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

                while (true)
                {
                    if (!ds18b20.TryReadTemperature(out var currentTemperature))
                    {
                        Console.WriteLine("Can't read!");
                    }
                    else
                    {
                        Console.WriteLine($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                    }

                    Thread.Sleep(1000);
                }
            }

            oneWire.Dispose();
        }
    }
}
