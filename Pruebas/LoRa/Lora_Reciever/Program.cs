using devMobile.IoT.SX127xLoRaDevice;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Lora_Reciever
{
    public class Program
    {
        const int SPI_BUS_ID = 1;
        const double Frequency = 915_000_000.0;

        static SX127XDevice reciever;
        const int reciever_NSS = Gpio.IO05;
        const int recieverDI00 = Gpio.IO25;
        const int recieverReset = Gpio.IO14;

        public static void Main()
        {
            Debug.WriteLine("Reciever LoRa!");

            // Config SPI1
            Configuration.SetPinFunction(Gpio.IO19, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(Gpio.IO23, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(Gpio.IO18, DeviceFunction.SPI1_CLOCK);

            var spiReciever = new SpiConnectionSettings(SPI_BUS_ID, reciever_NSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0,// From SemTech docs pg 80 CPOL=0, CPHA=0
                                     //SharingMode = SpiSharingMode.Shared
            };

            var spi = new SpiDevice(spiReciever);
            var gpio = new GpioController();

            var ok = false;
            while (!ok)
            {
                try
                {
                    reciever = new SX127XDevice(spi, gpio, dio0Pin: recieverDI00, resetPin: recieverReset);
                    reciever.Initialize(
                        Frequency,
                        lnaGain: RegLnaLnaGain.Default,
                        lnaBoost: true,
                        powerAmplifier: RegPAConfigPASelect.PABoost,
                        rxPayloadCrcOn: true,
                        rxDoneignoreIfCrcMissing: false
                        );

                    Debug.WriteLine("reciever OK!");
                    ok = true;
                }
                catch (Exception)
                {
                    Debug.WriteLine("reciever Ups!");
                    ok = false;
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            reciever.OnReceive += Reciever_OnReceive;
            reciever.Receive();

            //while (true) {
            //    Thread.Sleep(1000);
            //    reciever.InterruptGpioPin_ValueChanged(null, null);
            //}

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Reciever_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                string messageText = UTF8Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

                Log($"RECIEVER: PacketSNR: {e.PacketSnr:0.0}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes \r\n{messageText}");

                //Program.reciever.Send(UTF8Encoding.UTF8.GetBytes("OK"));
                //Program.reciever.Receive();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private static void Reciever_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {
            Program.reciever.Receive();

            Log($"Reciver Envia OK");
        }

        private static void Log(string message)
        {
            Console.WriteLine(string.Format("{0} | {1}", DateTime.UtcNow.ToString("HH:mm:ss"), message));
        }
    }
}
