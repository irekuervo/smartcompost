using Equipos.SX127X;
using NanoKernel.Ayudantes;
using NanoKernel.Comunicacion;
using NanoKernel.Dominio;
using NanoKernel.DTOs;
using NanoKernel.Herramientas.Comunicacion;
using NanoKernel.Herramientas.Medidores;
using NanoKernel.Hilos;
using NanoKernel.Logging;
using NanoKernel.Nodos;
using System;
using System.Collections;
using System.Device.Adc;
using System.Device.Gpio;
using System.IO;
using System.Threading;

namespace NodoAP
{
    /// <summary>
    /// 
    ///  TODO: Agregar medicion bateria
    ///  TODO: Arreglar frecuencia a una permitida por ENACOM
    ///  
    /// </summary>

    public class NodoAP : NodoBase
    {
        public override TiposNodo tipoNodo => TiposNodo.AccessPointLora;

        /// ---------------------------------------------------------------
        /// CONFIGURACION TUNEADA PARA DONGLE 4G
        private const int tamanioCola = 75; // Valor clave tuneado para que el heap no muera, cuanto mas grande mejor, pero puede quedarse sin memoria
        private const int ventanaDesencolamiento = 20; /// Desencolamos de a pedazos, no todo junto, json es matador
        private const int clientTimeoutSeconds = 20;
        private const int intentosEnvioMediciones = 1; // Creo que no lo vamos a usar > 1
        private const int milisIntentoEnvioMediciones = 100;
        private const int segundosMedicionNodoAp = 60 * 1;
        /// ---------------------------------------------------------------
        private SmartCompostClient cliente;
        private GpioPin led;
        private LoRaDevice lora;
        private const int PIN_MISO = 19;
        private const int PIN_MOSI = 23;
        private const int PIN_CLK = 18;
        private const int PIN_NSS = 5;
        private const int PIN_DIO0 = 25;
        private const int PIN_RESET = 14;
        private const double FREQ_LORA = 433e6; //920_000_000;
        /// ---------------------------------------------------------------
        private const int ADC_BATERIA = 3;  //pin 39        // ADC Channel 6 - GPIO 34
        private AdcController adc;
        private AdcChannel bateriaAdcSensor;
        /// ---------------------------------------------------------------
        private ConcurrentQueue colaMedicionesNodo = new ConcurrentQueue(tamanioCola);
        private ArrayList desencolados = new ArrayList();
        private MedicionesApDto medicionesAp = new MedicionesApDto();
        /// ---------------------------------------------------------------
        private MedicionesNodoDto medicionNodo;
        private Medidor medidor;
        private const string MED_TIRADOS = "tirados";
        private const string MED_RECIBIDOS = "encolados";
        private const string MED_ENVIADOS = "enviados";
        private const string MED_ERRORES = "errores";

        public override void Setup()
        {
            // TODO: Esto deberia hacerse con el deploy, no hardcodearse
            Config.RouterSSID = "SmartCompost"; //"Bondiola 2.4"; // 
            Config.RouterPassword = "Quericocompost"; //"conpapafritas";  //
            Config.SmartCompostHost = "smartcompost.net"; //"181.88.245.34"; //"192.168.1.6";
            Config.SmartCompostPort = "8080";
            Config.NumeroSerie = "58670345-7dc4-11ef-919e-0242ac160004";

            /// Configuramos el LED
            var gpio = new GpioController();
            led = gpio.OpenPin(2, PinMode.Output);
            /// Prendemos el led para avisar que estamos configurando
            led.Write(PinValue.High);

            /// Conectamos a internet
            Hilo.Intentar(() =>
            {
                if (ayInternet.ConectarsePorWifi(Config.RouterSSID, Config.RouterPassword) == false)
                {
                    throw new Exception("Fallo la conexion al router");
                }

                string ip = ayInternet.ObtenerIp();
                if (ip == "0.0.0.0")
                    throw new Exception("No se pudo asignar la ip");

                /// IP asignada
                Logger.Log($"Ip asignada: {ip}");

            }, $"Wifi");

            /// Vemos si podemos pingear la api
            bool ping = ayInternet.Ping(Config.SmartCompostHost);
            if (ping == false)
                Logger.Log("NO HAY PING AL SERVIDOR");

            /// Cliente
            cliente = new SmartCompostClient(Config.SmartCompostHost, Config.SmartCompostPort, clientTimeoutSeconds);

            /// Configuramos el Lora
            Hilo.Intentar(() =>
                {
                    lora = new LoRaDevice(
                        pinMISO: PIN_MISO,
                        pinMOSI: PIN_MOSI,
                        pinSCK: PIN_CLK,
                        pinNSS: PIN_NSS,
                        pinDIO0: PIN_DIO0,
                        pinReset: PIN_RESET);
                    lora.Iniciar(FREQ_LORA);
                    lora.ModoRecibir();
                },
                "Lora",
                accionException: () =>
                {
                    lora?.Dispose();
                });
            
            /// Mediciones del AP
            medicionNodo = new MedicionesNodoDto();
            medicionNodo.serial_number = Config.NumeroSerie;
            medicionNodo.last_updated = DateTime.UtcNow;

            // Avisamos que nos despertamos
            medicionNodo.AgregarMedicion(1, TiposMediciones.Startup);
            Hilo.Intentar(() => cliente.AddNodeMeasurments(Config.NumeroSerie, medicionNodo), intentos: 3);
            medicionNodo.measurements.Clear();

            // Inicializamos el medidor del ap
            adc = new AdcController();
            bateriaAdcSensor = adc.OpenChannel(ADC_BATERIA);
            medidor = new Medidor(segundosMedicionNodoAp * 1000);
            medidor.OnMedicionesEnPeriodoCallback += Medidor_OnMedicionesEnPeriodoCallback;
            //medidor.Iniciar();

            // Escuchamos cuando terminamos de configurar
            lora.OnReceive += Device_OnReceive;

            /// Avisamos que terminamos de configurar
            led.Write(PinValue.Low);
        }

