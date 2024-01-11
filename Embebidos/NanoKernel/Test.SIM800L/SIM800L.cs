
using System.IO.Ports;
using Test.SIM800L;

public class SIM800L
{
    private SerialPort serialPort;

    public SIM800L(string puertoSerie)
    {
        serialPort = new SerialPort(puertoSerie);
        // Configura el puerto serie según tus necesidades
        serialPort.BaudRate = 9600;
        serialPort.Parity = Parity.None;
        serialPort.StopBits = StopBits.One;
        serialPort.DataBits = 8;
        serialPort.Handshake = Handshake.None;
        serialPort.Open();
    }

    public void CerrarConexion()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    public string RealizarRequestGET(string url)
    {
        // Paso 1: Activar la funcionalidad completa del módem
        string respuesta = EnviarComando(Sim800lCommands.ActivarFuncionalidadCompleta());

        // Paso 2: Verificar el estado del PIN de la tarjeta SIM
        respuesta = EnviarComando(Sim800lCommands.VerificarEstadoPIN());

        // Paso 3: Configurar el APN, nombre de usuario y contraseña
        respuesta = EnviarComando(Sim800lCommands.ConfigurarAPN("airtelgprs.com", "", ""));

        // Paso 4: Iniciar la conexión GPRS
        respuesta = EnviarComando(Sim800lCommands.IniciarConexionGPRS());

        // Paso 5: Obtener la dirección IP asignada al módulo
        respuesta = EnviarComando(Sim800lCommands.ObtenerDireccionIP());

        // Paso 6: Iniciar una conexión TCP
        respuesta = EnviarComando(Sim800lCommands.IniciarConexionTCP("exploreembedded.com", 80));

        // Paso 7: Enviar datos por la conexión TCP
        respuesta = EnviarComando(Sim800lCommands.EnviarDatosTCP(63));

        // Paso 8: Realizar una solicitud HTTP GET
        respuesta = EnviarComando(Sim800lCommands.RealizarSolicitudGET(url));

        // Cerrar la conexión serial cuando hayas terminado
        CerrarConexion();

        return respuesta;
    }

    private string EnviarComando(string comando)
    {
        // Enviar el comando y esperar la respuesta
        serialPort.Write(comando + "\r");
        System.Threading.Thread.Sleep(1000); // Ajusta según sea necesario
        return serialPort.ReadExisting();
    }
}