using NanoKernel;
using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.Modulos;
using NanoKernel.Nodos;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor
    {
        private static ModuloBlinkLed blinker;
        private static ModuloSensor sensor;
        public static void Main()
        {
            //Nodo.Iniciar();

            blinker = new ModuloBlinkLed();
            blinker.Iniciar();

            sensor = new ModuloSensor();

            var medicion = sensor.Medir();
            Logger.Log("Enviando medicion: " + medicion);
            Thread.Sleep(2000);

            aySleep.DeepSleepSegundos(10);

            //// LO PUSE ACA POR QUE NO SABIA DONDE
            // Ejecutamos una tarea para mandar a deep sleep
            //MotorDeHilos.CrearHilo("DeepSleep", (ref bool a) =>
            //{
            //    Thread.Sleep(20_000);
            //    aySleep.DeepSleepSegundos(60 * 60);
            //}).Iniciar();
        }

        private static void Setup()
        {
           
        }

        #region Pruebas viejas

        //const int idMock = 2; // del 1 al 4 tenemos cargado
        //private static void Loop(ref bool hiloActivo)
        //{
        //    Thread.Sleep(1000);

        //    try
        //    {
        //        if (ayInternet.Hay)
        //        {


        //            // Mando medicion
        //            var res = ayInternet.EnviarJson(
        //                "http://smartcompost.net:8080/api/compost_bins/add_measurement",
        //                sensor.Medir(idMock));
        //            Logger.Log(res);

        //            // Busco fecha utc
        //            Logger.Log(ayFechas.GetNetworkTime().ToFechaLocal());
        //        }

        //    }
        //    catch (System.Exception)
        //    {
        //        Logger.Log("Upsi");
        //    }
        //}

        //private static void LoopDeepSleep(ref bool hiloActivo)
        //{
        //    Thread.Sleep(5000);
        //    aySleep.DeepSleep(10);
        //}

        //static Medidor medidor = new Medidor();
        //private static void LoopMedidor(ref bool hiloActivo)
        //{
        //    medidor.IniciarMedicionDeTiempo();
        //    Thread.Sleep(150);
        //    //CUENTA MEDIO RARO EL TIEMPO
        //    //Thread.Sleep(1000); 
        //    medidor.FinalizarMedicionDeTiempo();
        //    Thread.Sleep(10);
        //    medidor.Contar("vueltas-loop");
        //}

        //private static void EjemploBaseDeDatosClaveValor()
        //{
        //    IRepositorioClaveValor repo = new RepositorioClaveValorInterno("base2");

        //    repo.Update("clave1", "hola");

        //    var valor = repo.Get("clave1");

        //    Thread.Sleep(Timeout.Infinite);
        //}

        //private static void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        //{
        //    float tiempoPromedio = resultado.MedicionTiempoMilis.MedicionEnPeriodo.Promedio();
        //    Logger.Log("Tiempo promedio medido: " + tiempoPromedio.MilisToTiempo());
        //    Logger.Log("Vueltas loop: " + resultado.ContadoTotal("vueltas-loop"));
        //}
        #endregion
    }
}
