using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Modulo.SIM800L
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
    public class ComunicadorSerie : IDisposable
    {
        public event Action<string> ComandoEnviado;
        public event Action<string> RespuestaRecibida;

        private SerialPort serialPort;
        private Buffer buffer = new Buffer();

        public ComunicadorSerie(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        private object lockWrite = new object();
        private DateTime inicio;
        public string EnviarComando(string comando, int timeoutMilis = 5000)
        {
            lock (lockWrite)
            {
                var linea = comando + "\r";
                serialPort.WriteLine(linea);
                ComandoEnviado?.Invoke(linea);

                inicio = DateTime.UtcNow;

                while ((DateTime.UtcNow - inicio).TotalMilliseconds < timeoutMilis)
                {
                    int bytesDisponibles = serialPort.BytesToRead;

                    if (bytesDisponibles > 0)
                    {
                        buffer.Resize(bytesDisponibles);
                        int bytesRead = serialPort.Read(buffer.Data, 0, bytesDisponibles);
                        if (bytesRead != bytesDisponibles)
                            Debug.WriteLine("Me dieron distintos los buffers");

                        string res = Encoding.UTF8.GetString(buffer.Data, 0, buffer.Length);

                        RespuestaRecibida?.Invoke(res);

                        return res;
                    }

                    Thread.Sleep(100); // Pequeño tiempo de espera antes de volver a verificar
                }

                throw new Exception("Timeout comando: " + comando);
            }
        }

        public void Enviar(byte[] datos, int sleepMilis)
        {
            lock (lockWrite)
            {
                serialPort.Write(datos, 0, datos.Length);
                Thread.Sleep(sleepMilis);
            }
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
