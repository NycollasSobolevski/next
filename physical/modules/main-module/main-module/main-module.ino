
#include <Wire.h>


int CONNECTION_REQUEST_PORT = 3;
int CONNECTION_REQUEST_STATUS_PORT = 4;
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
  if (digitalRead(CONNECTION_REQUEST_PORT) == 0 || !CONNECTION_STATUS)
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

  addWireConnection(newPort);
  String message = String(newPort) + "-" + receivedId;
  Serial.println(newPort);
  Wire.beginTransmission(1);
  Wire.write((uint8_t *)message.c_str(), message.length());
  Wire.endTransmission();

  CONNECTION_STATUS = true;
  digitalWrite(CONNECTION_REQUEST_STATUS_PORT, CONNECTION_STATUS);

  return 1;
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
  // Serial.println("send handshake");
  // for(int i = 1; i < 16; i++ ){
  //   if(connections[i]){
  //      Wire.beginTransmission(i);
  //      Wire.write(1);
  //      Wire.endTransmission();
  //  }
  //}
  delay(500);
}