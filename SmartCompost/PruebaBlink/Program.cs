using System.Collections;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace PruebaBlink
{
    public class Program
    {
        public static void Main()
        {
            GpioController gpio;
            GpioPin led;

            // Configuramos el LED
            gpio = new GpioController();

            led = gpio.OpenPin(2, PinMode.Output);

            Debug.WriteLine("Hello from nanoFramework!");

            while (true)
            {
                led.Write(PinValue.High);
                Thread.Sleep(1000);
                led.Write(PinValue.Low);
                Thread.Sleep(1000);
            }

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
