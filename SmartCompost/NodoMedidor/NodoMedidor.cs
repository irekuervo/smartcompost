using NanoKernel.Ayudantes;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Modulos;
using NanoKernel.Nodos;
using System;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override string IdSmartCompost => "NODO-FIUBA-00000001";
        public override TiposNodo tipoNodo => TiposNodo.Medidor;

        private static ModuloBlinkLed blinker;
        private LoRa lora;
        private MacAddress macAddress;
        private MacAddress macAddressAP;
        public override void Setup()
        {
            blinker = new ModuloBlinkLed();
            blinker.Iniciar(400);

            // Conectamos a LoRa
            Logger.Log($"Conectando LoRa:");
            bool ok = false;
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

            macAddress = ayInternet.GetMacAddress();
            macAddressAP = new MacAddress(new byte[] { 176, 167, 50, 221, 15, 148 });
            paquete = new byte[12];
            // Detenemos el blinker para avisar que esta todo OK
            blinker.Detener();
        }

        byte[] paquete;
        public override void Loop(ref bool activo)
        {
            blinker.High();

            // devolvemos el Paquete OK = id destino + id origen
            Array.Copy(macAddressAP.Address, 0, paquete, 0, macAddressAP.Address.Length);
            Array.Copy(macAddress.Address, 0, paquete, 6, macAddress.Address.Length);

            // Ahora esta pensado para que sea un ping pong que empieza este nodo
            lora.Enviar(paquete);
            blinker.Low();

            Thread.Sleep(1000);
        }

        public override void Dispose()
        {
            blinker.Dispose();
            base.Dispose();
        }

        //public static void Main()
        //{
        //    //Nodo.Iniciar();

        //    blinker = new ModuloBlinkLed();
        //    blinker.Iniciar();

        //    sensor = new ModuloSensor();

        //    var medicion = sensor.Medir();
        //    Logger.Log("Enviando medicion: " + medicion);
        //    Thread.Sleep(2000);

        //   // aySleep.DeepSleepSegundos(10);

        //    //// LO PUSE ACA POR QUE NO SABIA DONDE
        //    // Ejecutamos una tarea para mandar a deep sleep
        //    //MotorDeHilos.CrearHilo("DeepSleep", (ref bool a) =>
        //    //{
        //    //    Thread.Sleep(20_000);
        //    //    aySleep.DeepSleepSegundos(60 * 60);
        //    //}).Iniciar();
        //}



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
