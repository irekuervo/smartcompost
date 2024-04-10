
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
        static int periodoMilis = 1000;
        static Hilo hilo;

        public ModuloBlinkLed(int ledPin = 2 /*default del esp-wroom-32*/)
        {
            gpio = new GpioController();
            led = gpio.OpenPin(ledPin, PinMode.Output);

            hilo = MotorDeHilos.CrearHiloLoop("Blinker", HiloLed);
        }

        public void CambiarPeriodo(int periodoMilis)
        {
            ModuloBlinkLed.periodoMilis = periodoMilis;
        }

        public void Iniciar(int periodoMilis)
        {
            ModuloBlinkLed.periodoMilis = periodoMilis;
            hilo.Iniciar();
        }

        public void Detener()
        {
            hilo.Detener();
        }

        private static void HiloLed(ref bool activo)
        {
            if (!activo) return;
            led.Write(PinValue.High);
            Thread.Sleep(periodoMilis);

            if (!activo) return;
            led.Write(PinValue.Low);
            Thread.Sleep(periodoMilis);
        }


    }
}
