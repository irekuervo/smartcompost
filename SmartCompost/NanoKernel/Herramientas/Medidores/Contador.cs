namespace NanoKernel.Herramientas.Medidores
{
    public class Contador
    {
        public ulong ContadorEnPeriodo { get; set; } = 0;
        public ulong ContadorTotal { get; set; } = 0;

        public void Contar(ulong cantidad = 1)
        {
            ContadorEnPeriodo += cantidad;
            ContadorTotal += cantidad;
        }

        public Contador Clonar()
        {
            Contador res = new Contador();
            res.ContadorEnPeriodo = ContadorEnPeriodo;
            res.ContadorTotal = ContadorTotal;
            return res;
        }
    }
}
