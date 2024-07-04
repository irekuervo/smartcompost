using NanoKernel.Ayudantes;
using System.IO;

namespace NanoKernel.Comunicacion
{
    public class PayloadMedicion
    {
        public ushort LargoNodoId { get; set; }
        public string NodoId { get; set; }
        public float Medicion { get; set; }

        public PayloadMedicion()
        {
        }

        public PayloadMedicion(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                this.LargoNodoId = br.ReadByte();
                this.NodoId = br.ReadString(LargoNodoId);
                this.Medicion = br.ReadSingle();
            }
        }

        public void Empaquetar(MemoryStream ms)
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(LargoNodoId);
                bw.Write(NodoId);
                bw.Write(Medicion);
            }
        }
    }
}
