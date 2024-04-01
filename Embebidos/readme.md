NANOFRAMEWORK (C# framework): https://www.nanoframework.net/ , https://github.com/nanoframework
NANOFF (Firmware flasher): https://github.com/nanoframework/nanoFirmwareFlasher

*** Paso 0: instalar vs2022 o dotnet 6***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 1: instalar en visual studio la extension: .Net Nanoframework Extension***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 2: instalar tools ***
dotnet tool install -g nanoff

*** Paso 3: Para actualizar el firmaware ***

Importante: si la placa tiene el CH340, obviar paso 2!!!

* 1) Elegir target y puerto COM. "ESP32_PSRAM_REV0" es el mas generico, por ahora anda

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM31

* 2) MANTENER APRETADO EL BOTON DE PROGRAMACION DEL ESP32 DURANTE TODO EL COMANDO (EL QUE DICE EN) !! Paciencia, pueden ser como 60 segs *
* 3) NO CONECTAR NADA EN LOS PINES TX/RX, ya que el debugger los usa *

*** Paso 3 bis (opcional): Depploy manual de .bin *** 
nanoff --target ESP32_PSRAM_REV0 --serialport COM12 --deploy --image "E:\GitHub\nf-Samples\samples\Blinky\Blinky\bin\Debug\Blinky.bin"

*** Paso 4: Manejo proyecto .Net *** 
Abrir un proyecto Blank Application (nanoFramework), y actualizar todos los paquetes nuget que hagan falta (con el preview checkbox ticked)

*** Paso 5: Deploy proyecto .Net *** 
Comprobar que en el explorador aparezca el device, y que se le puedan pedir sus "Device Capabilities"
Darle run si quiero debuggear, o deploy si solo quiero deployar

*** ERRORES COMUNES ***

- Version mismatch for mscorlib. Need v1.14.3.0 -> Updatear los paquetes de nuget!!

*** BITACORA ***
- Al parecer, en las placas que tienen el CH340 no funca el debugger en VS, pero lo demas si
- Con el cp210x va como piña todo
- Aveces deployeo algo que rompe todo y se autoreseta, y dejo de poder deployar. Ahi tengo que volver a instalar el firmware y probar de 0 (o deployar a mano cambiando algo que probablemente arregle el error)
- 22/01/24: me esta cagando a palos usar una app con otras referencias, se resetea todo el sistema. Este comportamiento tambien lo vi cuando yo cagaba a palos la app corriendo en caliente, ni tira exception, directamente entra en un loop muerto.
- 25/01/24: EL PROBLEMA ERA QUE TENIA GENERICS. POR DIOS, NO DEJAR NINGUN GENERICS, SE ROMPE TODO!!!
- 01/04/24: Noto una posible correlacion entre la imposibilidad de descubrir un device en el Device Explorer y los pines de SPI para placas sin el CH340

*** Comunicacion Serie ***
- COM1 (RT, TX) no se puede usar para debugear. Tenes que generar tu propia imagen de firmware para deshabilitar el debug por COM1

*** SIM800L ***
- SIM800L anda con 1 capa de 2200nF (2,2uF) sobre el proto pegado al modulo (bien cerca) y con 4V en la fuente de alimentacion de al menos 2A
- Para hablarle en crudo, uso un ESP32 con el pin EN conectado a GND (lo puentea y hablo directo con el conversor serie)
- La papa para TCP esta en esta nota de aplicacion del fabricante: https://www.waveshare.com/w/upload/2/25/SIM800_Series_TCPIP_Application_Note_V1.03.pdf
- 25/03/24: DEPRECADO, muy complicado, lento, y se soluciona mejor con un Dongle 4G que soporta 3G y 2G.


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
