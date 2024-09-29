using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using static Equipos.SX127X.SX127XDevice;

namespace Equipos.SX127X
{
    // Github: https://github.com/KiwiBryn/SX127X-NetNF/tree/master
    // Es un wrapper para simplificar la interaccion con el device
    public class LoRaDevice : IDisposable
    {
        public const int MAX_LORA_PAYLOAD_BYTES = 128;

        public event onReceivedEventHandler OnReceive;
        public event onTransmittedEventHandler OnTransmit;

        private readonly SX127XDevice device;
        private readonly SpiDevice spi;
        private readonly GpioController gpio;
        private bool iniciado;

        public LoRaDevice(
            int pinMISO = Gpio.IO19,
            int pinMOSI = Gpio.IO23,
            int pinSCK = Gpio.IO18, // Clock
            int pinNSS = Gpio.IO05, // Slave select: puede ser cualquier GPIO
            int pinDIO0 = Gpio.IO25, // Datos Lora: puede ser cualquier GPIO
            int pinReset = Gpio.IO14, // Puede ser cualquier GPIO
            int SPI_BUS = 1)
        {
            Configuration.SetPinFunction(pinMISO, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(pinMOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(pinSCK, DeviceFunction.SPI1_CLOCK);

            var spiSender = new SpiConnectionSettings(SPI_BUS, pinNSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0,// From SemTech docs pg 80 CPOL=0, CPHA=0
                                     //SharingMode = SpiSharingMode.Shared
            };

            spi = new SpiDevice(spiSender);
            gpio = new GpioController();

            device = new SX127XDevice(spi, gpio, dio0Pin: pinDIO0, resetPin: pinReset);
            device.OnReceive += (object sender, OnDataReceivedEventArgs e) => OnReceive?.Invoke(sender, e);
            device.OnTransmit += (object sender, OnDataTransmitedEventArgs e) => OnTransmit?.Invoke(sender, e);
        }

        public void Iniciar(double frequency)
        {
            device.Initialize(
                frequency,
                lnaGain: RegLnaLnaGain.Default,
                lnaBoost: true,
                powerAmplifier: RegPAConfigPASelect.PABoost,
                rxPayloadCrcOn: true,
                rxDoneignoreIfCrcMissing: false
                );

            iniciado = true;

            Thread.Sleep(100);
        }

        public void ModoRecibir() => device.Receive();

        /// <summary>
        /// OJO!! Se desconfigura todo, por eso iniciado = false
        /// </summary>
        public void ModoSleep() { iniciado = false; device.Sleep(); }

        public void Enviar(byte[] data) => Enviar(data, 0, data.Length);

        public void Enviar(byte[] data, int index, int length)
        {
            if (!iniciado)
                Iniciar(this.device.Frequency);

            device.Send(data, index, length);
        }

        public void Dispose()
        {
            gpio?.Dispose();
            spi?.Dispose();
        }
    }
}
