using nanoFramework.Networking;
using NanoKernel;
using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.Medidores;
using NanoKernel.Modulos;
using NanoKernel.Repositorios;
using System.Diagnostics;
using System.Threading;

namespace NodoMedidor
{
    public class Program
    {
        public static void Main()
        {
            App.Start(Setup, Loop);
        }

        static bool hayInternet = false;
        private static void Setup()
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //App.Start(assembly);

            //EjemploBaseDeDatosClaveValor();
            //medidor.OnMedicionesEnPeriodoCallback += Medidor_OnMedicionesEnPeriodoCallback;

            ModuloBlinkLed blink = new ModuloBlinkLed();
            blink.Iniciar(1000);

            // Jugamos probando sacar la configuracion de la eeprom
            IRepositorioClaveValor repo = new RepositorioClaveValorInterno("config");

            //repo.Update("wifi-ssid", "La Gorda");
            //repo.Update("wifi-pass", "comandante123");

            var ssid = repo.Get("wifi-ssid");
            var pass = repo.Get("wifi-pass");
           
            //var ssid = "La Gorda";
            //var pass = "comandante123";

            Logger.Log("Conectando '" + ssid + "' pass: " + pass);

            while (!ayInternet.ConectarsePorWifi(ssid, pass))
            {
                Debug.Write(".");
                Thread.Sleep(1000);
            }

            blink.CambiarPeriodo(500);
            Debug.WriteLine("Conectado");
        }

        private static void Loop(ref bool hiloActivo)
        {
            Thread.Sleep(1000);

            //if (ayInternet.Hay)
            //    Logger.Log(ayFechas.GetNetworkTime().ToFechaLocal());
        }

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
    }
}
