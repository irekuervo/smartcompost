
namespace Equipos.SX127X
{
    using System;

    using System.Device.Spi;

    public sealed class RegisterManager
    {
        private const int maxBufferSixe = 100;
        private const byte _registerAddressReadMask = 0X7f;
        private const byte _registerAddressWriteMask = 0x80;

        private readonly SpiDevice _spiDevice = null;

        private byte[] readBuffer = new byte[maxBufferSixe];
        private byte[] writeBuffer = new byte[maxBufferSixe];

        private byte[] byteBuffer = new byte[1];
        private object byteLock = new object();

        private object spiLock = new object();

        public RegisterManager(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        public Byte ReadByte(byte registerAddress)
        {
            return ReadBytes(registerAddress, 1)[0];
        }

        public byte[] ReadBytes(byte registerAddress, byte length)
        {
            if (length > maxBufferSixe || length == 0)
                return null;

            lock (spiLock)
            {
                try
                {
                    writeBuffer[0] = registerAddress &= _registerAddressReadMask;
                    writeBuffer[1] = 0;
                    var write = new SpanByte(writeBuffer, 0, 2);

                    var data = new SpanByte(readBuffer, 0, length + 1);

                    TransferData(write, data);

                    byte[] replyBuffer = new byte[length];
                    Array.Copy(readBuffer, 1, replyBuffer, 0, length);

                    return replyBuffer;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }

        public void WriteByte(byte address, byte value, bool validate = true)
        {
            lock (byteLock)
            {
                byteBuffer = new byte[] { value };

                WriteBytes(address, byteBuffer);

                if (validate)
                {
                    var realVal = ReadByte(address);
                    if (realVal != value)
                        throw new Exception($"Valor recibido {realVal} distinto al escrito {value} en {address}");
                }
            }
        }

        public void WriteBytes(byte address, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return;

            lock (spiLock)
            {
                writeBuffer[0] = address |= _registerAddressWriteMask;
                Array.Copy(bytes, 0, writeBuffer, 1, bytes.Length);

                var write = new SpanByte(writeBuffer, 0, bytes.Length + 1);

                TransferData(write, SpanByte.Empty);
            }
        }

        public void WriteWordMsbLsb(byte address, ushort value)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            byte[] bytesMsbLsb = new byte[] { valueBytes[1], valueBytes[0] };

            WriteBytes(address, bytesMsbLsb);
        }

        private void TransferData(SpanByte writebuffer, SpanByte readBuffer)
        {
            _spiDevice.TransferFullDuplex(writebuffer, readBuffer);
        }
    }
}
