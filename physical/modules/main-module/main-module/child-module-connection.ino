// #include <Wire.h>

// String SERIAL_ID = "2026001";
// int CONNECTION_REQUEST_PORT = 3;
// int CONNECTION_REQUEST_STATUS_PORT = 4;
// volatile int wirePort = 1;
// volatile bool mudouPorta = false;

// void requestId()
// {
//   Serial.println("Requested Id");
//   Serial.println(wirePort);
//   // Apenas transmite o ID quando solicitado pelo Mestre na porta 1
//   if (wirePort == 1)
//   {
//     Wire.write(SERIAL_ID.c_str());
//   }
// }

// void receiveConnection(int numBytes)
// {
//   String connectionRequestData = "";
//   while (Wire.available())
//   {
//     connectionRequestData += (char)Wire.read();
//   }

//   int separatorIndex = connectionRequestData.indexOf('-');
//   if (separatorIndex != -1)
//   {
//     String receivedId = connectionRequestData.substring(separatorIndex + 1);

//     if (receivedId == SERIAL_ID)
//     {
//       wirePort = connectionRequestData.substring(0, separatorIndex).toInt();
//       mudouPorta = true; // Apenas sinaliza o loop para reconfigurar com segurança
//     }
//   }
// }

// void receiveEvent(int bytes)
// {
//   if (Wire.available())
//   {
//     // Lógica para quando o slider já estiver na sua porta definitiva (> 1)
//     Serial.println(Wire.read());
//   }
// }

// void configureWire()
// {
//   Wire.end();
//   Wire.begin(wirePort);
//   Wire.onReceive(receiveEvent);
// }

// void tryConnect()
// {
//   Wire.end();
//   Wire.begin(1);
//   Wire.onRequest(requestId);
//   Wire.onReceive(receiveConnection);

//   bool CONNECTION_OPENED = digitalRead(CONNECTION_REQUEST_STATUS_PORT);
//   if (!CONNECTION_OPENED)
//   {
//     return;
//   }
//   digitalWrite(CONNECTION_REQUEST_PORT, HIGH);
// }

// void setup()
// {
//   Wire.begin(wirePort);
//   Serial.begin(115200);
//   pinMode(LED_BUILTIN, OUTPUT);
//   pinMode(CONNECTION_REQUEST_STATUS_PORT, INPUT);
//   pinMode(CONNECTION_REQUEST_PORT, OUTPUT);

//   tryConnect();
// }

// void loop()
// {
//   delay(500);

//   if (mudouPorta)
//   {
//     digitalWrite(CONNECTION_REQUEST_PORT, LOW); // Desliga a solicitação
//     configureWire();                            // Reinicia o I2C com a nova porta de forma segura
//     mudouPorta = false;

//     Serial.print("Nova porta I2C configurada: ");
//     Serial.println(wirePort);
//   }
//   else if (wirePort == 1)
//   {
//     // Se ainda não conectou, continua tentando
//     tryConnect();
//   }
// }