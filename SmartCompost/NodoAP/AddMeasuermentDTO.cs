using System;

namespace NodoAP
{
    public class AddMeasuermentDTO
    {
        public string ip_id { get; set; }
        public DateTime ap_datetime { get; set; }
        public float ap_battery_level { get; set; }
        public string node_id { get; set; }

        public Array node_measurments { get; set; }
    }

    public class NodeMeasuermentDTO
    {
        public DateTime datetime { get; set; }
        public string type { get; set; }
        public float value { get; set; }
    }
}
