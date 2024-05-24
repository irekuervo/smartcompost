using NanoKernel;
using NanoKernel.Ayudantes;
using NanoKernel.Hilos;
using NanoKernel.Loggin;
using NanoKernel.Medidores;
using NanoKernel.Modulos;
using NanoKernel.Repositorios;
using System.Diagnostics;
using System.Threading;

namespace NodoMedidor
{

    public class MainNodo
    {
        public static void Main()
        {
            App.Start(Setup, Loop);
        }

        private static ModuloBlinkLed blinker;
        private static ModuloSensor sensor;
        private static void Setup()
        {
            // Hacemos titilar el led del board
            blinker = new ModuloBlinkLed();
            blinker.Iniciar(1000);

            sensor = new ModuloSensor();

            // Sacamos los datos de la memoria interna
            IRepositorioClaveValor repo = new RepositorioClaveValorInterno("config");
            var ssid = repo.Get("wifi-ssid");
            var pass = repo.Get("wifi-pass");

            if (ssid == null) repo.Update("wifi-ssid", "La Gorda");
            if (pass == null) repo.Update("wifi-pass", "comandante123");

            ssid = repo.Get("wifi-ssid");
            pass = repo.Get("wifi-pass");

            // Conectamos a wifi
            Logger.Log("Conectando '" + ssid + "' pass: " + pass);
            while (!ayInternet.ConectarsePorWifi(ssid, pass))
            {
                Debug.Write(".");
                Thread.Sleep(1000);
            }
            blinker.CambiarPeriodo(500);
            Debug.WriteLine("Conectado");

            // Ejecutamos una tarea para mandar a deep sleep
            MotorDeHilos.CrearHilo("DeepSleep", (ref bool a) =>
            {
                Thread.Sleep(20_000);
                aySleep.DeepSleepSegundos(60 * 60);
            }).Iniciar();
        }


        const int idMock = 2; // del 1 al 4 tenemos cargado
        private static void Loop(ref bool hiloActivo)
        {
            Thread.Sleep(1000);

            try
            {
                if (ayInternet.Hay)
                {


                    // Mando medicion
                    var res = ayInternet.EnviarJson(
                        "http://smartcompost.net:8080/api/compost_bins/add_measurement",
                        sensor.Medir(idMock));
                    Logger.Log(res);

                    // Busco fecha utc
                    Logger.Log(ayFechas.GetNetworkTime().ToFechaLocal());
                }

            }
            catch (System.Exception)
            {
                Logger.Log("Upsi");
            }
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
