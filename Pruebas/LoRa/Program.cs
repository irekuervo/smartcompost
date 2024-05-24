using devMobile.IoT.SX127xLoRaDevice;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace LoRa
{
    ///Github: https://github.com/KiwiBryn/SX127X-NetNF/tree/master
    public class Program
    {
        const int SPI_BUS_ID = 1;
        const double Frequency = 915_000_000.0;

        static SX127XDevice sender;
        const int sender_NSS = Gpio.IO05;
        const int senderDI00 = Gpio.IO25;
        const int senderReset = Gpio.IO14;

        public static void Main()
        {
            Debug.WriteLine("Sender LoRa!");

            // Config SPI1
            Configuration.SetPinFunction(Gpio.IO19, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(Gpio.IO23, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(Gpio.IO18, DeviceFunction.SPI1_CLOCK);

            var spiSender = new SpiConnectionSettings(SPI_BUS_ID, sender_NSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0,// From SemTech docs pg 80 CPOL=0, CPHA=0
                                     //SharingMode = SpiSharingMode.Shared
            };

            var spi = new SpiDevice(spiSender);
            var gpio = new GpioController();

            bool ok = false;
            while (!ok)
            {
                try
                {
                    sender = new SX127XDevice(spi, gpio, dio0Pin: senderDI00);
                    sender.Initialize(
                        Frequency,
                        lnaGain: RegLnaLnaGain.Default,
                        lnaBoost: true,
                        powerAmplifier: RegPAConfigPASelect.PABoost,
                        rxPayloadCrcOn: true,
                        rxDoneignoreIfCrcMissing: false
                        );

                    Debug.WriteLine("sender OK!");
                    ok = true;
                }
                catch (Exception)
                {
                    Debug.WriteLine("sender Ups!");
                    ok = false;
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            //sender.OnReceive += Sender_OnReceive;
            sender.OnTransmit += Sender_OnTransmit;

            Thread.Sleep(500);

            int sendCount = 0;
            while (true)
            {
                string messageText = $"Sender envia {sendCount += 1}!";
                byte[] messageBytes = UTF8Encoding.UTF8.GetBytes(messageText);
                sender.Send(messageBytes);
                Thread.Sleep(5000);
            }

            //Thread.Sleep(Timeout.Infinite);
        }

        private static void Sender_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                string messageText = UTF8Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

                Log($"SENDER: PacketSNR: {e.PacketSnr:0.0}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes \r\n{messageText}");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private static void Sender_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {
            //Program.sender.Receive();
            Log($"Sender Envia OK");
        }

        private static void Log(string message)
        {
            Console.WriteLine(string.Format("{0} | {1}", DateTime.UtcNow.ToString("HH:mm:ss"), message));
        }
    }
}
