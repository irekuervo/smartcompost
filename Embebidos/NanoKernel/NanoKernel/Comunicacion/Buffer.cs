using System;

namespace NanoKernel.Comunicacion
{
    public class Buffer : IDisposable
    {
        public int Length => dataLength;
        public byte[] Data => buffer;

        private int dataLength;
        private byte[] buffer;

        public Buffer()
        {
            Resize(0);
        }

        public Buffer(int length)
        {
            Resize(length);
        }

        public void Resize(int length)
        {
            if (length <= 0)
                return;

            if (buffer == null || length > buffer.Length)
            {
                buffer = new byte[length];
                dataLength = length;
            }
        }

        public void Dispose()
        {
            this.buffer = null;
        }
    }
}
