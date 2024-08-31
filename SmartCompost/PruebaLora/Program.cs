using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace PruebaLora
{
    public class Program
    {
        // Estoy probando en la protoboard
        private const int PIN_MISO = 19;
        private const int PIN_MOSI = 23;
        private const int PIN_CLK = 18;
        private const int PIN_NSS = 5;
        private const int PIN_DIO0 = 22;

        public static void Main()
        {

            Configuration.SetPinFunction(PIN_MISO, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(PIN_MOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(PIN_CLK, DeviceFunction.SPI1_CLOCK);

            var spiSender = new SpiConnectionSettings(busId: 1, PIN_NSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0
            };

            var spi = new SpiDevice(spiSender);
            var gpio = new GpioController();

            LoraDevice lora = new LoraDevice(spi, gpio);

            lora.Restart();

            var modo = lora.ModoOperacion;

            lora.ModoOperacion = ModoMedicion.Sleep;

            modo = lora.ModoOperacion;

            lora.ModoOperacion = ModoMedicion.Standby;

            modo = lora.ModoOperacion;

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
