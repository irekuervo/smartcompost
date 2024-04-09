using NanoKernel.Estadisticas;

namespace NanoKernel.Medidores
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
            res.MedicionEnPeriodo = this.MedicionEnPeriodo.Clonar();
            res.MedicionTotal = this.MedicionTotal.Clonar();
            return res;
        }

        public void Limpiar()
        {
            MedicionEnPeriodo.Limpiar();
            MedicionTotal.Limpiar();
        }
    }
}
