using System.Net.Sockets;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        // Dirección IP del servidor (debe ser la IP pública del router si estás usando port forwarding)
        string serverIp = "190.229.242.238";
        int serverPort = 37000;

        // Crear un cliente TCP
        TcpClient client = new TcpClient(serverIp, serverPort);

        while (true)
        {
            try
            {
                // Obtener el flujo de red del cliente
                NetworkStream clientStream = client.GetStream();

                // Mensaje que enviarás al servidor
                string message = "Hola desde el cliente";

                // Convertir el mensaje a un array de bytes y enviarlo al servidor
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                clientStream.Write(messageBytes, 0, messageBytes.Length);

                // Buffer para almacenar la respuesta del servidor
                byte[] buffer = new byte[1024];

                // Leer la respuesta del servidor
                int bytesRead = clientStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Respuesta del servidor: {response}");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}