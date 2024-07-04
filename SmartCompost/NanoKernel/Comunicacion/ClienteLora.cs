using devMobile.IoT.SX127xLoRaDevice;
using NanoKernel.Ayudantes;
using NanoKernel.LoRa;
using System;
using System.IO;
using System.Text;

namespace NanoKernel.Comunicacion
{
    public class ClienteLora
    {
        private readonly LoRaDevice lora;

        private readonly Paquete paqueteBuffer;

        private readonly byte[] buffer = new byte[128];

        public ClienteLora(LoRaDevice lora, MacAddress direccionLocal)
        {
            this.lora = lora;

            this.lora.OnReceive += Lora_OnReceive;
            this.lora.OnTransmit += Lora_OnTransmit;

            paqueteBuffer = new Paquete(direccionLocal);
        }

        public void Enviar(string texto, MacAddress destino)
        { 
            Enviar(UTF8Encoding.UTF8.GetBytes(texto), destino, TipoPaqueteEnum.Texto);
        }

        private void Enviar(byte[] datos, MacAddress destino, TipoPaqueteEnum tipoPaquete)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                paqueteBuffer.TipoPaquete = (byte)tipoPaquete;
                paqueteBuffer.MacDestino = destino;
                paqueteBuffer.Payload = datos;
                paqueteBuffer.Empaquetar(ms);
                this.lora.Enviar(ms.ToArray());
            }
        }

        private void Lora_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {

        }

        private void Lora_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {

        }
    }
}
