
using NanoKernel.Hilos;
using System.Device.Gpio;
using System.Threading;

namespace NanoKernel.Modulos
{
    [Modulo("Blink")]
    ///TODO Si usa Hardware deberia usar otro modulo que lo gestione
    public class ModuloBlinkLed
    {
        static GpioController gpio;
        static GpioPin led;
        static int periodo = 1000;
        static Hilo hilo;

        public ModuloBlinkLed(int ledPin)
        {
            gpio = new GpioController();
            led = gpio.OpenPin(ledPin, PinMode.Output);

            hilo = Hilos.Hilos.CrearHiloLoop("Blinker", HiloLed);
        }

        public void Start(int periodo)
        {
            ModuloBlinkLed.periodo = periodo;
            hilo.Iniciar();
        }

        public void Stop()
        {
            hilo.Detener();
        }

        private static void HiloLed(ref bool detener)
        {
            if (detener) return;
            led.Write(PinValue.High);
            Thread.Sleep(periodo);

            if (detener) return;
            led.Write(PinValue.Low);
            Thread.Sleep(periodo);
        }


    }
}
