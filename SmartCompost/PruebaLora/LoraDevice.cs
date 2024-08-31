using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace PruebaLora
{
    public class LoraDevice
    {
        public ModoOperacion ModoOperacion
        {
            get
            {
                return (ModoOperacion)registers.ReadAddress((byte)Address.RegOpMode, (byte)Mascaras.ModoOperacion);
            }
            set
            {
                registers.Write((byte)Address.RegOpMode, (byte)value, (byte)Mascaras.ModoOperacion);
            }
        }

        private const int PIN_RESET = 21;
        private const byte RegVersionValueExpected = 0x12;
        private const double FXOSC = 32000000.0;
        private const double FSTEP = FXOSC / 524288.0;

        private RegisterManager registers;
        private GpioController gpio;
        public LoraDevice(SpiDevice spi, GpioController gpio)
        {
            registers = new RegisterManager(spi);
            this.gpio = gpio;

            gpio.OpenPin(PIN_RESET, PinMode.Output);
        }


        const double threshhold = 779e6;
        public void Iniciar(double frecuencia)
        {
            Restart();

            CheckIfConnected();

            ModoOperacion = ModoOperacion.Sleep; // para poder cambiar la config

            ConfigFreq(frecuencia);

            registers.Write((byte)Address.RegOpMode, 1, (byte)Mascaras.LongRangeMode); // seteamos modo lora

            registers.Write((byte)Address.RegSyncWord, 0x12); // config sync word

            registers.Write((byte)Address.RegPaDac, 0x87); // configuramos la potencia

            ModoOperacion = ModoOperacion.Standby;
        }

        private void ConfigFreq(double frecuencia)
        {
            uint frf = (uint)(frecuencia / FSTEP);

            byte frfMsb = (byte)((frf >> 16) & 0xFF);
            byte frfMid = (byte)((frf >> 8) & 0xFF);
            byte frfLsb = (byte)(frf & 0xFF);

            registers.Write((byte)Address.RegFrMsb, frfMsb);
            registers.Write((byte)Address.RegFrMid, frfMid);
            registers.Write((byte)Address.RegFrLsb, frfLsb);

            if (frecuencia > threshhold)
            {
                // Seteamos high frequency
                registers.Write((byte)Address.RegFrLsb, 0, (byte)Mascaras.LowFrequencyModeOn);
            }
            else
            {
                // Seteamos low frequency
                registers.Write((byte)Address.RegFrLsb, 1, (byte)Mascaras.LowFrequencyModeOn);
            }
        }

        public void Enviar(byte[] bytes) => Enviar(bytes, 0, bytes.Length);

        public void Enviar(byte[] bytes, int index, int length)
        {
            this.ModoOperacion = ModoOperacion.Sleep;

            registers.Write((byte)Address.RegFifoTxBaseAddr, 0x0);
            registers.Write((byte)Address.RegFifoAddrPtr, 0x0);
            registers.WriteBytes((byte)Address.RegFifo, bytes, index, length);
            registers.Write((byte)Address.RegPayloadLength, (byte)length);

            this.ModoOperacion = ModoOperacion.Transmit;
        }

        public void Restart()
        {
            gpio.Write(PIN_RESET, PinValue.Low);
            Thread.Sleep(20);
            gpio.Write(PIN_RESET, PinValue.High);
            Thread.Sleep(50);
        }

        public void CheckIfConnected()
        {
            var version = registers.ReadAddress((byte)Address.RegVersion);
            if (version != RegVersionValueExpected)
            {
                throw new System.Exception("Lora no conectado");
            }
        }
    }
}
