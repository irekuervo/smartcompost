using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using static Equipos.SX127X.SX127XDevice;

namespace Equipos.SX127X
{
    // Github: https://github.com/KiwiBryn/SX127X-NetNF/tree/master
    // Es un wrapper para simplificar la interaccion con el device
    public class LoRaDevice : IDisposable
    {
        public event onReceivedEventHandler OnReceive;
        public event onTransmittedEventHandler OnTransmit;

        private readonly SX127XDevice device;
        private readonly SpiDevice spi;
        private readonly GpioController gpio;
        private bool iniciado;

        public LoRaDevice(
            int pinMISO = Gpio.IO19,
            int pinMOSI = Gpio.IO23,
            int pinCLOCK = Gpio.IO18,
            int pinSlaveSelect = Gpio.IO05, // Puede ser cualquier GPIO
            int pinLoraDatos = Gpio.IO25,
            int pinLoraReset = Gpio.IO14,
            int SPI_BUS = 1)
        {
            Configuration.SetPinFunction(pinMISO, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(pinMOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(pinCLOCK, DeviceFunction.SPI1_CLOCK);

            var spiSender = new SpiConnectionSettings(SPI_BUS, pinSlaveSelect)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0,// From SemTech docs pg 80 CPOL=0, CPHA=0
                                     //SharingMode = SpiSharingMode.Shared
            };

            spi = new SpiDevice(spiSender);
            gpio = new GpioController();

            device = new SX127XDevice(spi, gpio, dio0Pin: pinLoraDatos, resetPin: pinLoraReset);
            device.OnReceive += (object sender, OnDataReceivedEventArgs e) => OnReceive?.Invoke(sender, e);
            device.OnTransmit += (object sender, OnDataTransmitedEventArgs e) => OnTransmit?.Invoke(sender, e);
        }

        public void Iniciar(double frequency = SX127XDevice.FrequencyDefault)
        {
            device.Initialize(
                frequency,
                lnaGain: RegLnaLnaGain.Default,
                lnaBoost: true,
                powerAmplifier: RegPAConfigPASelect.PABoost,
                rxPayloadCrcOn: true,
                rxDoneignoreIfCrcMissing: false
                );

            device.Receive();

            iniciado = true;
        }

        public void Enviar(byte[] data) => Enviar(data, 0, data.Length);

        public void Enviar(byte[] data, int index, int length)
        {
            if (!iniciado)
                throw new Exception("El device no esta iniciado");

            device.Send(data, index, length);

            // Me parece que esto jode
            //device.Receive();
        }

        public void Dispose()
        {
            gpio?.Dispose();
            spi?.Dispose();
        }
    }
}
