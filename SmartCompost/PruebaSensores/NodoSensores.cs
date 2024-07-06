using Iot.Device.Ds18b20;
using nanoFramework.Device.OneWire;
using NanoKernel.Nodos;

namespace NodoAP
{
    public class NodoSensores : NodoBase
    {
        public override string IdSmartCompost => "AP-FIUBA-AP00000001";
        public override TiposNodo tipoNodo => TiposNodo.AccessPoint;

        public override void Setup()
        {
            OneWireHost oneWire = new OneWireHost();
            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

        }

        public override void Loop(ref bool activo)
        {

        }
    }

}
