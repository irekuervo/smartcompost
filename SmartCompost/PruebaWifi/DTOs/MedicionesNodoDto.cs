using System;
using System.Collections;

namespace PruebaWifi
{
    public enum TipoPaqueteEnum
    {
        // De control, para futuro collision avoidance
        CLR = 0,
        RTS = 1,
        // De payload
        Texto = 10,
        Json = 11,
        MedicionNodo = 12,
    }

    public class MedicionesNodoDto
    {
        public string serial_number { get; }
        public DateTime last_updated { get; }
        public ArrayList measurements { get; }

        public MedicionesNodoDto(string serial_number, DateTime last_updated, ArrayList measurements)
        {
            this.serial_number = serial_number;
            this.last_updated = last_updated;
            this.measurements = measurements;
        }
    }

    public class MedicionDto
    {
        public float value { get; }
        public DateTime timestamp { get; }
        public string type { get; }

        public MedicionDto(float value, DateTime timestamp, string type)
        {
            this.value = value;
            this.timestamp = timestamp;
            this.type = type;
        }
    }
}
