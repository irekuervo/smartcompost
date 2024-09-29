using Equipos.SX127X;
using System;
using System.Text;
using System.Threading;

namespace PruebaLoraReciever
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
            lora.ModoRecibir();

            lora.OnReceive += Lora_OnReceive;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Lora_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                string messageText = UTF8Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
                Console.WriteLine($"RECIEVER: PacketSNR: {e.PacketSnr:0.0}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes \r\n{messageText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
