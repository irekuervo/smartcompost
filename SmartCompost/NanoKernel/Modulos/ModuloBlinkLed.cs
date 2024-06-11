
using NanoKernel.Hilos;
using System;
using System.Device.Gpio;
using System.Threading;

namespace NanoKernel.Modulos
{
    [Modulo("Blink")]
    ///TODO Si usa Hardware deberia usar otro modulo que lo gestione
    public class ModuloBlinkLed : IDisposable
    {
        private GpioController gpio;
        private GpioPin led;
        private int periodoMilis;
        private Hilo hilo;

        public ModuloBlinkLed(int ledPin = 2 /*default del esp-wroom-32*/, int periodoMilis = 1000)
        {
            gpio = new GpioController();
            led = gpio.OpenPin(ledPin, PinMode.Output);

            this.periodoMilis = periodoMilis;

            hilo = MotorDeHilos.CrearHiloLoop("Blinker", HiloLed);
        }

        public void CambiarPeriodo(int periodoMilis)
        {
            this.periodoMilis = periodoMilis;
        }

        public void Iniciar(int periodoMilis = 0)
        {
            if (periodoMilis > 0)
                this.periodoMilis = periodoMilis;

            hilo.Iniciar();
        }

        public void High() => led.Write(PinValue.High);
        public void Low() => led.Write(PinValue.Low);

        public void Detener()
        {
            hilo.Detener();
            led.Write(PinValue.Low);
        }

        private void HiloLed(ref bool activo)
        {
            if (!activo) return;
            High();
            Thread.Sleep(periodoMilis);

            if (!activo) return;
            Low();
            Thread.Sleep(periodoMilis);
        }

        public void Dispose()
        {
            Detener();
            gpio.Dispose();
        }
    }
}
