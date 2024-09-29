using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using System;
using System.Collections;
using System.IO;

namespace NanoKernel.DTOs
{
    public class MedicionesNodoDto
    {
        public string serial_number { get; set; }
        public DateTime last_updated { get; set; }
        public ArrayList measurements { get; set; } = new ArrayList();

        public void AgregarMedicion(float value, TiposMediciones tipo)
        {
            measurements.Add(new MedicionDto() { value = value, timestamp = DateTime.UtcNow, type = tipo.GetString() });
        }

        public long ToBytes(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                return Serialize(ms);
            }
        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serialize(ms);
                return ms.ToArray();
            }
        }

        private long Serialize(MemoryStream ms)
        {
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)TipoPaqueteEnum.MedicionNodo);
            bw.Write(serial_number);
            bw.Write(last_updated.Ticks);
            bw.Write((ushort)measurements.Count);
            foreach (MedicionDto item in measurements)
            {
                bw.Write(item.value);
                bw.Write(item.timestamp.Ticks);
                bw.Write(item.type);
            }
            return ms.Position;
        }

        public static MedicionesNodoDto FromBytes(byte[] data)
        {
            if (data == null) throw new Exception("Data es null");

            var medicionesNodoDto = new MedicionesNodoDto();

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                if (tipoPaquete != TipoPaqueteEnum.MedicionNodo)
                    throw new Exception("Paquete no es medicion nodo");

                medicionesNodoDto.serial_number = br.ReadString();
                medicionesNodoDto.last_updated = new DateTime(br.ReadInt64());

                var mediciones = br.ReadUInt16();
                for (int i = 0; i < mediciones; i++)
                {
                    MedicionDto m = new MedicionDto();
                    m.value = br.ReadSingle();
                    m.timestamp = new DateTime(br.ReadInt64());
                    // TODO: usamos la misma fecha para las mediciones ya que los nodos no tienen RTC
                    m.timestamp = medicionesNodoDto.last_updated;
                    m.type = br.ReadString();
                    medicionesNodoDto.measurements.Add(m);
                }

                return medicionesNodoDto;
            }
        }
    }

    public class MedicionDto
    {
        public float value { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; }

        public void Medir(float value)
        {
            this.value = value;
            this.timestamp = DateTime.UtcNow;
        }
    }
}