        // -------------- MENSAJES LORA -----------------
        private void Device_OnReceive(object sender, SX127XDevice.OnDataReceivedEventArgs e)
        {
            try
            {
                //Logger.Debug($"PacketSNR: {e.PacketSnr}, PacketRSSI: {e.PacketRssi}dBm, RSSI: {e.Rssi}dBm, Length: {e.Data.Length}bytes");
                Logger.Debug($"Paquete recibido: {e.Data.Length} bytes");

                if (e.Data == null)
                    return;

                byte[] medicionDesbordada = (byte[])colaMedicionesNodo.Enqueue(e.Data);
                if (medicionDesbordada == null)
                {
                    // Si lo encolo, le pongo la fecha de ahora a las mediciones
                    SetearTimestampMedicion(e.Data);
                    medidor.Contar(MED_RECIBIDOS);
                }
                else
                {
                    medicionDesbordada = null; // Liberamos memoria, no nos interesa ya
                    medidor.Contar(MED_TIRADOS);
                    Logger.Debug("Cola mediciones desbordada");
                }
            }
            catch (Exception ex)
            {
                medidor.Contar(MED_ERRORES);
                Logger.Log(ex.Message);
            }
        }

        private static void SetearTimestampMedicion(byte[] data)
        {
            // Le clavamos la hora de arrivo como la hora de medicion, es la mejor aproximacion que tenemos
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //movemos la posicion del buffer hasta la fecha, y la cambiamos
                var tipoPaquete = (TipoPaqueteEnum)br.ReadByte();
                if (tipoPaquete == TipoPaqueteEnum.MedicionNodo)
                {
                    br.ReadString();
                    Array.Copy(BitConverter.GetBytes(DateTime.UtcNow.Ticks), 0, data, (int)ms.Position, sizeof(long));
                }
            }
        }

