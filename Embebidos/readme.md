*** Paso 0: instalar vs2022 o dotnet 6***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 1: instalar tools ***
dotnet tool install -g nanoff

*** Paso 2: Para actualizar el firmaware ***
nanoff --update --target ESP32_PSRAM_REV0 --serialport COM31

*** Paso 2 bis (opcional): Depploy manual de .bin *** 
nanoff --target ESP32_PSRAM_REV0 --serialport COM12 --deploy --image "E:\GitHub\nf-Samples\samples\Blinky\Blinky\bin\Debug\Blinky.bin"

*** Paso 4: Manejo proyecto .Net *** 
Abrir un proyecto Blank Application (nanoFramework), y actualizar todos los paquetes nuget que hagan falta (con el preview checkbox ticked)

*** Paso 5: Deploy proyecto .Net *** 
Comprobar que en el explorador aparezca el device, y que se le puedan pedir sus "Device Capabilities"
Darle run si quiero debuggear, o deploy si solo quiero deployar

*** BITACORA ***
- Al parecer, en las placas que tienen el CH340 no funca el debugger en VS, pero lo demas si
- Con el cp210x va como piña todo

*** Comunicacion Serie ***
- Al parecer se simulan los COM, todavia no entiendo si son fisicos o virtuales, pero puedo preguntar cuales estan disponibles

// get available ports
 var ports = SerialPort.GetPortNames();
 Debug.WriteLine("Puertos COM disponibles: ");
 foreach (string port in ports)
 {
     Debug.WriteLine($" {port}");
 }

 // Here setting pin 32 for RX and pin 33 for TX both on COM2
 Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
 Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);

 SerialPort sp = new SerialPort("COM2");
 sp.ReadBufferSize = 1024;
 sp.DataReceived += Sp_DataReceived;
 sp.Open();

 Thread.Sleep(Timeout.Infinite);
