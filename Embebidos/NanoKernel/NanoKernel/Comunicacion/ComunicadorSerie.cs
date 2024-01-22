//using nanoFramework.Hardware.Esp32;
//using NanoKernel.Loggin;
//using NanoKernel.Modulos;
//using System;
//using System.IO.Ports;
//using System.Threading;

//namespace NanoKernel.Comunicacion
//{
//    public enum Baudrate
//    {
//        B_1200 = 1200,
//        B_9600 = 9600,
//        B_57600 = 57600,
//        B_115200 = 115200,
//    }

//    public class ComunicadorSerie : Canal
//    {
//        public bool IsOpen => serial == null ? false : serial.IsOpen;

//        private const char SERIAL_BUFFER_WATCHAR = '\r';
//        private const int DEFAULT_SERIAL_BUFFER_SIZE = 1024;
//        private const Baudrate DEFAULT_BAUDRATE = Baudrate.B_9600;
//        private const int DEFAULT_MTU = 4000;

//        private SerialPort serial;

//        public ComunicadorSerie(string port, int rxPin, int txPin, Baudrate baudrate = DEFAULT_BAUDRATE, int bufferSize = DEFAULT_SERIAL_BUFFER_SIZE)
//        {
//            if (port == "COM1")
//            {
//                Configuration.SetPinFunction(rxPin, DeviceFunction.COM1_RX);
//                Configuration.SetPinFunction(txPin, DeviceFunction.COM1_TX);
//            }
//            else if (port == "COM2")
//            {
//                Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);
//                Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
//            }
//            else if (port == "COM3")
//            {
//                Configuration.SetPinFunction(rxPin, DeviceFunction.COM3_RX);
//                Configuration.SetPinFunction(txPin, DeviceFunction.COM3_TX);
//            }
//            else
//                throw new Exception("Puerto desconocido: " + port);

//            serial = new SerialPort(port, (int)baudrate);
//            serial.ReadBufferSize = bufferSize;
//            serial.WriteBufferSize = bufferSize;
//            serial.WatchChar = SERIAL_BUFFER_WATCHAR;
//            serial.DataReceived += Serial_OnDataRecieved;

//            Logger.Log($"Creando comunicador serie {port}\r\n" +
//                $"baudrate={baudrate}\r\n" +
//                $"parity=None\r\n" +
//                $"databits=8\r\n" +
//                $"stopbits=1\r\n" +
//                $"buffer_size={bufferSize}");

//            serial.Open();
//        }

//        [Servicio("puertos")]
//        public string ObtenerPuertosCom()
//        {
//            var ports = SerialPort.GetPortNames();
//            string res = "Puertos COM disponibles: \r\n";
//            foreach (string port in ports)
//            {
//                res += $"[{port}]\r\n";
//            }
//            return res;
//        }

//        public override byte[] Send(byte[] data, int offset, int count)
//        {
//            SendAsync(data, offset, count);

//            int bytes_read = ReadData();


//            return buffer;
//        }

//        private void Serial_OnDataRecieved(object sender, SerialDataReceivedEventArgs e)
//        {
//            try
//            {
//                if (DataRecieved == null)
//                    return;

//                if (e.EventType != SerialData.WatchChar)
//                {
//                    return;
//                }

//                var bytes_read = ReadData();

               
//            }
//            catch (Exception)
//            {
//                Logger.Log("Error recibiendo datos");
//            }
//        }

//        private object writeLock = new object();
//        public override void SendAsync(byte[] data, int offset, int count)
//        {
//            lock (writeLock)
//            {
//                serial.Write(data, offset, count);
//                Logger.Log($"{serial.PortName}: {count} bytes sent");
//            }
//        }

//        private object readLock = new object();
//        private DateTime start;
//        private bool timeout = false;
//        private string errRead;
//        private int ReadData()
//        {
//            lock (readLock)
//            {
//                start = DateTime.UtcNow;
//                timeout = false;

//                while (serial.BytesToRead == 0)
//                {
//                    Thread.Sleep(10);
//                    if ((DateTime.UtcNow - start).TotalMilliseconds > Timeout)
//                    {
//                        timeout = true;
//                        break;
//                    }
//                }

//                if (timeout) { throw new Exception("Error de timeout"); }

//                if (serial.BytesToRead > DEFAULT_SERIAL_BUFFER_SIZE)
//                {
//                    errRead = "No puedo leer tanto tamanio: " + serial.BytesToRead;
//                    Logger.Log(errRead);
//                    throw new Exception(errRead);
//                }

//                var bytes_read = serial.Read(buffer, 0, serial.BytesToRead);
//                Logger.Log($"{serial.PortName}: {bytes_read} bytes recieved");

//                DataRecieved?.Invoke(buffer, 0, bytes_read);

//                return bytes_read;
//            }
//        }

//        public override void Dispose()
//        {
//            if (serial != null && serial.IsOpen)
//            {
//                serial.Close();
//                serial.Dispose();
//            }
//        }
//    }
//}