        // -------------- ENVIO MENSAJES -----------------
        public override void Loop(ref bool activo)
        {
            /// Si no hay mensajes encolados no hacemos nada
            if (colaMedicionesNodo.IsEmpty())
            {
                Thread.Sleep(100);
                return;
            }

            try
            {
                int tamanioCola = colaMedicionesNodo.Count();
                for (int i = 0; i < ventanaDesencolamiento && !colaMedicionesNodo.IsEmpty(); i++)
                {
                    var item = (byte[])colaMedicionesNodo.Dequeue();
                    try
                    {
                        desencolados.Add(item);
                        medicionesAp.AgregarMediciones(MedicionesNodoDto.FromBytes(item));
                    }
                    catch (Exception ex) {
                        desencolados.Remove(item);
                        Logger.Log(ex);
                    }
                }

                if (desencolados.Count == 0) {
                    Logger.Error("No se puede enviar nada");
                    return;
                }

                Logger.Debug($"Desencolando {desencolados.Count}/{tamanioCola} medicionesNodo");

                // Enviamos el mensaje
                medicionesAp.last_updated = DateTime.UtcNow;
                bool enviado = Hilo.Intentar(
                    () => cliente.AddApMeasurments(Config.NumeroSerie, medicionesAp),
                    nombreIntento: "Envio mediciones Nodos",
                    milisIntento: milisIntentoEnvioMediciones,
                    intentos: intentosEnvioMediciones);

                if (enviado)
                {
                    Blink(100);

                    medidor.Contar(MED_ENVIADOS, desencolados.Count - mensajesAP);
                    Logger.Log($"Se enviaron {desencolados.Count} medicionesNodo");
                }
                else
                {
                    medidor.Contar(MED_ERRORES);

                    /// Si podemos volvemos a meterlo en la cola, sino los tiro para dejar lugar a nuevos mensajes
                    int indiceReencolado = 0;
                    object obj = null;
                    do
                    {
                        obj = colaMedicionesNodo.Enqueue(desencolados[indiceReencolado++]);
                    }
                    while (obj == null && indiceReencolado < desencolados.Count);

                    medidor.Contar(MED_TIRADOS, desencolados.Count - indiceReencolado + 1);
                    Logger.Debug($"Reencolados {indiceReencolado + 1} medicionesNodo");
                }
            }
            catch (Exception e)
            {
                medidor.Contar(MED_ERRORES);
                Logger.Log(e);
            }
            finally
            {
                // Limpiamos todo
                mensajesAP = 0;
                medicionesAp.nodes_measurements.Clear();
                desencolados.Clear();
                LimpiarMemoria();

#if DEBUG
                Logger.Log($"Enviados: {medidor.ContadoTotal(MED_ENVIADOS)} | Tirados: {medidor.ContadoTotal(MED_TIRADOS)} | Encolados: {colaMedicionesNodo.Count()}");
#endif

            }
        }

        // -------------- MENSAJES DEBUG AP -----------------
        private int mensajesAP = 0; // Sino contamos las mediciones AP como mediciones Nodos
        private void Medidor_OnMedicionesEnPeriodoCallback(InstanteMedicion resultado)
        {
            try
            {
                medicionNodo.AgregarMedicion(colaMedicionesNodo.Count(), TiposMediciones.TamanioCola);

                var recibidos = resultado.ContadoEnPeriodo(MED_RECIBIDOS);
                if (recibidos > 0)
                    medicionNodo.AgregarMedicion(recibidos, TiposMediciones.MensajesRecibidos);

                var tirados = resultado.ContadoEnPeriodo(MED_TIRADOS);
                if (tirados > 0)
                    medicionNodo.AgregarMedicion(tirados, TiposMediciones.MensajesTirados);

                var errores = resultado.ContadoEnPeriodo(MED_ERRORES);
                if (errores > 0)
                    medicionNodo.AgregarMedicion(errores, TiposMediciones.Errores);

                var enviados = resultado.ContadoEnPeriodo(MED_ENVIADOS);
                if (enviados > 0)
                    medicionNodo.AgregarMedicion(enviados, TiposMediciones.MensajesEnviados);

                var bateria = MedirBateria();
                if (bateria > 0)
                    medicionNodo.AgregarMedicion(bateria, TiposMediciones.Bateria);

                medicionNodo.last_updated = DateTime.UtcNow;

                colaMedicionesNodo.Enqueue(medicionNodo.ToBytes());
                mensajesAP++;
                Logger.Debug("Encolando mediciones del AP");
            }
            catch (Exception ex)
            {
                medidor.Contar(MED_ERRORES);
                Logger.Log(ex);
            }
            finally
            {
                medicionNodo.measurements.Clear();
            }
        }

        private float MedirBateria()
        {
            int analogValue = bateriaAdcSensor.ReadValue();
            float vSensor = analogValue / 4095f * 3.3f;

            // Cuenta de la bateria, mapeando las cotas con el ADC
            // y = a x + b
            // 0 = a * 2.52 V + b
            // 100 = a * 3.3 V + b
            // y = 128.21 * x − 323.06
            double bateriaPorcentaje = 128.21 * vSensor - 323.06;
            if (bateriaPorcentaje > 100) bateriaPorcentaje = 100;
            if (bateriaPorcentaje < 0) bateriaPorcentaje = 0;

            return analogValue;
        }

        private void Blink(int time)
        {
            led.Write(PinValue.High);
            Thread.Sleep(time);
            led.Write(PinValue.Low);
        }
    }
}
