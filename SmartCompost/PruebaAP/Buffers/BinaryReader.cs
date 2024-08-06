using System;
using System.IO;
using System.Text;

namespace PruebaAP
{
    public class BinaryReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer = new byte[8];

        public BinaryReader(MemoryStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public byte ReadByte()
        {
            FillBuffer(sizeof(byte));
            return _buffer[0];
        }

        public ushort ReadUInt16()
        {
            FillBuffer(sizeof(ushort));
            return BitConverter.ToUInt16(_buffer, 0);
        }

        public int ReadInt32()
        {
            FillBuffer(sizeof(int));
            return BitConverter.ToInt32(_buffer, 0);
        }

        public long ReadInt64()
        {
            FillBuffer(sizeof(long));
            return BitConverter.ToInt64(_buffer, 0);
        }

        public double ReadDouble()
        {
            FillBuffer(sizeof(double));
            return BitConverter.ToDouble(_buffer, 0);
        }

        public float ReadSingle()
        {
            FillBuffer(sizeof(float));
            return BitConverter.ToSingle(_buffer, 0);
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "El número de bytes debe ser no negativo.");

            byte[] result = new byte[count];
            FillBuffer(count, result);

            return result;
        }

        public string ReadString(int length = -1)
        {
            if (length < 1)
                length = ReadUInt16();

            return Encoding.UTF8.GetString(ReadBytes(length), 0, length);
        }

        private void FillBuffer(int numBytes, byte[] buffer = null)
        {
            buffer = buffer ?? _buffer;

            if (_stream.Read(buffer, 0, numBytes) != numBytes)
                throw new Exception("No hay suficientes bytes en el stream.");
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
