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

