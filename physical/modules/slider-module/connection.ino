#include "Wire.h";
#include "connection.h";
#include "configurations.h";

String SERIAL_ID = "ABC";

int CONNECTION_REQUEST_PORT = 12;
int CONNECTION_REQUEST_STATUS_PORT = 14;
bool CONNECTION_STATUS = true;

int wirePort = 1;
bool mudouPorta = false;

void configureGPIOPorts(){
    pinMode(CONNECTION_REQUEST_PORT, OUTPUT);
    pinMode(CONNECTION_REQUEST_STATUS_PORT, INPUT);
}

/** @brief send SERIAL_ID when is on connecting mode*/
void requestId()
{
    if (wirePort == 1)
    {
        Wire.write((uint8_t*)SERIAL_ID.c_str(), SERIAL_ID.length());
    }
}

/** @brief receive method when is on connecting mode */
void receiveConnection(int numBytes)
{
    String connectionRequestData = "";
    while (Wire.available())
    {
        connectionRequestData += (char)Wire.read();
    }

    int separatorIndex = connectionRequestData.indexOf('-');
    if (separatorIndex != -1)
    {
        String receivedId = connectionRequestData.substring(separatorIndex + 1);

        if (receivedId == SERIAL_ID)
        {
            wirePort = connectionRequestData.substring(0, separatorIndex).toInt();
            mudouPorta = true; // Apenas sinaliza o loop para reconfigurar com segurança
        }
    }
}

/** @brief execute when receive information from MASTER (I2C) */
void receiveEvent(int numBytes)
{
    while (Wire.available())
    {
        Serial.println(Wire.read());
    }
}

/** @brief disconnect and reconnect the Wire connection */
void configureWire()
{
    Wire.end();
    Wire.begin(wirePort);
    Wire.onReceive(receiveEvent);
    Wire.onRequest(SendMessage);
}

/** Try connect with MAIN MODULE via I2C */
void tryConnect()
{
    configureGPIOPorts();
    Wire.end();
    Wire.begin(1);
    Wire.onRequest(requestId);
    Wire.onReceive(receiveConnection);
    digitalWrite(CONNECTION_REQUEST_PORT, HIGH);

    Serial.print("Trying connect");
    while (wirePort == 1 && !mudouPorta)
    {
        delay(250);
        digitalWrite(LED_BUILTIN, HIGH);
        Serial.print(wirePort);

        delay(250);
        digitalWrite(LED_BUILTIN, LOW);
    }
    if (mudouPorta)
    {
        digitalWrite(CONNECTION_REQUEST_PORT, LOW);
        configureWire();
        mudouPorta = false;

        Serial.print("Nova porta I2C configurada: ");
        Serial.println(wirePort);
    }
}