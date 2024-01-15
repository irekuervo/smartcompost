using System.IO.Ports;
using System.Text;
using Test.SIM800L;

public class ModuloSIM800L : IDisposable
{
    public event Action<string> ComandoEnviado;
    public event Action<string> RespuestaRecibida;
    public event Action<string> OnEstadoActualizado;

    public string IP => ip;

    private SerialPort serialPort;
    private System.Threading.Timer watchdog;

    private string apn;
    private string apnUsuario;
    private string apnPassword;

    private string calidadSenial = "-";
    private string puertoCOM;
   
    // TCP
    private bool hayConexionTCP = false;
    private string ip;
    private string host = "";
    private int port = 0;

    public ModuloSIM800L()
    {

    }

    public ModuloSIM800L(string apn, string apnUsuario, string apnPassword)
    {
        SetAPN(apn, apnUsuario, apnPassword);
    }

    public void SetAPN(string apn, string apnUsuario, string apnPassword)
    {
        if (string.IsNullOrWhiteSpace(apn) || string.IsNullOrWhiteSpace(apnUsuario) || string.IsNullOrWhiteSpace(apnPassword))
            throw new Exception("Las credenciales no pueden estar vacias");

        this.apn = apn;
        this.apnUsuario = apnUsuario;
        this.apnPassword = apnPassword;
    }

    public void Iniciar(string puertoSerie)
    {
        Detener();

        IniciarComunicacionSerie(puertoSerie);

        InicializarModulo();

        //watchdog?.Dispose();
        //watchdog = new System.Threading.Timer((object? state) =>
        //{
        //    try
        //    {
        //        EnviarComandoOK(Sim800lCommands.OK);
        //        calidadSenial = EnviarComandoOK(Sim800lCommands.IntensidadSenal());
        //        OnEstadoActualizado?.Invoke(calidadSenial);
        //    }
        //    catch (Exception ex)
        //    {
        //        Iniciar(puertoCOM);
        //    }
        //}, null, 5000, 5000);
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

    private void InicializarModulo()
    {
        // Vemos que esté OK
        EnviarComandoOK(Sim800lCommands.ModuloActivo());

        // Activar la funcionalidad completa del módem
        EnviarComandoOK(Sim800lCommands.ActivarFuncionalidadCompleta());

        // Verificar el estado del PIN de la tarjeta SIM
        EnviarComandoOK(Sim800lCommands.VerificarEstadoPIN());

        // Paso 3: Configurar el APN, nombre de usuario y contraseña
        EnviarComandoOK(Sim800lCommands.ConfigurarAPN(apn, apnUsuario, apnPassword));

        // Iniciar la conexión GPRS
        EnviarComandoOK(Sim800lCommands.IniciarConexionGPRS());

        // Obtener la dirección IP asignada al módulo
        this.ip = EnviarComando(Sim800lCommands.ObtenerDireccionIP());
    }

    public void IniciarClienteTCP(string host, int port)
    {
        try
        {
            this.host = host;
            this.port = port;
            EnviarComando(Sim800lCommands.IniciarConexionTCP(host, port));
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
            EnviarComando(Sim800lCommands.DetenerConexionTCP());
            hayConexionTCP = false;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public string EnviarPayload(byte[] payload)
    {
        if (hayConexionTCP == false)
            throw new Exception("No hay ninguna conexion TCP activa");

        EnviarComando(Sim800lCommands.EnviarDatosTCP(payload.Length));

        // Paso 8: Realizar una solicitud HTTP GET
        respuesta = EnviarComando(Sim800lCommands.EnviarDatosTCP(url));

        // Cerrar la conexión serial cuando hayas terminado
        Detener();

        return respuesta;
    }

    private string EnviarComandoOK(string comando, int timeoutMilisegundos = 5000)
    {
        var res = EnviarComando(comando, timeoutMilisegundos);
        if (res.Contains(Sim800lCommands.OK) == false)
        {
            throw new Exception($"Error con el comando {comando}: " + res);
        }

        return res;
    }

    private object lockObject = new object();
    private string EnviarComando(string comando, int timeoutMilisegundos = 5000)
    {
        lock (lockObject)
        {
            var linea = comando + "\r";
            serialPort.Write(linea);
            ComandoEnviado?.Invoke(linea);

            DateTime inicio = DateTime.Now;
            byte[] buffer = new byte[1024 * 2]; // Ajusta el tamaño según sea necesario

            while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMilisegundos)
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

    private void Detener()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    public void Dispose()
    {
        watchdog?.Dispose();
        Detener();
    }
}