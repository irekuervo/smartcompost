using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion.Old;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Device.Gpio;
using System.IO;
using System.Threading;

namespace NodoMedidor
{
    public class NodoMedidor : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.MedidorLora;

        // ---------------------------------------------------------------
        private const bool DEEPSLEEP = false;
        private const bool ES_PROTOBOARD = true;
        private const int segundosSleep = 15;
        private const int milisLoop = 1000;
        // ---------------------------------------------------------------
        private Random random = new Random();
        private GpioController gpio;
        private GpioPin led;
        private LoRaDevice lora;
        private MedicionesNodoDto dto;

        public override void Setup()
        {
            // Configuramos el LED
            gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            // Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            // Configuramos el Lora
            Hilo.Intentar(() =>
            {
                if (ES_PROTOBOARD)
                    lora = new LoRaDevice(pinLoraDatos: 4, pinLoraReset: 15);
                else
                    lora = new LoRaDevice();
                lora.Iniciar();
            }, "Lora");

            dto = new MedicionesNodoDto() { serial_number = Config.NumeroSerie };

            // Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        private readonly MemoryStream buffer = new MemoryStream();
        public override void Loop(ref bool activo)
        {
            try
            {
                // Mido la bateria
                float bateria = (float)random.NextDouble() * 100;
                // Mido la temperatura
                float temperatura = (float)random.NextDouble() * 5 + 25;
                // Mido la humedad
                float humedad = (float)random.NextDouble() * 100;

                dto.AgregarMedicion(bateria, TiposMediciones.Bateria);
                dto.AgregarMedicion(temperatura, TiposMediciones.Temperatura);
                dto.AgregarMedicion(humedad, TiposMediciones.Humedad);
                dto.last_updated = DateTime.UtcNow;

                byte[] payload = dto.ToBytes(buffer);
                lora.Enviar(payload);

                Logger.Debug($"Paquete {dto.last_updated.Ticks} enviado, {payload.Length} bytes");
                Blink();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                dto.measurements.Clear();
                buffer.Position = 0;
                LimpiarMemoria();

                if (DEEPSLEEP)
                    aySleep.DeepSleepSegundos(segundosSleep);
                else
                    Thread.Sleep(milisLoop);
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
