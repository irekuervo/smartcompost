using Modulo.SIM800L;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace AppDesarrollo
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("hola");

            new ModuloBlinkLed(2);

            try
            {
                Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
                Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);
                SerialPort serial = new SerialPort("COM2", 115200);
                serial.Open();

                var comSerie = new ComunicadorSerie(serial);
                var api = new API(comSerie);

                api.Iniciar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Thread.Sleep(Timeout.Infinite);
        }

    }
}
