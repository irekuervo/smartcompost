NANOFRAMEWORK (C# framework): https://www.nanoframework.net/ , https://github.com/nanoframework
NANOFF (Firmware flasher): https://github.com/nanoframework/nanoFirmwareFlasher
SX127X (LoRa): https://github.com/KiwiBryn/SX127X-NetNF

*** Paso 0: instalar vs2022 o dotnet 6***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 1: instalar en visual studio la extension: .Net Nanoframework Extension***
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

*** Paso 2: instalar tools ***

    dotnet tool install -g nanoff

    dotnet tool update -g nanoff

*** Paso 3: Para actualizar el firmaware ***

Importante: si la placa tiene el CH340, obviar el punto 2!!!. Asegurarse de que ningun proceso del OS este usando el puerto serie que vas a usar (Visual Studio por ejemplo con el .Net Nanoframework Extension)

* 1) Elegir target y puerto COM. "ESP32_PSRAM_REV0" es el mas generico, por ahora anda

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM31

* 2) MANTENER APRETADO EL BOTON DE PROGRAMACION DEL ESP32 DURANTE TODO EL COMANDO (EL QUE DICE BOOT) !! Paciencia, pueden ser como 60 segs *
* 3) NO CONECTAR NADA EN LOS PINES TX/RX, ya que el debugger los usa *
* 4) Conectar y desconectar el usb del esp32 para bootearlo bien

*** Paso 3 bis (opcional): Depploy manual de .bin *** 
nanoff --target ESP32_PSRAM_REV0 --serialport COM12 --deploy --image "E:\GitHub\nf-Samples\samples\Blinky\Blinky\bin\Debug\Blinky.bin"

*** Paso 4: Manejo proyecto .Net *** 
Abrir un proyecto Blank Application (nanoFramework), y actualizar todos los paquetes nuget que hagan falta (con el preview checkbox ticked)

*** Paso 5: Deploy proyecto .Net *** 
Comprobar que en el explorador aparezca el device, y que se le puedan pedir sus "Device Capabilities"
Darle run si quiero debuggear, o deploy si solo quiero deployar

*** WIFI ***
- Configurar el network configuration del board: Options=Auto Connect, Security/Encryption=WPA2

*** ERRORES COMUNES ***

- Tengo los paquetes de Nuget raros -> Visual Studio -> Tools -> Nuget Package Manager -> Package Manager Console 

    "Update-Package -reinstall" para toda la solucion
    "Update-Package -reinstall -Project YourProjectName" para un solo proyecto

- Version mismatch for mscorlib. Need v1.14.3.0 -> Updatear los paquetes de nuget!!
- The connected target does not have support for mscorlib. -> Updetear el firmware!!
- ERROR: failing to connect to device: cuando todo falla y todo esta perdido, un mass erase del eeprom puede ayudar:

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM10 --masserase

O si ves que nunca te responde el device, puede ser que se updeteo el firmware, y el cache esta choto:

    nanoff --clearcache
