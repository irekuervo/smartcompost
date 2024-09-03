using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.Spi;
using System.Text;
using System.Threading;

namespace PruebaLora
{
    public class Program
    {

        private static bool PROTOBOARD = false; // FALSE ES LA PLACA

        private const int PIN_MISO = 19;
        private const int PIN_MOSI = 23;
        private const int PIN_CLK = 18;
        private const int PIN_NSS = 5;

        // Estoy probando en la protoboard como TX, y la placa como RX
        private static int PIN_RESET = 21;
        private static int PIN_DIO0 = 22;

        private static LoraDevice lora;

        public static void Main()
        {
            Configuration.SetPinFunction(PIN_MISO, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(PIN_MOSI, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(PIN_CLK, DeviceFunction.SPI1_CLOCK);

            if (!PROTOBOARD)
            {
                PIN_RESET = 14;
                PIN_DIO0 = 25;
            }

            var spiSender = new SpiConnectionSettings(busId: 1, PIN_NSS)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0
            };

            var spi = new SpiDevice(spiSender);
            var gpio = new GpioController();

            lora = new LoraDevice(spi, gpio, PIN_RESET, PIN_DIO0);

            lora.Iniciar(927_000_000);

            if (PROTOBOARD)
            {
                while (true)
                {
                    lora.Enviar(Encoding.UTF8.GetBytes("hola"));
                    Thread.Sleep(1000);
                    lora.ModoOperacion = ModoOperacion.Sleep;
                    Thread.Sleep(1000);
                }
            }
            else
            {
                lora.Recibir();
                Thread.Sleep(Timeout.Infinite);
            }

        }
    }
}
