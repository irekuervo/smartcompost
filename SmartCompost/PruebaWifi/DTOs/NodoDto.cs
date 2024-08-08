using System;

namespace PruebaWifi
{
    public class NodoDto
    {
        public int id { get; set; }
        public string description { get; set; }
        public string serial_number { get; set; }
        public string model { get; set; }
        public DateTime last_updated { get; set; }
        public DateTime date_created { get; set; }
    }
}
