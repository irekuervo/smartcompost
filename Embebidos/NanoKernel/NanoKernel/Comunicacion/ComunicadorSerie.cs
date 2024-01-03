using nanoFramework.Hardware.Esp32;
using NanoKernel.Loggin;
using NanoKernel.Modulos;
using System;
using System.IO.Ports;
using System.Threading;

namespace NanoKernel.Comunicacion
{

    public class ComunicadorSerie : Comunicador
    {
        public override event OnDataRecieved DataRecieved;

        private const string PORT = "COM2";
        private const int RX_PORT = 32;
        private const int TX_PORT = 33;
        private const char SERIAL_BUFFER_WATCHAR = '\r';
        private const int SERIAL_BUFFER_SIZE = 256;
        private const int SERIAL_READ_TIMEOUT = 4000;
        private const int SERIAL_WRITE_TIMEOUT = 4000;
        private const int BAUDRATE = 9600;

        private SerialPort serial;
        private byte[] SERIAL_BUFFER = new byte[SERIAL_BUFFER_SIZE];

        public ComunicadorSerie()
        {
            // Here setting pin 32 for RX and pin 33 for TX both on COM2
            Configuration.SetPinFunction(RX_PORT, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(TX_PORT, DeviceFunction.COM2_TX);

            string[] portsAvailable = SerialPort.GetPortNames();
            if (portsAvailable.Length == 0)
                throw new Exception("No hay ningun puerto disponible");

            serial = new SerialPort(PORT, BAUDRATE);
            serial.ReadBufferSize = SERIAL_BUFFER_SIZE;
            serial.ReadTimeout = SERIAL_READ_TIMEOUT;
            serial.WriteTimeout = SERIAL_WRITE_TIMEOUT;
            serial.WatchChar = SERIAL_BUFFER_WATCHAR;
            serial.DataReceived += Serial_OnDataRecieved;

            Logger.Log($"Creando comunicador serie {PORT}\r\n" +
                $"baudrate={BAUDRATE}\r\n" +
                $"parity=None\r\n" +
                $"databits=8\r\n" +
                $"stopbits=1\r\n" +
                $"read_timeout={SERIAL_READ_TIMEOUT}\r\n" +
                $"write_timeout={SERIAL_WRITE_TIMEOUT}\r\n" +
                $"buffer_size={SERIAL_BUFFER_SIZE}");
            serial.Open();
        }

        [Servicio("puertos")]
        public void ObtenerPuertosCom()
        {
            // get available ports
            var ports = SerialPort.GetPortNames();
            Logger.Log("Puertos COM disponibles: ");
            foreach (string port in ports)
            {
                Logger.Log($" {port}");
            }
        }

        public override byte[] Send(byte[] data, int offset, int count)
        {
            SendAsync(data, offset, count);

            while (serial.BytesToRead == 0)
                Thread.Sleep(0);

            byte[] res = new byte[ReadData(serial)];

            Array.Copy(SERIAL_BUFFER, 0, res, 0, res.Length);

            return res;
        }

        public override void SendAsync(byte[] data, int offset, int count)
        {
            serial.Write(data, offset, count);
            Logger.Log($"{serial.PortName}: {count} bytes sent");
        }

        private void Serial_OnDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType != SerialData.WatchChar)
            {
                return;
            }

            SerialPort serialDevice = (SerialPort)sender;

            DataRecieved?.Invoke(SERIAL_BUFFER, 0, ReadData(serialDevice));
        }

        private int ReadData(SerialPort serialDevice)
        {
            if (serialDevice.BytesToRead > SERIAL_BUFFER_SIZE)
            {
                Logger.Log("No puedo leer tanto tamanio: " + serialDevice.BytesToRead);
                return 0;
            }

            if (serialDevice.BytesToRead == 0)
                return 0;

            int bytes_read = serialDevice.Read(SERIAL_BUFFER, 0, serialDevice.BytesToRead);
            Logger.Log($"{serialDevice.PortName}: {bytes_read} bytes recieved");
            return bytes_read;
        }

        public override void Dispose()
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
                serial.Dispose();
            }
        }
    }
}
