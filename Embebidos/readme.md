*** Paso 0: instalar vs2022 o dotnet 6***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 1: instalar en visual studio la extension: .Net Nanoframework Extension***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 2: instalar tools ***
dotnet tool install -g nanoff

*** Paso 3: Para actualizar el firmaware ***
nanoff --update --target ESP32_PSRAM_REV0 --serialport COM31

*** Paso 3 bis (opcional): Depploy manual de .bin *** 
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
- COM1 no se puede usar para debugear. Todavia no se si en modo release se puede usar!

*** SIM800L ***
- SIM800L anda con 2 capas de 2200nF (2,2uF) sobre el proto pegado al modulo (bien cerca) y con 4V en la fuente de alimentacion
- Para hablarle en crudo, uso un ESP32 con el pin EN conectado a GND (lo puentea y hablo directo con el conversor serie)
- La papa para TCP esta en esta nota de aplicacion del fabricante: https://www.waveshare.com/w/upload/2/25/SIM800_Series_TCPIP_Application_Note_V1.03.pdf





*** c# ***
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
