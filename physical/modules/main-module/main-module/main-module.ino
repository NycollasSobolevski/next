#include <Wire.h>


int CONNECTION_REQUEST_PORT = 12;
int CONNECTION_REQUEST_STATUS_PORT = 14;
bool CONNECTION_STATUS = true;

String connections[16] = {};

int getConnectionIndex(String id)
{
  for (int i = 0; i < 16; i++)
  {
    if (connections[i] == id)
    {
      return i;
    }
  }
  return -1;
}
int getAvailableConnection()
{
  for (int i = 2; i < 16; i++)
  {
    if (connections[i].length() == 0)
      return i;
  }
  return -1;
}

void addWireConnection(int index, String value)
{
  connections[index] = value;
}

int verifyConnectionRequest()
{
  bool isRequested = digitalRead(CONNECTION_REQUEST_PORT);
  if (isRequested == 0 || !CONNECTION_STATUS)
  {
    return -1;
  }

  Serial.print(digitalRead(CONNECTION_REQUEST_PORT));
  Serial.print(" - ");

  CONNECTION_STATUS = false;
  digitalWrite(CONNECTION_REQUEST_STATUS_PORT, CONNECTION_STATUS);

  Wire.requestFrom(1, 32);
  String receivedId = "";
  while (Wire.available())
  {
    char c = Wire.read();
    if (c != 255)
    {
      receivedId += c;
    }
  }
  if (receivedId.length() <= 0)
  {
    CONNECTION_STATUS = true;
    digitalWrite(CONNECTION_REQUEST_STATUS_PORT, CONNECTION_STATUS);
    return -1;
  }

  Serial.println("Salvando o ID: " + receivedId);
  int newPort = getConnectionIndex(receivedId);
  if (newPort <= 0)
  {
    newPort = getAvailableConnection();
  }

  addWireConnection(newPort, receivedId);
  String message = String(newPort) + "-" + receivedId;
  Serial.println(newPort);
  Wire.beginTransmission(1);
  Wire.write((uint8_t *)message.c_str(), message.length());
  Wire.endTransmission();

  for(int i =0; i< 5; i++)
  {
    digitalWrite(LED_BUILTIN, 1);
    delay(200);
    digitalWrite(LED_BUILTIN, 0);
    delay(200);
  }

  CONNECTION_STATUS = true;
  digitalWrite(CONNECTION_REQUEST_STATUS_PORT, CONNECTION_STATUS);

  return 1;
}

void onReceiveMessage(int numBytes)
{
  String value = "";
  while(Wire.available())
  {
    value += Wire.read();
  }
  //Serial.write(value);
  Serial.println(value);
}

void setup()
{
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(CONNECTION_REQUEST_PORT, INPUT);
  pinMode(CONNECTION_REQUEST_STATUS_PORT, OUTPUT);

  Wire.begin();
  Serial.begin(115200);

  if (CONNECTION_STATUS)
  {
    digitalWrite(CONNECTION_REQUEST_STATUS_PORT, 1);
  }
}

void loop()
{
  verifyConnectionRequest();
  Serial.println("send handshake");
  for(int i = 1; i < 16; i++ ){
    if(connections[i]){
      Wire.requestFrom(i, 16);
      String value ="";
      while(Wire.available())
      {
        char c = Wire.read();
        Serial.println(c);
        value += c;
      }
      Serial.print("Message received:");
      Serial.println(value);
   }
  }
  delay(500);
}