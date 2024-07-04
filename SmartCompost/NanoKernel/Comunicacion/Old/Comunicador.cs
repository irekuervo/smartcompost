//using NanoKernel.Loggin;
//using System;
//using System.Text;
//using System.Threading;

using System;

namespace NanoKernel.Comunicacion.Old
{
    public delegate void OnDataRecieved(byte[] data, int offset, int count);


    /// <summary>
    /// Todavia no me queda claro que es esto.
    /// 
    /// Una idea es que los comunicadores solo pueden hablar entre ellos
    /// Es decir, un comunicador tiene canales, y envia paquetes con 
    /// la forma y tiempo de otro comunicador del framework (entre nodos)
    /// 
    /// Por otro lado, estaria bueno tener una clase que simplifique las 
    /// implementaciones de protocolos (mecanismo de polling para pasar de full duplex a half duplex, etc)
    /// </summary>
    public class Comunicador
    {
        public event OnDataRecieved DataRecieved;

        public void SendAsync(byte[] bufferResponse, int v, int length)
        {
            throw new NotImplementedException();
        }
    }

}

//namespace NanoKernel.Comunicacion
//{
//    

//    // Es para una conexion punto a punto
//    public abstract class Canal : IDisposable
//    {
//        public Canal(string direccion)
//        {
//            this.direccion = direccion;
//        }

//        public event OnDataRecieved DataRecieved;
//        public int Timeout { get; set; } = 1000;
//        protected Buffer buffer;
//        private string direccion;

//        public string Send(string text, int timeout = -1)
//        {
//            var payload = Encoding.UTF8.GetBytes(text);
//            var res = Send(payload, 0, payload.Length);
//            return Encoding.UTF8.GetString(res.Data, 0, res.Length);
//        }

//        public void SendAsync(string text)
//        {
//            var payload = Encoding.UTF8.GetBytes(text);
//            Send(payload, 0, payload.Length);
//        }

//        public Buffer Send(byte[] data, int offset, int count)
//        {

//        }


//        protected abstract void SendImpl(byte[] data, int offset, int count);







//        private object readLock = new object();
//        private DateTime start;
//        private bool timeout = false;
//        private string errRead;
//        private int ReadData()
//        {
//            lock (readLock)
//            {
//                start = DateTime.UtcNow;
//                timeout = false;

//                while (serial.BytesToRead == 0)
//                {
//                    Thread.Sleep(10);
//                    if ((DateTime.UtcNow - start).TotalMilliseconds > Timeout)
//                    {
//                        timeout = true;
//                        break;
//                    }
//                }

//                if (timeout) { throw new Exception("Error de timeout"); }

//                if (serial.BytesToRead > DEFAULT_SERIAL_BUFFER_SIZE)
//                {
//                    errRead = "No puedo leer tanto tamanio: " + serial.BytesToRead;
//                    Logger.Log(errRead);
//                    throw new Exception(errRead);
//                }

//                var bytes_read = serial.Read(buffer, 0, serial.BytesToRead);
//                Logger.Log($"{serial.PortName}: {bytes_read} bytes recieved");

//                DataRecieved?.Invoke(buffer, 0, bytes_read);

//                return bytes_read;
//            }
//        }

//        public abstract void Dispose();



//    }
//}
