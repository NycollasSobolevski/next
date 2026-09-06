#include <Wire.h>
int CONNECTION_REQUEST_PORT = 3;

bit connections[16] = []; 

void setup()
{
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(CONNECTION_REQUEST_PORT, INPUT)
  Wire.begin();
  Serial.begin(115200);
}

int getAvailableConnection(){
  for(int i = 1; i < 16; i++){
  	if(connections[i] == 0)
      return i;
  }
  return -1;
}

void addWireConnection(int index){
	connections[index] = 1;
}

void verifyConnectionRequest(){
  if(!digitalRead(CONNECTION_REQUEST_PORT))
  	return;
  
  int newPort = getAvailableConnection();
  Wire.beginTransmission(1);
  Wire.write(newPort);
  Wire.endTransmission();
}

void loop()
{
	verifyConnectionRequest();
  	Serial.println("send handshake");
    for(int i = 1; i < 16; i++ ){
      if(connections[i]){
          Wire.beginTransmission(i);
          Wire.write(1);
          Wire.endTransmission();
      }
    }
}