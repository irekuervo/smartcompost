using System;
using System.IO;
using System.Text;

namespace NanoKernel.Ayudantes
{
    public class BinaryWriter
    {
        private readonly MemoryStream _stream;

        public BinaryWriter(MemoryStream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void Write(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Write(ushort value)
        {
            WriteInternal(BitConverter.GetBytes(value), 0, sizeof(ushort));
        }

        public void Write(int value)
        {
            WriteInternal(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public void Write(long value)
        {
            WriteInternal(BitConverter.GetBytes(value), 0, sizeof(long));
        }

        public void Write(double value)
        {
            WriteInternal(BitConverter.GetBytes(value), 0, sizeof(double));
        }

        public void Write(float value)
        {
            WriteInternal(BitConverter.GetBytes(value), 0, sizeof(float));
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public void Write(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0 || count < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            WriteInternal(buffer, index, count);
        }

        public void Write(string value)
        {
            Write(Encoding.UTF8.GetBytes(value));
        }

        private void WriteInternal(byte[] buffer, int index, int count)
        {
            _stream.Write(buffer, index, count);
        }
    }
}
