using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace AppDesarrollo
{
    public class Program
    {
        static GpioController gpio;
        static GpioPin led;
        public static void Main()
        {

            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);

           

            new Thread(HiloLed).Start();

            //Thread.Sleep(5000);

           // Configuration.SetPinFunction(34, DeviceFunction.COM2_RX);
            //Configuration.SetPinFunction(35, DeviceFunction.COM2_TX);

            //var serial = new SerialPort("COM2", 9600);
            //serial.Open();

           // new Thread(() => { for (; ; ) { serial.WriteLine("Alive"); Thread.Sleep(1000); } }).Start();

            Thread.Sleep(Timeout.Infinite);
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
