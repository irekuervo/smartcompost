
*** BITACORA ***

- 17/07/24:
    -Despues de dejar el AP intensivo andando muchisimo tiempo, perdi control del equipo totalmente. Tuve que borrar la flash.
    Sospecho que puede ser un problema de memoria a falta de buen manejo de los objetos instanciados.
- 16/07/24:
    -Commit 217160ae87b424fcee333c081b594b6003504400: ANDA TODO!
- 03/07/24:
    -Tenemos un problema de sobrecalentamiento del switching del NODO AP
    -Tenemo un problema de CRC del lora en el NODO AP:
        Teniendo el AP y el Medidor SIN BATERIA, puedo hacer que se hablen de muy cerca, pero me tira error de CRC el AP
- 03/07/24: 
    -Un bajon que te actualicen todo, estoy aprendiendo a configurar el proyecto para uqe funcione clavado en una version. 
    -Aprendi que si todo falla, hay que borrar todo con esptools. 
    -Aprendi que si puedo flashear pero no me reconoce en nanoff, cambiar el puerto usb ayuda
- 01/04/24: 
    -Noto una posible correlacion entre la imposibilidad de descubrir un device en el Device Explorer y los pines de SPI para placas sin el CH340
- 25/01/24: 
    -EL PROBLEMA ERA QUE TENIA GENERICS. POR DIOS, NO DEJAR NINGUN GENERICS, SE ROMPE TODO!!!
- 22/01/24: 
    -me esta cagando a palos usar una app con otras referencias, se resetea todo el sistema. Este comportamiento tambien lo vi cuando yo cagaba a palos la app corriendo en caliente, ni tira exception, directamente entra en un loop muerto.
- PREVIO SIN FECHA
    -Al parecer, en las placas que tienen el CH340 no funca el debugger en VS, pero lo demas si
    -Con el cp210x va como piña todo
    -Aveces deployeo algo que rompe todo y se autoreseta, y dejo de poder deployar. Ahi tengo que volver a instalar el firmware y probar de 0 (o deployar a mano cambiando algo que probablemente arregle el error)

*** WIFI ***
- Cuando no pude conectarme a nada, con error inclusive en el scan, me ayudo configurar el network configuration del board: Options=Auto Connect, Security/Encryption=WPA2

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
