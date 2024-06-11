using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Medidores;
using NanoKernel.Modulos;
using NanoKernel.Nodos;
using NanoKernel.Repositorios;
using System;
using System.Diagnostics;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : Nodo
    {
        public override string IdSmartCompost => "FIUBA-00000001";
        public override TiposNodo tipoNodo => TiposNodo.LoraAP;

        private const string WIFI_SSID = "Bondiola 2.4";
        private const string WIFI_PASS = "comandante123";

        private ModuloBlinkLed blinker;
        private LoRa lora;

        public override void Setup()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar(400);

            // Conectamos a wifi
            Logger.Log($"Conectando WiFi: {WIFI_SSID}-{WIFI_PASS}");
            bool ok = false;
            while (!ok)
            {
                try
                {
                    ok = ayInternet.ConectarsePorWifi(WIFI_SSID, WIFI_PASS);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            // Conectamos a LoRa
            Logger.Log($"Conectando LoRa: {WIFI_SSID}-{WIFI_PASS}");
            ok = false;
            while (!ok)
            {
                try
                {
                    lora = new LoRa();
                    lora.Iniciar();
                    ok = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }

            blinker.Detener();
        }


        //const int idMock = 2; // del 1 al 4 tenemos cargado
        //private static void Loop(ref bool hiloActivo)
        //{
        //    Thread.Sleep(1000);

        //    try
        //    {
        //        if (ayInternet.Hay)
        //        {
        //            // idNodo: Codigo de fabrica SmartCompost definida por nosotros

        //            //Mando medicion
        //            //var res = ayInternet.EnviarJson(
        //            //    "http://smartcompost.net:8080/api/{idNodo}/add_measurement",
        //            //    sensor.Medir(idMock));
        //            //Logger.Log(res);

        //            // Busco fecha utc
        //            Logger.Log(ayFechas.GetNetworkTime().ToFechaLocal());
        //        }

        //    }
        //    catch (System.Exception)
        //    {
        //        Logger.Log("Upsi");
        //    }
        //}

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
