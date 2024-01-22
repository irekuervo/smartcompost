using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Threading;

namespace AppDesarrollo
{
    public class ModuloBlinkLed
    {
        static GpioController gpio;
        static GpioPin led;

        public ModuloBlinkLed(int ledPin)
        {
            gpio = new GpioController();
            led = gpio.OpenPin(ledPin, PinMode.Output);

            new Thread(HiloLed).Start();
        }

        public static void HiloLed()
        {
            while (true)
            {
                led.Write(PinValue.High);
                Thread.Sleep(1000);
                led.Write(PinValue.Low);
                Thread.Sleep(1000);
            }
        }
    }
}
