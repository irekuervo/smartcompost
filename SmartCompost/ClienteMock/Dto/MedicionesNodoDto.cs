namespace MockSmartcompost.Dto
{
    public class MedicionesNodoDto
    {
        public string serial_number { get; set; }
        public DateTime last_updated { get; set; }
        public List<Medicion> measurements { get; set; } = new List<Medicion>();
    }

    public class Medicion
    {
        public float value { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; }
    }
}
