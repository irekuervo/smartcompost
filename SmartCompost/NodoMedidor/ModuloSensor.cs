using NanoKernel.Modulos;
using System;
using System.Device.Adc;

namespace NodoMedidor
{
    [Modulo("Sensores")]
    public class ModuloSensor
    {
        private static Random random = new Random();

        private static AdcController adc = new AdcController();

        const int idMock = 1;

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
        public Medicion Medir(int id = idMock)
        {
            return new Medicion(id, Temperatura(), Humedad());
        }
    }

    public class Medicion
    {
        public Medicion()
        {
            // Para serializar
        }

        public Medicion(int id, double temperatura, double humedad)
        {
            this.id = id;
            this.datetime = DateTime.UtcNow;
            

        }

        public int id { get; set; }
        public DateTime datetime { get; set; }
        public Array measurments { get; set; }
    }

    public class MedicionSensor
    { 
        public string type { get; set; }
        public float value { get; set; }
    
    }

}
