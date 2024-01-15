using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SIM800L
{
    /// <summary>
    /// Comandos de https://www.elecrow.com/wiki/images/2/20/SIM800_Series_AT_Command_Manual_V1.09.pdf
    /// </summary>
    public static class Sim800lCommands
    {
        public const string OK = "OK\r\n";

        // Método para verificar si el módulo está activo
        public static string ModuloActivo()
        {
            return "AT";
        }

        // Método para obtener la intensidad de señal
        public static string IntensidadSenal()
        {
            return "AT+CSQ";
        }

        // Método para verificar la conexión
        public static string EstaConectado()
        {
            return "AT+CREG?";
        }

        // Método para conectar a una red APN
        public static string ConectarAPN(string apn)
        {
            return $"AT+CSTT=\"{apn}\"";
        }

        // Método para activar la funcionalidad completa del módem
        public static string ActivarFuncionalidadCompleta()
        {
            return "AT+CFUN=1";
        }

        // Método para verificar el estado del PIN de la tarjeta SIM
        public static string VerificarEstadoPIN()
        {
            return "AT+CPIN?";
        }

        // Método para establecer el APN, nombre de usuario y contraseña
        public static string ConfigurarAPN(string apn, string usuario, string contraseña)
        {
            return $"AT+CSTT=\"{apn}\",\"{usuario}\",\"{contraseña}\"";
        }

        // Método para iniciar la conexión inalámbrica GPRS
        public static string IniciarConexionGPRS()
        {
            return "AT+CIICR";
        }

        // Método para obtener la dirección IP asignada al módulo
        public static string ObtenerDireccionIP()
        {
            return "AT+CIFSR";
        }

        // Método para iniciar una conexión TCP, Pagina 222
        public static string IniciarConexionTCP(string domainName, int puerto)
        {
            return $"AT+CIPSTART=\"TCP\",\"{domainName}\",{puerto}";
        }

        // Método para enviar datos por la conexión TCP
        public static string EnviarDatosTCP(int longitud)
        {
            return $"AT+CIPSEND={longitud}";
        }

        // Pagina 227
        public static string DetenerConexionTCP()
        {
            return $"AT+CIPCLOSE=0";
        }

        // Método para realizar una solicitud HTTP GET
        public static string RealizarSolicitudGET(string url)
        {
            return $"GET {url} HTTP/1.0";
        }
    }
}
