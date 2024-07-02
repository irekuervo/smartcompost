using System;
using System.IO;

namespace NanoKernel.Ayudantes
{
    public class BinaryReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _buffer = new byte[8];

        public BinaryReader(MemoryStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public int ReadInt32()
        {
            FillBuffer(4);
            return BitConverter.ToInt32(_buffer, 0);
        }

        public long ReadInt64()
        {
            FillBuffer(8);
            return BitConverter.ToInt64(_buffer, 0);
        }

        public double ReadDouble()
        {
            FillBuffer(8);
            return BitConverter.ToDouble(_buffer, 0);
        }

        public float ReadSingle()
        {
            FillBuffer(4);
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
