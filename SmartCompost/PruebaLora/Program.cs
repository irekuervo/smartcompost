using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.Spi;
using System.Text;
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


            int step = 100_000;
            int i = 0;

            while (true)
            {
                if (i > 4)
                    i = 0;

                lora.Iniciar(927_000_000 );
                lora.Enviar(Encoding.UTF8.GetBytes("hola"));
                Thread.Sleep(1000);
                i++;
            }

        }
    }
}
