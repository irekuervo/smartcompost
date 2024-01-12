
using System.Net.Sockets;
using System.Net;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        // Establecer la dirección IP y el puerto en el que el servidor escuchará
        IPAddress ipAddress = IPAddress.Any;
        int port = 37000;

        // Crear un objeto TcpListener
        TcpListener listener = new TcpListener(ipAddress, port);

        try
        {
            // Iniciar la escucha del servidor
            listener.Start();
            Console.WriteLine($"Servidor escuchando en {ipAddress}:{port}");

            while (true)
            {
                // Aceptar la conexión de un cliente
                TcpClient client = listener.AcceptTcpClient();

                // Iniciar un hilo para manejar la comunicación con el cliente
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Detener el TcpListener al salir
            listener.Stop();
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient tcpClient = (TcpClient)obj;

        // Obtener la dirección IP del cliente
        string clientAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
        Console.WriteLine($"Cliente conectado desde {clientAddress}");

        // Obtener el flujo de red del cliente
        NetworkStream clientStream = tcpClient.GetStream();

        // Buffer para almacenar los datos recibidos
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                // Leer datos del cliente
                int bytesRead = clientStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    break; // El cliente ha cerrado la conexión
                }

                // Convertir los datos a una cadena y mostrarlos en la consola
                string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Datos recibidos: {dataReceived}");

                // Puedes realizar operaciones con los datos recibidos aquí

                // Enviar una respuesta al cliente (opcional)
                byte[] response = Encoding.UTF8.GetBytes("Mensaje recibido correctamente");
                clientStream.Write(response, 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la comunicación con el cliente: {ex.Message}");
        }
        finally
        {
            // Cerrar la conexión con el cliente
            tcpClient.Close();
            Console.WriteLine($"Cliente desconectado desde {clientAddress}");
        }
    }
}
