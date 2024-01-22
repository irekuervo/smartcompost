using System;
using System.Diagnostics;
using Test.SIM800L;

namespace Modulo.SIM800L
{
    public class API : IDisposable
    {
        //public event Action<string> ComandoEnviado;
        //public event Action<string> RespuestaRecibida;
        //public event Action<string> OnEstadoActualizado;

        public string IP => ip;
        public bool Conectado => conectado;
        public string CalidadSenial => calidadSenial;

        private ComunicadorSerie com;
        private System.Threading.Timer watchdog;

        private string puertoCOM;
        private string apn;
        private string apnUsuario;
        private string apnPassword;

        private bool conectado = false;
        private string calidadSenial = "-";


        // TCP
        private bool hayConexionTCP = false;
        private string ip = "";
        private string host = "";
        private int port = 0;

        public API(ComunicadorSerie com)
        {
            this.com = com;
        }

        public void Iniciar()
        {
            Detener();

            try
            {
                EnviarComandoOK(ComandosSIM800L.ModuloActivo(), maxIntentos: 5);
                Debug.WriteLine("Sim800 Iniciado");
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo conectar con el dispositivo luego de 5 intentos: " + ex.Message);
            }
        }

        public void ActualizarEstado()
        {
            EstaConectadoRedAPN();

            ObtenerDireccionIP();

            ObtenerCalidadSenial();
        }

        private bool ObtenerCalidadSenial()
        {
            try
            {
                var res = EnviarComandoOK(ComandosSIM800L.CalidadSenial());
                this.calidadSenial = LimpiarString(res.Replace("AT+CSQ\r\r\n+CSQ: ", ""));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool EstaConectadoRedAPN()
        {
            try
            {
                EnviarComandoOK(ComandosSIM800L.EstaConectado());
                this.conectado = true;
                return true;
            }
            catch (Exception)
            {
                this.conectado = false;
                return false;
            }
        }

        private bool ObtenerDireccionIP()
        {
            try
            {
                var res = this.com.EnviarComando(ComandosSIM800L.ObtenerDireccionIP());
                this.ip = LimpiarString(res.Replace("AT+CIFSR\r\r\n", ""));
                return true;
            }
            catch (Exception)
            {
                this.ip = "-";
                return false;
            }
        }

        public void ConectarAPN(string apn, string apnUsuario, string apnPassword)
        {
            if (string.IsNullOrEmpty(apn) || string.IsNullOrEmpty(apnUsuario) || string.IsNullOrEmpty(apnPassword))
                throw new Exception("Las credenciales no pueden estar vacias");

            this.apn = apn;
            this.apnUsuario = apnUsuario;
            this.apnPassword = apnPassword;

            this.com.EnviarComando(ComandosSIM800L.ConfigurarAPN(apn, apnUsuario, apnPassword), timeoutMilis: 5_000);

            if (EstaConectadoRedAPN() == false)
                throw new Exception($"No se pudo conectar a la red: {apn}, usr: {apnUsuario}, pass: {apnPassword}");

            this.com.EnviarComando(ComandosSIM800L.IniciarConexionGPRS(), timeoutMilis: 30_000);

            ObtenerDireccionIP();
        }

        public void IniciarClienteTCP(string host, int port)
        {
            try
            {
                this.host = host;
                this.port = port;
                this.com.EnviarComando(ComandosSIM800L.IniciarConexionTCP(host, port), timeoutMilis: 30_000);
                hayConexionTCP = true;
            }
            catch (Exception ex)
            {
                hayConexionTCP = false;
                throw ex;
            }
        }

        public void DetenerClienteTCP()
        {
            if (hayConexionTCP == false)
                return;

            try
            {
                this.com.EnviarComando(ComandosSIM800L.DetenerConexionTCP());
                hayConexionTCP = false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string EnviarComandoOK(string comando, int timeoutMilisegundos = 5000, int maxIntentos = 1)
        {
            int intento = 0;
            bool ok = false;
            string res = "";
            while (intento++ < maxIntentos)
            {
                res = this.com.EnviarComando(comando, timeoutMilisegundos);
                if (res.Contains(ComandosSIM800L.OK) && res.Contains("ERROR") == false)
                {
                    ok = true;
                    break;
                }
            }

            if (ok == false)
            {
                throw new Exception($"Error con el comando {comando}: " + res);
            }

            return res;
        }

        //public void EnviarPayload(byte[] payload, int timeoutMilisegundos = 5000, int sleepMilis = 1000)
        //{
        //    if (hayConexionTCP == false)
        //        throw new Exception("No hay ninguna conexion TCP activa");

        //    lock (lockWriteBuffer)
        //    {
        //        var resTCP = EnviarComando(ComandosSIM800L.EnviarDatosTCP(payload.Length), timeoutMilisegundos, sleepMilis);
        //        if (resTCP.ToLower().Contains("error") || resTCP.Contains(">") == false)
        //            throw new Exception("No se pueden enviar datos TCP");

        //        serialPort.Write(payload, 0, payload.Length);
        //        ComandoEnviado?.Invoke(Encoding.UTF8.GetString(payload, 0, payload.Length));

        //        Thread.Sleep(sleepMilis);

        //        EnviarCTRL_Z(sleepMilis);

        //        DateTime inicio = DateTime.UtcNow;
        //        byte[] buffer = new byte[1024 * 2]; // Ajusta el tamaño según sea necesario

        //        while ((DateTime.UtcNow - inicio).TotalMilliseconds < timeoutMilisegundos)
        //        {
        //            int bytesDisponibles = serialPort.BytesToRead;

        //            if (bytesDisponibles > 0)
        //            {
        //                int bytesRead = serialPort.Read(buffer, 0, bytesDisponibles);
        //                var res = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //                RespuestaRecibida?.Invoke(res);
        //                return;
        //            }

        //            Thread.Sleep(100); // Pequeño tiempo de espera antes de volver a verificar
        //        }

        //        throw new Exception("Timeout");
        //    }
        //}

        private object lock_ctrlz = new object();
        public void EnviarCTRL_Z(int sleepMilis = 1000)
        {
            lock (lock_ctrlz)
            {
                byte[] buffer = new byte[1];
                buffer[0] = 26; // ^Z
                this.com.Enviar(buffer, sleepMilis);
            }
        }

        private void Detener()
        {
            DetenerClienteTCP();

            this.com.Dispose();
        }

        public void Restart()
        {
            EnviarComandoOK(ComandosSIM800L.Restart(), maxIntentos: 5);
        }

        public void Dispose()
        {
            watchdog?.Dispose();

            try
            {
                Detener();
            }
            catch (Exception)
            {
                // Best Efford por ahora
            }
        }

        private string LimpiarString(string str) => str.Replace("\r\n", "").Replace("OK", "");

    }
}

