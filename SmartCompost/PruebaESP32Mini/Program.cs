using System;
using System.Device.Gpio;
using System.Threading;

namespace PruebaESP32Mini
{
    public class Program
    {
        public static void Main()
        {
            var gpio = new GpioController();
            var led = gpio.OpenPin(8, PinMode.Output);
            while (true)
            {
                led.Write(PinValue.High);
                Thread.Sleep(1000);
                led.Write(PinValue.Low);
                Thread.Sleep(1000);
                Console.WriteLine("holis");
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
