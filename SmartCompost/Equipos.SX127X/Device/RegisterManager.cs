﻿
namespace Equipos.SX127X
{
    using System;

    using System.Device.Spi;

    public sealed class RegisterManager
    {
        private const byte _registerAddressReadMask = 0x7f;
        private const byte _registerAddressWriteMask = 0x80;

        private readonly SpiDevice _spiDevice = null;

        private readonly byte[] readBuffer;
        private readonly byte[] writeBuffer;
        private readonly byte[] byteBuffer = new byte[1]; // para escribir solo 1 byte

        private object byteLock = new object();

        private object spiLock = new object();

        private readonly int maxBufferSize;

        public RegisterManager(SpiDevice spiDevice, int maxBufferSize = 128)
        {
            this.maxBufferSize = maxBufferSize;

            readBuffer = new byte[maxBufferSize];
            writeBuffer = new byte[maxBufferSize];

            _spiDevice = spiDevice;
        }

        public Byte ReadByte(byte registerAddress)
        {
            return ReadBytes(registerAddress, 1)[0];
        }

        public byte[] ReadBytes(byte registerAddress, byte length)
        {
            if (length > maxBufferSize || length == 0)
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

        // Esto asume que el valor ya esta enmascarado correctamente
        public void WriteByte(byte address, byte value)
        {
            lock (byteLock)
            {
                byteBuffer[0] = value;

                WriteBytes(address, byteBuffer);

                // Nota para erik: el validate lo saque porque no tiene que dar igual al valor.
                // Hay que comprar contra una mascara para poder validar realmente el registro
            }
        }

        public void WriteBytes(byte address, byte[] bytes) => WriteBytes(address, bytes, 0, bytes.Length);

        public void WriteBytes(byte address, byte[] dataBuffer, int index, int length)
        {
            if (dataBuffer == null || dataBuffer.Length == 0)
                return;

            lock (spiLock)
            {
                // Los datos que mandamos tienen la direccion en la pos 0, por eso length + 1
                writeBuffer[0] = address |= _registerAddressWriteMask;
                Array.Copy(dataBuffer, index, writeBuffer, 1, length);

                var write = new SpanByte(writeBuffer, 0, length + 1);

                // No nos interesa leer nada de la respuesta
                TransferData(write, SpanByte.Empty);
            }
        }

        public void WriteWordMsbLsb(byte address, ushort value)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            byte[] bytesMsbLsb = new byte[] { valueBytes[1], valueBytes[0] };

            WriteBytes(address, bytesMsbLsb);
        }

        private void TransferData(SpanByte writeSpan, SpanByte readSpan)
        {
            _spiDevice.TransferFullDuplex(writeSpan, readSpan);
        }
    }
}
