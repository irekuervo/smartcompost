using System.IO.Ports;
using System.Text;
using Test.SIM800L;

public class ModuloSIM800L : IDisposable
{
    public event Action<string> ComandoEnviado;
    public event Action<string> RespuestaRecibida;
    public event Action<string> OnEstadoActualizado;

    private SerialPort serialPort;
    private System.Threading.Timer watchdog;

    private string apn;
    private string apnUsuario;
    private string apnPassword;

    private string calidadSenial = "-";
    private string puertoCOM;
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

        puertoCOM = puertoSerie;

        serialPort = new SerialPort(puertoCOM);
        // Configura el puerto serie según tus necesidades
        serialPort.BaudRate = 9600;
        serialPort.Parity = Parity.None;
        serialPort.StopBits = StopBits.One;
        serialPort.DataBits = 8;
        serialPort.Handshake = Handshake.None;
        serialPort.Open();

        EnviarComandoOK(Sim800lCommands.ModuloActivo());

        watchdog?.Dispose();
        watchdog = new System.Threading.Timer((object? state) =>
        {
            try
            {
                EnviarComandoOK(Sim800lCommands.OK);
                calidadSenial = EnviarComandoOK(Sim800lCommands.IntensidadSenal());
                OnEstadoActualizado?.Invoke(calidadSenial);
            }
            catch (Exception ex)
            {
                Iniciar(puertoCOM);
            }
        }, null, 5000, 5000);
    }

    public string RealizarRequestGET(string url, string host, int port)
    {
        // Paso 1: Activar la funcionalidad completa del módem
        string respuesta = EnviarComandoOK(Sim800lCommands.ActivarFuncionalidadCompleta());

        // Paso 2: Verificar el estado del PIN de la tarjeta SIM
        respuesta = EnviarComandoOK(Sim800lCommands.VerificarEstadoPIN());

        // Paso 3: Configurar el APN, nombre de usuario y contraseña
        respuesta = EnviarComandoOK(Sim800lCommands.ConfigurarAPN(apn, apnUsuario, apnPassword));

        // Paso 4: Iniciar la conexión GPRS
        respuesta = EnviarComandoOK(Sim800lCommands.IniciarConexionGPRS());

        // Paso 5: Obtener la dirección IP asignada al módulo
        respuesta = EnviarComando(Sim800lCommands.ObtenerDireccionIP());

        // Paso 6: Iniciar una conexión TCP
        respuesta = EnviarComando(Sim800lCommands.IniciarConexionTCP(host, port));

        // Paso 7: Enviar datos por la conexión TCP
        respuesta = EnviarComando(Sim800lCommands.EnviarDatosTCP(url.Length));

        // Paso 8: Realizar una solicitud HTTP GET
        respuesta = EnviarComando(Sim800lCommands.RealizarSolicitudGET(url));

        // Cerrar la conexión serial cuando hayas terminado
        Detener();

        return respuesta;
    }

    private string EnviarComandoOK(string comando, int timeoutMilisegundos = 5000)
    {
        EscribirComando(comando);

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
                if (res.Contains(Sim800lCommands.OK) == false)
                {
                    throw new Exception($"Error con el comando {comando}: " + res);
                }

                return res;
            }

            Thread.Sleep(100); // Pequeño tiempo de espera antes de volver a verificar
        }

        throw new Exception("Timeout");
    }

    private string EnviarComando(string comando, int timeoutMilisegundos = 5000)
    {
        EscribirComando(comando);

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

    private object lockObject = new object();
    private void EscribirComando(string comando)
    {
        lock (lockObject)
        {
            var linea = comando + "\r";
            serialPort.Write(linea);
            ComandoEnviado?.Invoke(linea);
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