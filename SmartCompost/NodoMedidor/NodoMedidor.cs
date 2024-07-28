using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.IO;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override string IdSmartCompost => "Medidor";
        public override TiposNodo tipoNodo => TiposNodo.Medidor;

        private const bool MODO_LOOP = true; // En false se va a dormir
        private const int segundosSleep = 15;

        private Random random = new Random();
        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;

        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            lora = new LoRaDevice(pinLoraDatos: 4); // el pin 4 lo uso en la protoboard
            // Intentamos conectarnos al lora
            Hilo.Intentar(() => lora.Iniciar(), "Lora");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        const int tamanioPaquete = 27;
        readonly byte[] paquete = new byte[tamanioPaquete];
        public override void Loop(ref bool activo)
        {
            // Mido la bateria
            float bateria = (float)random.NextDouble() * 100;
            // Mido la temperatura
            float temperatura = (float)random.NextDouble() * 5 + 25;
            // Mido la humedad
            float humedad = (float)random.NextDouble() * 100;

            // Mandamos el paquete
            using (MemoryStream ms = new MemoryStream(paquete))
            {
                var fecha = DateTime.UtcNow.Ticks;
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write((byte)TipoPaqueteEnum.Medicion); // 1 byte
                bw.Write(ayInternet.GetMacAddress().Address); // 6 bytes
                bw.Write(fecha); // 8 bytes (lo voy a usar como id de paquete)
                bw.Write(bateria); // 4 bytes
                bw.Write(temperatura);  // 4 bytes
                bw.Write(humedad);  // 4 bytes

                try
                {
                    lora.Enviar(paquete);
                    Logger.Log($"Paquete {fecha} enviado");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            if (MODO_LOOP)
            {
                Thread.Sleep(5000);
            }
            else
            {
                aySleep.DeepSleepSegundos(segundosSleep);
            }
        }

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }
    }
}
