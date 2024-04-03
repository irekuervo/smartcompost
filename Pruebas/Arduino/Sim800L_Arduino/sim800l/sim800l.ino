#include <SoftwareSerial.h>

#define txPin 12
#define rxPin 13

SoftwareSerial mySerial(rxPin, txPin);

void setup() {
  //Begin serial communication with Arduino and Arduino IDE (Serial Monitor)
  Serial.begin(9600);

  //Begin serial communication with Arduino and SIM800L
  mySerial.begin(115200);

  Serial.println("Initializing...");
  delay(1000);

  mySerial.println("AT");  //Once the handshake test is successful, it will back to OK
  updateSerial();

  mySerial.println("AT+CMGF=1");  // Configuring TEXT mode
  updateSerial();

  mySerial.println("AT+CNMI=1,2,0,0,0");  // Configuring TEXT mode
  updateSerial();

  // mySerial.println("AT+CMGS=\"+541138776880\"");//change ZZ with country code and xxxxxxxxxxx with phone number to sms
  // updateSerial();
  // mySerial.print("AGUANTE EL PAPO VIEJA"); //text content
  // updateSerial();
  // mySerial.write(26);
}

void loop() {
  updateSerial();
}

void updateSerial() {
  delay(500);
  while (Serial.available()) {
    mySerial.write(Serial.read());  //Forward what Serial received to Software Serial Port
  }
  delay(500);
  while (mySerial.available()) {
    Serial.write(mySerial.read());  //Forward what Software Serial received to Serial Port
  }
}