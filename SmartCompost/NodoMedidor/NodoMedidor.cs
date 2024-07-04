using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
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
        public override string IdSmartCompost => "FIUBA-N00000001";
        public override TiposNodo tipoNodo => TiposNodo.Medidor;

        private static ModuloBlinkLed blinker;
        private LoRaDevice lora;
        private ClienteLora cliente;

        private MacAddress direccionRouter;
        public override void Setup()
        {
            blinker = new ModuloBlinkLed(400);
            blinker.Iniciar();

            // Conectamos a LoRa
            ContectarLora();

            cliente = new ClienteLora(lora, this.MacAddress);

            direccionRouter = new MacAddress("B0:A7:32:DD:1E:F4");

            // Detenemos el blinker para avisar que esta todo OK
            blinker.Detener();
        }

        private void ContectarLora()
        {
            Logger.Log($"Conectando LoRa:");
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
        }

        public override void Loop(ref bool activo)
        {
            cliente.Enviar("hola", direccionRouter);
            blinker.BlinkOnce(100);
            Thread.Sleep(4900);
        }

        public override void Dispose()
        {
            blinker.Dispose();
            base.Dispose();
        }
    }
}
