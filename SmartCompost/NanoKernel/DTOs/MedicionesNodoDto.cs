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

        public byte[] ToBytes(MemoryStream buffer)
        {
            BinaryWriter bw = new BinaryWriter(buffer);

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

            return buffer.ToArray();
        }

        public static MedicionesNodoDto FromBytes(byte[] data)
        {
            MedicionesNodoDto medicionesNodoDto = new MedicionesNodoDto();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                if (tipoPaquete != TipoPaqueteEnum.MedicionNodo)
                    return null;

                medicionesNodoDto.serial_number = br.ReadString();
                medicionesNodoDto.last_updated = new DateTime(br.ReadInt64());

                var mediciones = br.ReadUInt16();
                for (int i = 0; i < mediciones; i++)
                {
                    MedicionDto m = new MedicionDto();
                    m.value = br.ReadSingle();
                    br.ReadInt64(); // Leemos el valor para avanzar el buffer, pero no nos sirve la fecha que viene
                    m.timestamp = DateTime.UtcNow; // Mentimos y decimos que la fecha es esta, le pifiamos por pocos segundos
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
    }
}
