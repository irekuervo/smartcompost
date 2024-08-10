using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.MedidorLora;

        // ---------------------------------------------------------------
        private const bool ES_PROTOBOARD = true; // Tiene otro pinout, y se porta distinto para hacer purebas
        private const bool ES_SUPERMINI = false; //solo para protoboard
        private const int segundosSleep = 15;
        private const int milisLoop = 1000;
        // ---------------------------------------------------------------
        private Random random = new Random();
        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private MedicionesNodoDto dto;

        private const int pinLedOnboard = 2;
        private const int pinLedOnboardMini = 1;

        private readonly byte[] buffer = new byte[128];

        private readonly string[] codigos = new string[]{
            "b2c40a98-5534-11ef-92ae-0242ac140004",
            "282a2047-5668-11ef-92ae-0242ac140004",
            "2cbf5e3f-5668-11ef-92ae-0242ac140004" };

        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            if (ES_PROTOBOARD)
                led = gpio.OpenPin(pinLedOnboard, PinMode.Output);
            else
                led = gpio.OpenPin(pinLedOnboardMini, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            Hilo.Intentar(() =>
            {
                if (ES_PROTOBOARD)
                {
                    if (ES_SUPERMINI) lora = new LoRaDevice(
                        pinMISO: 9,
                        pinMOSI: 10,
                        pinCLOCK: 8,
                        pinSlaveSelect: 5,
                        pinLoraDatos: 4,
                        pinLoraReset: 3);
                    else lora = new LoRaDevice(pinLoraDatos: 4, pinLoraReset: 15);
                }
                else
                    lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora");

            dto = new MedicionesNodoDto() { serial_number = Config.NumeroSerie };

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }


        public override void Loop(ref bool activo)
        {
            try
            {
                // TODO: MEDIR DE VERDAD
                float bateria = (float)random.NextDouble() * 100;
                float temperatura = (float)random.NextDouble() * 5 + 25;
                float humedad = (float)random.NextDouble() * 100;

                if (ES_PROTOBOARD)
                    dto.serial_number = codigos[random.Next(3)]; // simulamos un nodo random

                dto.AgregarMedicion(bateria, TiposMediciones.Bateria);
                dto.AgregarMedicion(temperatura, TiposMediciones.Temperatura);
                dto.AgregarMedicion(humedad, TiposMediciones.Humedad);
                dto.last_updated = DateTime.UtcNow;

                long length = dto.ToBytes(buffer);
                lora.Enviar(buffer, 0, (int)length);

                Logger.Debug($"Paquete {dto.last_updated.Ticks} enviado, {length} bytes");
                Blink();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                dto.measurements.Clear();

                LimpiarMemoria();

                if (ES_PROTOBOARD)
                    Thread.Sleep(milisLoop);
                else
                    aySleep.DeepSleepSegundos(segundosSleep);
            }
        }

        private void Blink(int milis = 100)
        {
#if DEBUG
            led.Write(PinValue.High);
            Thread.Sleep(milis);
            led.Write(PinValue.Low);
#endif
        }
    }
}
