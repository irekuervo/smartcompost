using NanoKernel.Modulos;
using System;
using System.Device.Adc;
using System.Threading;

namespace NodoMedidor
{
    [Modulo("Sensores")]
    public class ModuloSensor
    {
        private static Random random = new Random();

        private static AdcController adc = new AdcController();
       

        public ModuloSensor()
        {

        }

        [Servicio("ValorPin")]
        public int ValorPin(int pin)
        {
            AdcChannel channel = adc.OpenChannel(pin);
            return channel.ReadValue();
        }

        [Servicio("Temperatura")]
        public double Temperatura()
        {
            return random.NextDouble() * 100;
        }

        [Servicio("Humedad")]
        public double Humedad()
        {
            return random.NextDouble() * 20;
        }

        [Servicio("Medir")]
        public Medicion Medir()
        {
            return new Medicion(Temperatura(), Humedad());
        }
    }

    public class Medicion
    {
        public double Temperatura { get; set; }
        public double Humedad { get; set; }

        public Medicion(double temperatura, double humedad)
        {
            this.Temperatura = temperatura;
            this.Humedad = humedad;
        }
    }
}
