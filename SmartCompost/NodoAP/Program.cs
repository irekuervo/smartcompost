using NanoKernel.Loggin;
using NanoKernel.LoRa;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NodoAP
{
    public class Program
    {
        static GpioController gpio;
        static GpioPin led;
        static LoRaDevice device;

        public static void Main(string[] args)
        {
            // 14-07-24 ISSUE: NO ANDA LORA
            // ANDA: Pruebas desarrollo
            // NO ANDA: Mi implementacion
            // PLAN: Ir desde el codigo de pruebas hasta el codigo final, y encontrar el error

            // Comento el codigo posta
            //new NodoAP().Iniciar();

            // Escribo desde aca de 0

            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);

            led.Write(PinValue.High);

            var ok = false;
            while (!ok)
            {
                try
                {
                    device = new LoRaDevice();
                    device.Iniciar();
                    Logger.Log("Lora iniciado!");
                    ok = true;
                }
                catch (Exception)
                {
                    Debug.WriteLine("error iniciando lora");
                    ok = false;
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            device.OnReceive += Device_OnReceive;

            led.Write(PinValue.Low);
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Device_OnReceive(object sender, devMobile.IoT.SX127xLoRaDevice.SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                string messageText = UTF8Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);

                led.Write(PinValue.High);
                Thread.Sleep(100);
                led.Write(PinValue.Low);

                Logger.Log($"RECIEVER: PacketSNR: {e.PacketSnr}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes \r\n{messageText}");

                //Program.reciever.Send(UTF8Encoding.UTF8.GetBytes("OK"));
                //Program.reciever.Receive();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
    }
}
