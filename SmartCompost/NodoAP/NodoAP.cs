using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Medidores;
using NanoKernel.Modulos;
using NanoKernel.Nodos;
using NanoKernel.Repositorios;
using System;
using System.Threading;

namespace NodoAP
{
    public class NodoAP : NodoBase
    {
        public override string IdSmartCompost => "AP-FIUBA-AP00000001";
        public override TiposNodo tipoNodo => TiposNodo.AccessPoint;

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
            Logger.Log($"Conectando LoRa:");
            ok = false;
            while (!ok)
            {
                try
                {
                    lora = new LoRa();
                    lora.OnReceive += Lora_OnReceive;
                    lora.OnTransmit += Lora_OnTransmit;
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

            // Detenemos el blinker para avisar que esta todo OK
            blinker.Detener();
        }

        int idPaquete = 0;

        private void Lora_OnTransmit(object sender, devMobile.IoT.SX127xLoRaDevice.SX127XDevice.OnDataTransmitedEventArgs e)
        {
            Logger.Log("Enviado id: " + idPaquete);
        }

        private void Lora_OnReceive(object sender, devMobile.IoT.SX127xLoRaDevice.SX127XDevice.OnDataReceivedEventArgs e)
        {
            blinker.High();
            idPaquete = BitConverter.ToInt32(e.Data, 0);
            Logger.Log("Recibido id: " + idPaquete);
            Thread.Sleep(1000);
            lora.Enviar(BitConverter.GetBytes(idPaquete));
            blinker.Low();
        }

        public override void Dispose()
        {
            blinker.Dispose();
            base.Dispose();
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
