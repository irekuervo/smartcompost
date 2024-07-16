using NanoKernel.Herramientas.Estadisticas;

namespace NanoKernel.Herramientas.Medidores
{
    public class Medicion
    {
        public Medicion()
        {
            MedicionEnPeriodo = new EstadisticaEscalar();
            MedicionTotal = new EstadisticaEscalar();
        }

        public EstadisticaEscalar MedicionEnPeriodo { get; set; }
        public EstadisticaEscalar MedicionTotal { get; set; }

        public void AgregarMuestra(float muestra)
        {
            // Por ahora no usamos el histograma
            MedicionEnPeriodo.AgregarMuestra(muestra, calcularHistograma: false);
            MedicionTotal.AgregarMuestra(muestra, calcularHistograma: false);
        }

        public Medicion Clonar()
        {
            Medicion res = new Medicion();
            res.MedicionEnPeriodo = MedicionEnPeriodo.Clonar();
            res.MedicionTotal = MedicionTotal.Clonar();
            return res;
        }

        public void Limpiar()
        {
            MedicionEnPeriodo.Limpiar();
            MedicionTotal.Limpiar();
        }
    }
}
