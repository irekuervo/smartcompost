using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using System;
using System.Collections;
using System.IO;

namespace NanoKernel.DTOs
{
    public class ApMedicionesDto
    {
        public DateTime last_updated { get; set; }
        public ArrayList nodes { get; set; } = new ArrayList();


        public bool AgregarMediciones(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                var numeroSerie = br.ReadString();
                var ticksMedicion = br.ReadInt64(); //Es la fecha, pero no la usamos porque no viene correcto
                var bateria = br.ReadSingle();
                var temperatura = br.ReadSingle();
                var humedad = br.ReadSingle();

                var nodo = BuscarMedicion(numeroSerie);
                if (nodo == null)
                    nodo = new MedicionesNodoDto() { serial_number = numeroSerie };

                var now = DateTime.UtcNow;

                nodo.measurements.Add(new Medicion()
                {
                    timestamp = now,
                    type = TiposMediciones.TIPO_BATERIA,
                    value = bateria
                });
                nodo.measurements.Add(new Medicion()
                {
                    timestamp = now,
                    type = TiposMediciones.TIPO_TEMPERATURA,
                    value = temperatura
                });
                nodo.measurements.Add(new Medicion()
                {
                    timestamp = now,
                    type = TiposMediciones.TIPO_HUMEDAD,
                    value = humedad
                });
            }

            return true;
        }

        private MedicionesNodoDto BuscarMedicion(string numeroSerial)
        {
            foreach (var item in nodes)
            {
                var nodo = item as MedicionesNodoDto;
                if (nodo != null && nodo.serial_number == numeroSerial)
                    return nodo;
            }

            return null;
        }
    }
}
