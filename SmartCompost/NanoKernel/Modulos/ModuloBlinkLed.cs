using System;
using System.Device.Gpio;
using System.Threading;

namespace NanoKernel.Modulos
{
    [Modulo("Blink")]
    public class ModuloBlinkLed : IDisposable
    {
        private GpioController gpio;
        private GpioPin led;
        private int periodoMilis;
        private Timer timer;
        private const int MIN_PERIODO = 100;

        public ModuloBlinkLed(int periodoMilis = 1000, int ledPin = 2 /*default del esp-wroom-32*/)
        {
            gpio = new GpioController();
            led = gpio.OpenPin(ledPin, PinMode.Output);
            timer = new Timer(ToggleLed, null, Timeout.Infinite, Timeout.Infinite);
            this.ledOn = false;
            this.periodoMilis = periodoMilis;
        }

        public void CambiarPeriodo(int periodoMilis)
        {
            periodoMilis = periodoMilis < MIN_PERIODO ? MIN_PERIODO : periodoMilis;
            this.periodoMilis = periodoMilis;
            timer.Change(0, this.periodoMilis);
        }

        public void Iniciar()
        {
            CambiarPeriodo(this.periodoMilis);
        }
        public void Detener()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            led.Write(PinValue.Low);
        }

        public void High() { led.Write(PinValue.High); ledOn = true; }
        public void Low() {led.Write(PinValue.Low); ledOn = false; } 

        private bool ledOn = false;
        private void ToggleLed(object state)
        {
            if (ledOn)
                Low();
            else
                High();
        }

        public void Dispose()
        {
            Detener();
            timer.Dispose();
            gpio.Dispose();
        }
    }
}
