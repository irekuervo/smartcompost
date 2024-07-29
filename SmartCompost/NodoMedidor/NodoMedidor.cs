using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
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
        public override TiposNodo tipoNodo => TiposNodo.MedidorLora;

        // ---------------------------------------------------------------
        private const bool DEEPSLEEP = false;
        private const bool ES_PROTOBOARD = true;
        private const int segundosSleep = 15;
        private const int milisLoop = 1000;
        // ---------------------------------------------------------------
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
            Hilo.Intentar(() => {
                if (ES_PROTOBOARD)
                    lora = new LoRaDevice(pinLoraDatos: 4, pinLoraReset: 15);
                else
                    lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        readonly byte[] buffer = new byte[50];
        public override void Loop(ref bool activo)
        {
            // Mido la bateria
            float bateria = (float)random.NextDouble() * 100;
            // Mido la temperatura
            float temperatura = (float)random.NextDouble() * 5 + 25;
            // Mido la humedad
            float humedad = (float)random.NextDouble() * 100;

            // Mandamos el paquete
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryWriter bw = new BinaryWriter(ms);

                var fecha = DateTime.UtcNow.Ticks;
                
                bw.Write((byte)TipoPaqueteEnum.Medicion); // 1 byte
                bw.Write(InfoNodo.NumeroSerie); // Largo variable
                bw.Write(fecha); // 8 bytes (lo voy a usar como id de paquete)
                bw.Write(bateria); // 4 bytes
                bw.Write(temperatura);  // 4 bytes
                bw.Write(humedad);  // 4 bytes

                try
                {
                    lora.Enviar(ms.ToArray());
                    Blink();
                    Logger.Debug($"Paquete {fecha} enviado");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            if (DEEPSLEEP)
                aySleep.DeepSleepSegundos(segundosSleep);
            else
                Thread.Sleep(milisLoop);
        }

        private void Blink(int milis = 100)
        {
#if DEBUG
            led.Write(PinValue.High);
            Thread.Sleep(milis);
            led.Write(PinValue.Low); 
#endif
        }
    }
}
