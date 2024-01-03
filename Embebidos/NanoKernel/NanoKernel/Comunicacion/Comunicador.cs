using System;

namespace NanoKernel.Comunicacion
{
    public abstract class Comunicador : IDisposable
    {
        public delegate void OnDataRecieved(byte[] data, int offset, int count);
        public abstract byte[] Send(byte[] data, int offset, int count);
        public abstract void SendAsync(byte[] data, int offset, int count);

        public abstract event OnDataRecieved DataRecieved;
        public abstract void Dispose();
    }
}
