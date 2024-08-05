using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace PruebaWifi
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");


            // Configuramos el LED
            var gpio = new GpioController();
            var led = gpio.OpenPin(8, PinMode.Output);

            while (true)
            {
                led.Write(PinValue.High);
                Thread.Sleep(1000);
                led.Write(PinValue.Low);
                Thread.Sleep(1000);
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
