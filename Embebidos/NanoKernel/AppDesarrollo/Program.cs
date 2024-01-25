using Modulo.SIM800L;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace AppDesarrollo
{
    public class Program
    {
        private static ModuloBlinkLed blinker;
        public const string APN = "internet.movil";
        public const string APN_USER = "internet";
        public const string APN_PASSWORD = "internet";
        public const string TCP_Server_URL = "190.229.242.238";
        public const int TCP_Server_IP = 37000;

        static bool saltear = false;
        static API api;
        public static void Main()
        {
            try
            {
                blinker = new ModuloBlinkLed(2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            bool activo = true;
            while (activo)
            {
                try
                {
                    if (saltear == false)
                    {
                        Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
                        Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);
                        SerialPort serial = new SerialPort("COM2", 115200);
                        serial.Open();

                        var comSerie = new ComunicadorSerie(serial);
                        api = new API(comSerie);
                        saltear = true;
                    }

                    api.Restart();

                    Thread.Sleep(10000);

                    api.Iniciar();

                    api.ConectarAPN(APN, APN_USER, APN_PASSWORD);

                    api.IniciarClienteTCP(TCP_Server_URL, TCP_Server_IP);
                   
                    while(api.EnviarComando("AT+CIPSTATUS").Contains("OK") == false)
                    {
                        Thread.Sleep(1000);
                    }

                    api.EnviarPayload(Encoding.UTF8.GetBytes("HOLA DESDE ESP32"));
                    Thread.Sleep(10000);
                    blinker.OK();
                    activo = false;
                }
                catch (Exception ex)
                {
                    blinker.Error();
                    Debug.WriteLine(ex.Message);
                } 
            }

            Thread.Sleep(Timeout.Infinite);
        }

    }
}
