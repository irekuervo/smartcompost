using NanoKernel.Loggin;
using NanoKernel.Modulos;
using System;
using System.Text;

namespace NanoKernel.Comunicacion
{
    internal class ComunicadorSIM800L : Comunicador
    {
        public override event OnDataRecieved DataRecieved;

        private ComunicadorSerie com;

        public ComunicadorSIM800L(ComunicadorSerie com)
        {
            this.com = com;
            this.com.DataRecieved += Com_DataRecieved;
        }

        private void Com_DataRecieved(byte[] data, int offset, int count)
        {
            Logger.Log(Encoding.UTF8.GetString(data, offset, count));
        }

        [Servicio("com")]
        public void Comando(string comando)
        {
            byte[] payload = Encoding.UTF8.GetBytes(comando);
            com.SendAsync(payload, 0, payload.Length);
        }

        public override byte[] Send(byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void SendAsync(byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {

        }
    }
}
