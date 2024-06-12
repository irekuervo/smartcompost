using devMobile.IoT.SX127xLoRaDevice;
using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using System;
using System.IO;

namespace NanoKernel.LoRa
{
    public class ServidorLoRa : IDisposable
    {
        private readonly MacAddress id;

        private readonly LoRa lora;


        private readonly byte[] paquete;
        public ServidorLoRa(MacAddress id, LoRa lora)
        {
            this.id = id;
            this.lora = lora;

            lora.OnReceive += Lora_OnReceive;
            lora.OnTransmit += Lora_OnTransmit;

            paquete = new byte[12];

        }

        private void Lora_OnTransmit(object sender, SX127XDevice.OnDataTransmitedEventArgs e)
        {

        }

        private void Lora_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            using (MemoryStream memoryStream = new MemoryStream(e.Data))
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                byte[] id = reader.ReadBytes(6);

                // Esto seria revisar que el id destino sea el id local
                if (this.id.Address.IsEqualsTo(id) == false)
                {
                    Logger.Log("Ups");
                    // Descarto el paquete
                    return;
                }

                byte[] idSender = reader.ReadBytes(6);

                // devolvemos el Paquete OK = id destino + id origen
                Array.Copy(idSender, 0, paquete, 0, idSender.Length);
                Array.Copy(id, 0, paquete, 6, id.Length);
                Logger.Log("Ok");
                lora.Enviar(paquete);
            }
        }

        public void Iniciar()
        {
            lora.Iniciar();
        }

        public void Dispose()
        {
            
        }
    }
}
