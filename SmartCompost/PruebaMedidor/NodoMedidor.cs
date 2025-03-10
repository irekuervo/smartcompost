﻿using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

namespace PruebaMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.MedidorLora;

        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private uint paquete = 1;
        public override void Setup()
        {
            Logger.Log("----ACCESS POINT----");
            Logger.Log("");

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
            Hilo.Intentar(() => lora.Iniciar(433e6), "Lora");

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);

            var ran = new Random();
            lora.Enviar(Encoding.UTF8.GetBytes($"[{paquete++}] Temp:{ran.NextDouble() * 5 + 30}"));
            Blink(100);
            Thread.Sleep(900);
            aySleep.DeepSleepSegundos(15, "pinto loco");
        }

        public override void ColaLoop(ref bool activo)
        {
            //lora.Enviar(Encoding.UTF8.GetBytes($"[{paquete++}] Este mensaje es largo para probar cuantos bytes puedo mandar y que parezca que mando mucha info importante"));
            //Blink(100);
            //Thread.Sleep(900);
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
