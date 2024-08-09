using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using NanoKernel.Logging;
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
        }

        public static MedicionesNodoDto FromBytes(byte[] data)
        {
            if (data == null) return null;

            try
            {
                var medicionesNodoDto = new MedicionesNodoDto();

                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                    if (tipoPaquete != TipoPaqueteEnum.MedicionNodo)
                        return null;

                    medicionesNodoDto.serial_number = br.ReadString();
                    medicionesNodoDto.serial_number = "b2c40a98-5534-11ef-92ae-0242ac140004"; // HARDCODEADO PARA PROBAR
                    medicionesNodoDto.last_updated = new DateTime(br.ReadInt64());

                    var mediciones = br.ReadUInt16();
                    for (int i = 0; i < mediciones; i++)
                    {
                        MedicionDto m = new MedicionDto();
                        m.value = br.ReadSingle();
                        br.ReadInt64(); // Leemos el valor para avanzar el buffer, pero no nos sirve la fecha que viene

                        // TODO: deberia venir del lora, mentimos y le pifiamos por poco
                        m.timestamp = DateTime.UtcNow;

                        m.type = br.ReadString();
                        medicionesNodoDto.measurements.Add(m);
                    }

                    return medicionesNodoDto;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
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
