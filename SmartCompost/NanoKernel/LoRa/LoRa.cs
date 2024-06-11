using devMobile.IoT.SX127xLoRaDevice;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using static devMobile.IoT.SX127xLoRaDevice.SX127XDevice;

namespace NanoKernel.LoRa
{
    ///Github: https://github.com/KiwiBryn/SX127X-NetNF/tree/master
    public class LoRa : IDisposable
    {
        public event onReceivedEventHandler OnReceive;
        public event onTransmittedEventHandler OnTransmit;

        private readonly SX127XDevice device;
        private readonly SpiDevice spi;
        private readonly GpioController gpio;
        private bool iniciado;

        public LoRa(
            int pinMISO = Gpio.IO19,
            int pinMOSI = Gpio.IO23,
            int pinCLOCK = Gpio.IO18,
            int pinNSS = Gpio.IO05,
            int pinLoraDatos = Gpio.IO25,
            int pinLoraReset = Gpio.IO14,
            int SPI_BUS = 1)
        {
            Configuration.SetPinFunction(pinMISO, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(pinMOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(pinCLOCK, DeviceFunction.SPI1_CLOCK);

            var spiSender = new SpiConnectionSettings(SPI_BUS, pinNSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0,// From SemTech docs pg 80 CPOL=0, CPHA=0
                                     //SharingMode = SpiSharingMode.Shared
            };

            spi = new SpiDevice(spiSender);
            gpio = new GpioController();

            device = new SX127XDevice(spi, gpio, dio0Pin: pinLoraDatos);
            device.OnReceive += (object sender, OnDataReceivedEventArgs e) => OnReceive?.Invoke(sender, e);
            device.OnTransmit += (object sender, OnDataTransmitedEventArgs e) => OnTransmit?.Invoke(sender, e);
        }

        public void Iniciar(double frequency = 433e6)
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

        public void Enviar(byte[] data)
        {
            if (!iniciado)
                throw new Exception("El device no esta iniciado");

            device.Send(data);
            device.Receive();
        }

        public void Dispose()
        {
            gpio.Dispose();
            spi.Dispose();
        }
    }
}
