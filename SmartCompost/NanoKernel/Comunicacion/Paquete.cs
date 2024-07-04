using NanoKernel.Ayudantes;
using System;
using System.IO;

namespace NanoKernel.Comunicacion
{
    public enum TipoPaqueteEnum
    {
        // De control, para futuro collision avoidance
        CLR = 0,
        RTS = 1,
        // De payload
        Texto = 10,
        Json = 11,
        Medicion = 12,
    }

    public class Paquete : IDisposable
    {
        public byte TipoPaquete { get; set; }
        public MacAddress MacOrigen { get; set; }
        public MacAddress MacDestino { get; set; }
        public ushort TamanioPayload { get; set; }
        public byte[] Payload { get; set; }

        public Paquete(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                this.TipoPaquete = br.ReadByte();
                this.MacOrigen = new MacAddress(br.ReadBytes(6));
                this.MacDestino = new MacAddress(br.ReadBytes(6));
                this.TamanioPayload = br.ReadUInt16();
                this.Payload = br.ReadBytes(TamanioPayload);
            }
        }

        public Paquete(MacAddress MacOrigen, TipoPaqueteEnum tipoPaquete = TipoPaqueteEnum.Medicion)
        {
            this.MacOrigen = MacOrigen;
            this.MacDestino = MacDestino;
            this.TipoPaquete = (byte)tipoPaquete;
        }

        public void Empaquetar(MemoryStream ms)
        {
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(TipoPaquete);
            bw.Write(MacOrigen.Address);
            bw.Write(MacDestino.Address);
            bw.Write(Payload);

            if (Payload.Length > ushort.MaxValue)
                throw new Exception("Paquete excede tamaño maximo de " + ushort.MaxValue.FormatearBytes());

            bw.Write((ushort)Payload.Length);
        }

        public void Dispose()
        {
            this.Payload = null;
        }

        public override string ToString()
        {
            return $"Origen: {MacOrigen} Destino: {MacDestino} Datos: {Payload.Length}";
        }
    }
}
