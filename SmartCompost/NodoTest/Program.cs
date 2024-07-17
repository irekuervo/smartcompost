using NanoKernel.Ayudantes;
using NanoKernel.Herramientas.Medidores;
using NanoKernel.Herramientas.Repositorios;
using NanoKernel.Logging;
using NanoKernel.Modulos;
using System.Threading;

namespace NodoTest
{
    public class Program
    {
        static ModuloBlinkLed blinker;
        public static void Main()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar();
            Thread.Sleep(Timeout.Infinite);
        }

        static void setup()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar();
        }

        static void loop(ref bool activo)
        {
            Thread.Sleep(1000);
        }

        #region Pruebas viejas

        private static void LoopDeepSleep(ref bool hiloActivo)
        {
            Thread.Sleep(5000);
            aySleep.DeepSleepSegundos(10);
        }

        static Medidor medidor = new Medidor();

        private static void LoopMedidor(ref bool hiloActivo)
        {
            medidor.IniciarMedicionDeTiempo();
            Thread.Sleep(150);
            //CUENTA MEDIO RARO EL TIEMPO
            //Thread.Sleep(1000); 
            medidor.FinalizarMedicionDeTiempo();
            Thread.Sleep(10);
            medidor.Contar("vueltas-loop");
        }

        private static void EjemploBaseDeDatosClaveValor()
        {
            IRepositorioClaveValor repo = new RepositorioClaveValorInterno("base2");

            repo.Update("clave1", "hola");

            var valor = repo.Get("clave1");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        {
            float tiempoPromedio = resultado.MedicionTiempoMilis.MedicionEnPeriodo.Promedio();
            Logger.Log("Tiempo promedio medido: " + tiempoPromedio.MilisToTiempo());
            Logger.Log("Vueltas loop: " + resultado.ContadoTotal("vueltas-loop"));
        }
        #endregion
    }
}
