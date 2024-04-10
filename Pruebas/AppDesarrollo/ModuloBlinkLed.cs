using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Threading;

namespace AppDesarrollo
{
    public class ModuloBlinkLed
    {
        static GpioController gpio;
        static GpioPin led;
        static int periodo = 1000;

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
                Thread.Sleep(periodo);
                led.Write(PinValue.Low);
                Thread.Sleep(periodo);
            }
        }

        public void Error()
        {
            periodo = 300;
        }

        public void OK()
        {
            periodo = 1000;
        }
    }
}
