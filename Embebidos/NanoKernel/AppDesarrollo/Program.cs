using System;
using System.Diagnostics;
using System.Threading;

namespace AppDesarrollo
{
    public class Program
    {
        private static ModuloBlinkLed blinker;
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

            //try
            //{
            //    Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
            //    Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);
            //    SerialPort serial = new SerialPort("COM2", 115200);
            //    //serial.Open();

            //    var comSerie = new ComunicadorSerie(serial);
            //    var api = new API(comSerie);

            //    //api.Iniciar();
            //}
            //catch (Exception ex)
            //{
            //    blinker.Error();
            //    Debug.WriteLine(ex.Message);
            //}

            Thread.Sleep(Timeout.Infinite);
        }

    }
}
