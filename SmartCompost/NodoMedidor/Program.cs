using NanoKernel;
using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.Medidores;
using NanoKernel.Repositorios;
using System.Threading;

namespace NodoMedidor
{
    public class Program
    {
        static Medidor medidor = new Medidor();

        public static void Main()
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //App.Start(assembly);

            IRepositorioDocumental repo = new RepositorioDocumentalInterno("base1");

            repo.Update("clave1", "hola");

            var valor = repo.Get("clave1");

            Thread.Sleep(Timeout.Infinite);


            medidor.OnMedicionesEnPeriodoCallback += Medidor_OnMedicionesEnPeriodoCallback;

            App.Loop += Loop;
            App.Start();
        }

        private static void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        {
            float tiempoPromedio = resultado.MedicionTiempoMilis.MedicionEnPeriodo.Promedio();
            Logger.Log("Tiempo promedio medido: " + tiempoPromedio.MilisToTiempo());
            Logger.Log("Vueltas loop: " + resultado.ContadoTotal("vueltas-loop"));
        }

        private static void Loop(ref bool hiloActivo)
        {
            medidor.IniciarMedicionDeTiempo();
            Thread.Sleep(150);
            //CUENTA MEDIO RARO EL TIEMPO
            //Thread.Sleep(1000); 
            medidor.FinalizarMedicionDeTiempo();
            Thread.Sleep(10);
            medidor.Contar("vueltas-loop");
        }
    }
}
