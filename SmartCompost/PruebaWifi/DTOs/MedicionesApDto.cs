using System;
using System.Collections;

namespace PruebaWifi
{
    public class MedicionesApDto
    {
        public MedicionesApDto(DateTime last_updated, ArrayList nodes_measurements)
        {
            this.last_updated = last_updated;
            this.nodes_measurements = nodes_measurements;
        }

        public DateTime last_updated { get; }
        public ArrayList nodes_measurements { get; }

        public static MedicionesApDto Demo(int nodos = 3, int mediciones = 3)
        {
            var res = new MedicionesApDto(DateTime.UtcNow, new ArrayList());

            for (int i = 0; i < nodos; i++)
            {
                MedicionesNodoDto m = new MedicionesNodoDto("b2c40a98-5534-11ef-92ae-0242ac140004", DateTime.UtcNow, new ArrayList());

                for (int j = 0; j < mediciones; j++)
                {
                    MedicionDto med = new MedicionDto(j * i, DateTime.UtcNow, "typ");
                    m.measurements.Add(med);
                }
                res.nodes_measurements.Add(m);
            }

            return res;
        }
    }
}
