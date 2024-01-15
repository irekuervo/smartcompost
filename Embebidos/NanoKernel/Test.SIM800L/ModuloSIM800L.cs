using System.IO.Ports;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Test.SIM800L;

public class ModuloSIM800L : IDisposable
{
    public event Action<string> ComandoEnviado;
    public event Action<string> RespuestaRecibida;
    public event Action<string> OnEstadoActualizado;

    public string IP => ip;
    public bool Conectado => conectado;
    public string CalidadSenial => calidadSenial;

    private SerialPort serialPort;
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

    public ModuloSIM800L()
    {

    }

    public ModuloSIM800L(string apn, string apnUsuario, string apnPassword)
    {
        ConectarAPN(apn, apnUsuario, apnPassword);
    }

    public void Iniciar(string puertoSerie)
    {
        Detener();

        IniciarComunicacionSerie(puertoSerie);

        int intentos = 0;
        bool ok = false;
        // Nos tratamos de comunicar con el modulo, puede tomar algunos intentos para sincronizar el clock
        while (intentos++ < 5)
        {
            try
            {
                EnviarComandoOK(ComandosSIM800L.ModuloActivo());
                ok = true;
                break;
            }
            catch (Exception)
            {
                Thread.Sleep(500);
            }
        }

        if (ok == false)
            throw new Exception("No se pudo establer conexion con el modulo");

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
            var res = EnviarComando(ComandosSIM800L.ObtenerDireccionIP());
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
        if (string.IsNullOrWhiteSpace(apn) || string.IsNullOrWhiteSpace(apnUsuario) || string.IsNullOrWhiteSpace(apnPassword))
            throw new Exception("Las credenciales no pueden estar vacias");

        this.apn = apn;
        this.apnUsuario = apnUsuario;
        this.apnPassword = apnPassword;

        // Activar la funcionalidad completa del módem
        //EnviarComando(ComandosSIM800L.ActivarFuncionalidadCompleta());

        // Verificar el estado del PIN de la tarjeta SIM
        //EnviarComando(ComandosSIM800L.VerificarEstadoPIN(), timeoutMilis: 5_000);

        // Paso 3: Configurar el APN, nombre de usuario y contraseña
        EnviarComando(ComandosSIM800L.ConfigurarAPN(apn, apnUsuario, apnPassword), timeoutMilis: 5_000);

        if (EstaConectadoRedAPN() == false)
            throw new Exception($"No se pudo conectar a la red: {apn}, usr: {apnUsuario}, pass: {apnPassword}");

        // Iniciar la conexión GPRS
        EnviarComando(ComandosSIM800L.IniciarConexionGPRS(), timeoutMilis: 30_000);

        ObtenerDireccionIP();
    }

    public void IniciarClienteTCP(string host, int port)
    {
        try
        {
            this.host = host;
            this.port = port;
            EnviarComando(ComandosSIM800L.IniciarConexionTCP(host, port), timeoutMilis: 30_000);
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
            EnviarComando(ComandosSIM800L.DetenerConexionTCP());
            hayConexionTCP = false;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void IniciarComunicacionSerie(string puertoSerie)
    {
        puertoCOM = puertoSerie;

        serialPort = new SerialPort(puertoCOM);
        // Configura el puerto serie según tus necesidades
        serialPort.BaudRate = 9600;
        serialPort.Parity = Parity.None;
        serialPort.StopBits = StopBits.One;
        serialPort.DataBits = 8;
        serialPort.Handshake = Handshake.None;
        serialPort.Open();
    }

    private string EnviarComandoOK(string comando, int timeoutMilisegundos = 5000, int sleepMilis = 1000)
    {
        var res = EnviarComando(comando, timeoutMilisegundos, sleepMilis);
        if (res.Contains(ComandosSIM800L.OK) == false || res.Contains("ERROR"))
        {
            throw new Exception($"Error con el comando {comando}: " + res);
        }

        return res;
    }

    private object lockObject = new object();
    public string EnviarComando(string comando, int timeoutMilis = 5000, int sleepMilis = 2000)
    {
        lock (lockObject)
        {
            var linea = comando + "\r";
            serialPort.Write(linea);
            ComandoEnviado?.Invoke(linea);

            DateTime inicio = DateTime.Now;
            byte[] buffer = new byte[1024 * 2]; // Ajusta el tamaño según sea necesario

            Thread.Sleep(sleepMilis);

            while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMilis)
            {
                int bytesDisponibles = serialPort.BytesToRead;

                if (bytesDisponibles > 0)
                {
                    int bytesRead = serialPort.Read(buffer, 0, Math.Min(bytesDisponibles, buffer.Length));
                    string res = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    RespuestaRecibida?.Invoke(res);

                    return res;
                }

                Thread.Sleep(100); // Pequeño tiempo de espera antes de volver a verificar
            }

            throw new Exception("Timeout");
        }
    }

    public void EnviarPayload(byte[] payload, int timeoutMilisegundos = 5000, int sleepMilis = 1000)
    {
        if (hayConexionTCP == false)
            throw new Exception("No hay ninguna conexion TCP activa");

        lock (lockObject)
        {
            EnviarComandoOK(ComandosSIM800L.EnviarDatosTCP(payload.Length), timeoutMilisegundos, sleepMilis);

            serialPort.Write(payload, 0, payload.Length);
            ComandoEnviado?.Invoke(Encoding.UTF8.GetString(payload));

            Thread.Sleep(sleepMilis);

            byte[] buffer = new byte[1];
            buffer[0] = 26; // ^Z
            serialPort.Write(buffer, 0, 1);

            DateTime inicio = DateTime.Now;
            buffer = new byte[1024 * 2]; // Ajusta el tamaño según sea necesario

            while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMilisegundos)
            {
                int bytesDisponibles = serialPort.BytesToRead;

                if (bytesDisponibles > 0)
                {
                    int bytesRead = serialPort.Read(buffer, 0, Math.Min(bytesDisponibles, buffer.Length));
                    var res = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    RespuestaRecibida?.Invoke(res);
                }

                Thread.Sleep(100); // Pequeño tiempo de espera antes de volver a verificar
            }

            throw new Exception("Timeout");
        }
    }

    private void Detener()
    {
        DetenerClienteTCP();

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    public void Restart()
    {
        EnviarComando(ComandosSIM800L.Restart());
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

    private string LimpiarString(string str) => str.Replace("\r\n", "").Replace("OK","");

}