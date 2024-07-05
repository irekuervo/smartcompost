using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Loggin;
using NanoKernel.LoRa;
using NanoKernel.Modulos;
using NanoKernel.Nodos;
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
        private LoRaDevice lora;
        private RouterLoraWifi router;

        public override void Setup()
        {
            blinker = new ModuloBlinkLed(400);
            blinker.Iniciar();

            ConectarWifi();

            ConectarLora();

            IniciarRouter();

            blinker.Detener();
        }

        private static void ConectarWifi()
        {
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

            Logger.Log($"OK");
        }

        private void ConectarLora()
        {
            Logger.Log($"Conectando LoRa...");
            bool ok = false;
            while (!ok)
            {
                try
                {
                    lora = new LoRaDevice();
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
            Logger.Log($"OK");
        }

        private void IniciarRouter()
        {
            Logger.Log($"Iniciando router...");
            router = new RouterLoraWifi(lora, blinker, this.MacAddress);
            Logger.Log($"OK");
        }

        public override void Dispose()
        {
            router.Dispose();
            blinker.Dispose();
            base.Dispose();
        }
    }
}
