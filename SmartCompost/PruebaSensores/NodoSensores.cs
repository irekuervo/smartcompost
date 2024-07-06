using NanoKernel.Nodos;

namespace NodoAP
{
    public class NodoSensores : NodoBase
    {
        public override string IdSmartCompost => "AP-FIUBA-AP00000001";
        public override TiposNodo tipoNodo => TiposNodo.AccessPoint;

        public override void Setup()
        {

        }

        public override void Loop(ref bool activo)
        {

        }
    }
}
