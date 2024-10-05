SMARTCOMPOST-NANOFRAMEWORK 

Stack: 

    SDK: .net 8
    IDE: visual studio 2022 17.10.5
    Nanoff: .NET nanoFramework Firmware Flasher v2.5.96+5e1d1e4a6e
    VsExtension: .NET nanoFramwork Extension 2022.3.0.93
    Firmware: https://docs.nanoframework.net/content/reference-targets/esp32.html
        -ESP WROOM: ESP32_REV3 1.12.0.166 (SIN PSRAM, sino es el REV0)
        -ESP CAM:   ESP32_PSRAM_REV0 (tiene memoria expandible)
        -ESP MINI:  XIAO_ESP32C3 1.12.0.32 (es de la familia de ESP32C)

Procedimiento de Deploy:

    Para cada placa tiene que haber un archivo deployXXX.json apuntando a su respectivo infoNodoXXX.json

    Comando de deploy:
        nanoff --update --target ESP32_REV3 --serialport COM3 --masserase --fwversion 1.12.0.166  --filedeployment C:\tmp\deployAP.json --image "C:\src\smartcompost\SmartCompost\NodoAP\bin\Release\NodoAP.bin"

Lista de comandos:

* Deploy basico:

    nanoff --update --target ESP32_REV3 --serialport COM3 --fwversion 1.12.0.166

* Deploy borrando eeprom:

    nanoff --update --target ESP32_REV3 --serialport COM3 --masserase --fwversion 1.12.0.166

* Deploy solo build especifico:

    nanoff --update --target ESP32_REV3 --serialport COM3 --fwversion 1.12.0.166 --image "...\bin\Debug\Blinky.bin"

* Subir solo archivo:

    nanoff --serialport COM3 --filedeployment C:\path\deploy.json

* Limpiar flash entera:  

    python -m esptool --port COM24 erase_flash

