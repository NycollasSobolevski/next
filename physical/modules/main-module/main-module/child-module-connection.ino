
#include <Wire.h>
String SERIAL_ID = "2026001";
int CONNECTION_REQUEST_PORT = 3;
int CONNECTION_REQUEST_STATUS_PORT = 4;
volatile int wirePort = 1;
volatile bool mudouPorta = false;

void requestId() {
  if(wirePort == 1)
  {	
	Wire.write(SERIAL_ID.c_str());
  }
}
void receiveConnection(int numBytes){
  String connectionRequestData = "";
  while(Wire.available()){
	connectionRequestData += (char)Wire.read();
  } 
  int separatorIndex = connectionRequestData.indexOf('-');
  String receivedId = connectionRequestData.substring(separatorIndex + 1);
  if(receivedId == SERIAL_ID){
   	wirePort = connectionRequestData.substring(0,separatorIndex).toInt();
  }
  mudouPorta = true;
}
void receiveEvent(int bytes){
  if (Wire.available()) {
    //do anything
    Serial.println(Wire.read());
  }
}
void configureWire(){
  Wire.end();
  Wire.begin(wirePort);
  // change according module
  Wire.onReceive(receiveEvent);
}
void tryConnect(){
  Wire.end();
  Wire.begin(1);
  Wire.onRequest(requestId);
  Wire.onReceive(receiveConnection);
  
  bool CONNECTION_OPENED = digitalRead(CONNECTION_REQUEST_STATUS_PORT);
  if(!CONNECTION_OPENED){
	return;
  }
  digitalWrite(CONNECTION_REQUEST_PORT, 1);

}


void setup()
{
  Wire.begin(wirePort);
  Serial.begin(115200);
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(CONNECTION_REQUEST_STATUS_PORT, INPUT);
  
  tryConnect();
  while(wirePort == 1){
  	if (mudouPorta) {
      digitalWrite(CONNECTION_REQUEST_PORT, 0); // Desliga a solicitação
      configureWire(); // Reinicia o I2C com a nova porta
      mudouPorta = false;

      Serial.print("Nova porta I2C configurada: ");
      Serial.println(wirePort);
    }
  	delay(500);
  }
  
}

void loop() {
  
}