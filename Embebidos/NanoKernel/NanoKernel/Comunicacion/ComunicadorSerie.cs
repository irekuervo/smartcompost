using nanoFramework.Hardware.Esp32;
using NanoKernel.Loggin;
using System.Diagnostics;
using System.IO.Ports;

namespace NanoKernel.Comunicacion
{

    public class ComunicadorSerie : Comunicador
    {
        private const int RX_PORT = 32;
        private const int TX_PORT = 33;
        private const char SERIAL_BUFFER_WATCHAR = '\r';
        private const int SERIAL_BUFFER_SIZE = 256;
        private const int SERIAL_READ_TIMEOUT = 4000;
        private const int SERIAL_WRITE_TIMEOUT = 4000;
        private const string PORT = "COM2";
        private const int BAUDRATE = 9600;

        public override event OnDataRecieved DataRecieved;

        private SerialPort serial;
        private byte[] SERIAL_BUFFER = new byte[SERIAL_BUFFER_SIZE];
        private int bytes_read = 0;

        public ComunicadorSerie()
        {
            // Here setting pin 32 for RX and pin 33 for TX both on COM2
            Configuration.SetPinFunction(RX_PORT, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(TX_PORT, DeviceFunction.COM2_TX);

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
            throw new System.NotImplementedException();
        }

        public override void SendAsync(byte[] data, int offset, int count)
        {
            serial.Write(data, offset, count);
        }

        private void Serial_OnDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType != SerialData.WatchChar)
            {
                return;
            }

            SerialPort serialDevice = (SerialPort)sender;

            if (serialDevice.BytesToRead > SERIAL_BUFFER_SIZE)
            {
                Logger.Log("No puedo leer tanto tamanio: " + serialDevice.BytesToRead);
                return;
            }

            if (serialDevice.BytesToRead > 0)
            {
                bytes_read = serialDevice.Read(SERIAL_BUFFER, 0, serialDevice.BytesToRead);

                Logger.Log($"{serialDevice.PortName}: {bytes_read} bytes recieved");

                DataRecieved?.Invoke(SERIAL_BUFFER, 0, bytes_read);
            }
        }
    }
}
