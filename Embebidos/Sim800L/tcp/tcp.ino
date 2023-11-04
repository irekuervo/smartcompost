#include <SoftwareSerial.h>

// Configura el puerto serie SoftwareSerial
SoftwareSerial sim800l(12, 13);  // RX, TX

void setup() {
   // Inicializa la comunicación con el módulo SIM800L
  
  Serial.begin(9600);

  sim800l.begin(115200);
  

  Serial.println("Initializing...");
  delay(1000);

  sim800l.println("AT");  //Once the handshake test is successful, it will back to OK
  updateSerial();

  Serial.println("Configurando APN...");

  // Configura el APN según tu operador
  sim800l.println("AT+SAPBR=3,1,\"CONTYPE\",\"GPRS\"");
  updateSerial();
  sim800l.println("AT+SAPBR=3,1,\"APN\",\"internet.movil\"");
  updateSerial();
  sim800l.println("AT+SAPBR=3,1,\"USER\",\"internet\"");
  updateSerial();
  sim800l.println("AT+SAPBR=3,1,\"PWD\",\"internet\"");
  updateSerial();

  Serial.println("Conexion GPRS...");

  // Abre una conexión GPRS
  sim800l.println("AT+SAPBR=1,1");
  updateSerial();

  // Realiza la solicitud HTTP GET
  sim800l.println("AT+HTTPINIT");
  updateSerial();
  sim800l.println("AT+HTTPSSL=1");  // Habilita SSL (para HTTPS)
  updateSerial();
  sim800l.println("AT+HTTPPARA=\"CID\",1");
  updateSerial();
  sim800l.println("AT+HTTPPARA=\"URL\",\"https://jsonplaceholder.typicode.com/todos/1\"");
  updateSerial();
  sim800l.println("AT+HTTPACTION=0");
  delay(5000);

  // Lee la respuesta HTTP
  sim800l.println("AT+HTTPREAD");
  delay(1000);
  while (sim800l.available()) {
    Serial.write(sim800l.read());
  }

  // Cierra la conexión GPRS
  sim800l.println("AT+HTTPTERM");
  
  updateSerial();
}

void loop() {
  updateSerial();
}

void updateSerial() {
  delay(2000);
  while (Serial.available()) {
    sim800l.write(Serial.read());  //Forward what Serial received to Software Serial Port
  }
  delay(2000);
  while (sim800l.available()) {
    Serial.write(sim800l.read());  //Forward what Software Serial received to Serial Port
  }
}