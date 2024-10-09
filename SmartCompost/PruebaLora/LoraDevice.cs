using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace PruebaLora
{
    public class LoraDevice : IDisposable
    {
        public ModoOperacion ModoOperacion
        {
            get
            {
                return (ModoOperacion)registers.Read((byte)Address.RegOpMode, (byte)Mascaras.ModoOperacion);
            }
            set
            {
                registers.Write((byte)Address.RegOpMode, (byte)value, (byte)Mascaras.ModoOperacion);
            }
        }

        public bool RxPayloadCrcOn
        {
            get
            {
                return registers.Read((byte)Address.RegModemConfig2, (byte)Mascaras.RxPayloadCrcOn) == TRUE;
            }
            set
            {
                byte val = value ? TRUE : FALSE;
                registers.Write((byte)Address.RegModemConfig2, val, (byte)Mascaras.RxPayloadCrcOn);
            }
        }

        public bool AGC
        {
            get
            {
                return registers.Read((byte)Address.RegModemConfig3, (byte)Mascaras.AGC_AutoOn) == TRUE;
            }
            set
            {
                byte val = value ? TRUE : FALSE;
                registers.Write((byte)Address.RegModemConfig3, val, (byte)Mascaras.AGC_AutoOn);
            }
        }


        private const byte RegVersionValueExpected = 0x12;
        private const double FXOSC = 32000000.0;
        private const double FSTEP = FXOSC / 524288.0;
        private const double FREQ_THRESHOLD = 779e6;

        private const int PA_BOOST = 0x80;
        private const int Pout_Boost = 17;

        private const byte TRUE = 1;
        private const byte FALSE = 0;

        private RegisterManager registers;
        private GpioController gpio;
        private int resetPin = 14;
        private int dio0Pin = 25;

        public LoraDevice(SpiDevice spi, GpioController gpio, int resetPin = 14, int dio0Pin = 25)
        {
            registers = new RegisterManager(spi);
            this.gpio = gpio;

            this.resetPin = resetPin;
            this.dio0Pin = dio0Pin;

            gpio.OpenPin(resetPin, PinMode.Output);
        }

        // Inspirado en https://github.com/sandeepmistry/arduino-LoRa/blob/master/src/LoRa.cpp
        public void Iniciar(double frecuencia)
        {
            Restart();

            CheckIfConnected();

            // nos suscribimos a la interrupcion del pin DIO0
            gpio.OpenPin(dio0Pin, PinMode.InputPullDown);
            gpio.RegisterCallbackForPinValueChangedEvent(dio0Pin, PinEventTypes.Rising, InterruptGpioPin_ValueChanged);

            ModoOperacion = ModoOperacion.Sleep; // para poder cambiar la config

            // seteamos la frecuencia
            SetFrequency(frecuencia);

            // AutomaticGainControl on
            AGC = true;

            // seteamos la ganancia
            SetPowerGain();

            registers.Write((byte)Address.RegOpMode, TRUE, (byte)Mascaras.LongRangeMode); // seteamos modo lora

            // config sync word
            registers.Write((byte)Address.RegSyncWord, 0x12);

            ModoOperacion = ModoOperacion.Standby;
        }

        public void Recibir()
        {
            registers.Write((byte)Address.RegDioMapping1, 0x0); //Dio0 rxDone
            ModoOperacion = ModoOperacion.ReceiveContinuous;
        }

        const byte RxDone = 0b01000000;
        private void InterruptGpioPin_ValueChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("Algo paso!!");

            // Leemos el flag que genero el interrupt
            Byte irqFlags = registers.Read((byte)Address.RegIrqFlags);

            if ((irqFlags & (byte)Mascaras.RxDoneMask) == RxDone)
            {
                Console.WriteLine("Recibi un paquete!!");
            }

            // Clear de todos los flags
            registers.Write((byte)Address.RegIrqFlags, 255);
        }

        public void Enviar(byte[] bytes) => Enviar(bytes, 0, bytes.Length);

        public void Enviar(byte[] bytes, int index, int length)
        {
            this.ModoOperacion = ModoOperacion.Sleep;

            // seteamos los address del buffer FIFO de RX/TX
            registers.Write((byte)Address.RegFifoTxBaseAddr, 0x0);
            registers.Write((byte)Address.RegFifoAddrPtr, 0x0);

            // escribimos en el buffer
            registers.WriteBytes((byte)Address.RegFifo, bytes, index, length);

            // seteamos el largo del payload
            registers.Write((byte)Address.RegPayloadLength, (byte)length);

            this.ModoOperacion = ModoOperacion.Transmit;
        }

        public void Restart()
        {
            gpio.Write(resetPin, PinValue.Low);
            Thread.Sleep(10);
            gpio.Write(resetPin, PinValue.High);
            Thread.Sleep(10);
        }

        public void CheckIfConnected()
        {
            var version = registers.Read((byte)Address.RegVersion);
            if (version != RegVersionValueExpected)
            {
                throw new System.Exception("Lora no conectado");
            }
        }

        // Lo mandamos al palo, PA_BOOST = true
        // Pout=17-(15-OutputPower) if PaSelect = 1 (PA_BOOST pin)
        private void SetPowerGain()
        {
            //// configuramos la potencia
            //// 3.4.3. High Power +20 dBm Operation
            registers.Write((byte)Address.RegPaDac, 0x87);

            setOCP(140);

            registers.Write((byte)Address.RegPAConfig, PA_BOOST | Pout_Boost);
        }

        private void SetFrequency(double frecuencia)
        {
            uint frf = (uint)(frecuencia / FSTEP);

            byte frfMsb = (byte)((frf >> 16) & 0xFF);
            byte frfMid = (byte)((frf >> 8) & 0xFF);
            byte frfLsb = (byte)(frf & 0xFF);

            registers.Write((byte)Address.RegFrMsb, frfMsb);
            registers.Write((byte)Address.RegFrMid, frfMid);
            registers.Write((byte)Address.RegFrLsb, frfLsb);

            if (frecuencia > FREQ_THRESHOLD)
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

        private void setOCP(int mA)
        {
            int ocpTrim = 27;

            if (mA <= 120)
            {
                ocpTrim = (mA - 45) / 5;
            }
            else if (mA <= 240)
            {
                ocpTrim = (mA + 30) / 10;
            }

            registers.Write((byte)Address.RegOcp, (byte)(0x20 | (0x1F & ocpTrim)));
        }

        public void Dispose()
        {
            ModoOperacion = ModoOperacion.Sleep;
        }
    }
}
