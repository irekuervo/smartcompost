using Equipos.SX127X;
using System;
using System.Text;
using System.Threading;

namespace PruebaLoraSender
{
    public class Program
    {
        private const double FRECUENCIA = 433e6;
        private const int PIN_MISO = 19;
        private const int PIN_MOSI = 23;
        private const int PIN_CLK = 18;
        private const int PIN_NSS = 5;
        private const int PIN_DIO0 = 25;
        private const int PIN_RESET = 14;

        public static void Main()
        {
            var lora = new LoRaDevice(
                pinMISO: PIN_MISO,
                pinMOSI: PIN_MOSI,
                pinSCK: PIN_CLK,
                pinNSS: PIN_NSS,
                pinDIO0: PIN_DIO0,
                pinReset: PIN_RESET);

            lora.Iniciar(FRECUENCIA);

            int sendCount = 0;
            while (true)
            {
                string messageText = $"Sender envia {sendCount += 1}!";
                byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageText);
                Console.WriteLine(messageText);

                lora.Enviar(messageBytes);

                lora.ModoSleep();

                Thread.Sleep(1000);
            }
        }
    }
}
