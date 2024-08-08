using System;
using System.Collections;

namespace NanoKernel.DTOs
{
    public class MedicionesApDto
    {
        public DateTime last_updated { get; set; }
        public ArrayList nodes_measurements { get; set; } = new ArrayList();


        public void AgregarMedicion(string numeroSerie, MedicionDto medicion)
        {
            var nodo = BuscarMedicion(numeroSerie);
            if (nodo == null)
            {
                nodo = new MedicionesNodoDto() { serial_number = numeroSerie };
                nodes_measurements.Add(nodo);
            }
            nodo.measurements.Add(medicion);

            nodo.last_updated = DateTime.UtcNow;
        }

        public void AgregarMediciones(MedicionesNodoDto medicionesNodo)
        {
            if (medicionesNodo == null)
                return;

            var nodo = BuscarMedicion(medicionesNodo.serial_number);
            if (nodo == null)
            {
                nodes_measurements.Add(medicionesNodo);
            }
            else
            {
                foreach (var item in medicionesNodo.measurements)
                {
                    nodo.measurements.Add(item);
                }
            }
        }

        private MedicionesNodoDto BuscarMedicion(string numeroSerial)
        {
            foreach (var item in nodes_measurements)
            {
                var nodo = item as MedicionesNodoDto;
                if (nodo != null && nodo.serial_number == numeroSerial)
                    return nodo;
            }

            return null;
        }
    }
}
