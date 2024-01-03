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
        public bool IsOpen => serial == null ? false : serial.IsOpen;

        private const char SERIAL_BUFFER_WATCHAR = '\r';
        private const int DEFAULT_SERIAL_BUFFER_SIZE = 256;
        private const int DEFAULT_BAUDRATE = 9600;
        private const int SERIAL_READ_TIMEOUT = 4000;
        private const int SERIAL_WRITE_TIMEOUT = 4000;

        private SerialPort serial;
        private byte[] buffer;

        public ComunicadorSerie(string port, int rxPin, int txPin, int baudrate = DEFAULT_BAUDRATE, int bufferSize = DEFAULT_SERIAL_BUFFER_SIZE)
        {
            if (port == "COM1")
            {
                Configuration.SetPinFunction(rxPin, DeviceFunction.COM1_RX);
                Configuration.SetPinFunction(txPin, DeviceFunction.COM1_TX);
            }
            else if (port == "COM2")
            {
                Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);
                Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
            }
            else if (port == "COM3")
            {
                Configuration.SetPinFunction(rxPin, DeviceFunction.COM3_RX);
                Configuration.SetPinFunction(txPin, DeviceFunction.COM3_TX);
            }
            else
                throw new Exception("Puerto desconocido: " + port);

            serial = new SerialPort(port, baudrate);
            serial.ReadBufferSize = bufferSize;
            serial.WriteBufferSize = bufferSize;
            serial.ReadTimeout = SERIAL_READ_TIMEOUT;
            serial.WriteTimeout = SERIAL_WRITE_TIMEOUT;
            serial.WatchChar = SERIAL_BUFFER_WATCHAR;
            serial.DataReceived += Serial_OnDataRecieved;

            Logger.Log($"Creando comunicador serie {port}\r\n" +
                $"baudrate={baudrate}\r\n" +
                $"parity=None\r\n" +
                $"databits=8\r\n" +
                $"stopbits=1\r\n" +
                $"read_timeout={SERIAL_READ_TIMEOUT}\r\n" +
                $"write_timeout={SERIAL_WRITE_TIMEOUT}\r\n" +
                $"buffer_size={bufferSize}");

            buffer = new byte[bufferSize];
            serial.Open();
        }

        [Servicio("puertos")]
        public string ObtenerPuertosCom()
        {
            var ports = SerialPort.GetPortNames();
            string res = "Puertos COM disponibles: \r\n";
            foreach (string port in ports)
            {
                res += $"[{port}]\r\n";
            }
            return res;
        }

        public override byte[] Send(byte[] data, int offset, int count)
        {
            SendAsync(data, offset, count);
            DateTime start = DateTime.UtcNow;
            bool timeout = false;
            while (serial.BytesToRead == 0)
            {
                Thread.Sleep(10);
                if ((DateTime.UtcNow - start).TotalMilliseconds > SERIAL_READ_TIMEOUT)
                {
                    timeout = true;
                    break;
                }
            }

            if (timeout) { throw new Exception("Error de timeout"); }

            byte[] res = new byte[ReadData(serial)];

            Array.Copy(buffer, 0, res, 0, res.Length);

            return res;
        }

        public override void SendAsync(byte[] data, int offset, int count)
        {
            serial.Write(data, offset, count);
            Logger.Log($"{serial.PortName}: {count} bytes sent");
        }

        private void Serial_OnDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType != SerialData.WatchChar)
                {
                    return;
                }

                SerialPort serialDevice = (SerialPort)sender;

                DataRecieved?.Invoke(buffer, 0, ReadData(serialDevice));
            }
            catch (Exception)
            {
                Logger.Log("Error recibiendo datos");
            }
        }

        private int ReadData(SerialPort serialDevice)
        {
            if (serialDevice.BytesToRead > DEFAULT_SERIAL_BUFFER_SIZE)
            {
                Logger.Log("No puedo leer tanto tamanio: " + serialDevice.BytesToRead);
                return 0;
            }

            if (serialDevice.BytesToRead == 0)
                return 0;

            int bytes_read = serialDevice.Read(buffer, 0, serialDevice.BytesToRead);
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
