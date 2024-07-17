using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Hilos;
using NanoKernel.Loggin;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override string IdSmartCompost => "Medidor";
        public override TiposNodo tipoNodo => TiposNodo.Medidor;

        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private uint paquete = 1;
        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            lora = new LoRaDevice();
            lora.OnReceive += Device_OnReceive;
            lora.OnTransmit += Device_OnTransmit;
            // Intentamos conectarnos al lora
            Hilo.Intentar(() => lora.Iniciar(), "Lora");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        // Este loop se corre una sola vez, por el deep sleep
        public override void Loop(ref bool activo)
        {
            var ran = new Random();
            lora.Enviar(Encoding.UTF8.GetBytes($"[{paquete++}] Temp:{ran.NextDouble() * 5 + 30}"));
            Blink(100);
            Thread.Sleep(900);
            aySleep.DeepSleepSegundos(15, "pinto loco");
        }

        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                led.Write(PinValue.High);
                Thread.Sleep(100);
                led.Write(PinValue.Low);

                Logger.Log($"PacketSNR: {e.PacketSnr}, Packet RSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private void Device_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {
            Logger.Log("Se envio el paquete " + paquete);
        }

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }
    }
}
