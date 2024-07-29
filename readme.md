SMARTCOMPOST-NANOFRAMEWORK 

Version 1.0.0 - 02/07/2024

Stack: 

    SDK: .net 8
    IDE: visual studio 2022 17.10.5
    Nanoff: .NET nanoFramework Firmware Flasher v2.5.90+c66e018c48
    VsExtension: .NET nanoFramwork Extension 2022.3.0.93
    Firmware: ESP32_PSRAM_REV0 1.10.0.49

Procedimiento de Deploy:

    Para cada placa tiene que haber un archivo deployXXX.json apuntando a su respectivo infoNodoXXX.json

    Comando de deploy:
        nanoff --update --target ESP32_PSRAM_REV0 --serialport COM3 --masserase --fwversion 1.10.0.49  --filedeployment C:\tmp\deployAP.json --image "C:\src\smartcompost\SmartCompost\NodoAP\bin\Release\NodoAP.bin"

Lista de comandos:

* Deploy basico:

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM3 --fwversion 1.10.0.49

* Deploy borrando eeprom:

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM3 --masserase --fwversion 1.10.0.49

* Deploy solo build especifico:

    nanoff --update --target ESP32_PSRAM_REV0 --serialport COM3 --fwversion 1.10.0.49 --image "...\bin\Debug\Blinky.bin"

* Subir solo archivo:

    nanoff --serialport COM3 --filedeployment C:\path\deploy.json

* Limpiar flash entera:  

    python -m esptool --port COM24 erase_flash

