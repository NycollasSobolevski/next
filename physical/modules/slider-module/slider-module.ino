#include "configurations.h";

void setup(){
  pinMode(pinActionButton, INPUT_PULLUP);
  pinMode(pinPot, INPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  pinMode(pinStep, OUTPUT);
  pinMode(pinDir, OUTPUT);

  Serial.begin(115200);

  tryConnect();

  Serial.println("\n -------- Pin Settings -------- \n");

  Serial.print("pinActionButton: ");
  Serial.println(pinActionButton);
  Serial.print("pinPot: ");
  Serial.println(pinPot);
  Serial.print("LED_BUILTIN: ");
  Serial.println(LED_BUILTIN);
  Serial.print("pinStep: ");
  Serial.println(pinStep);
  Serial.print("pinDir: ");
  Serial.println(pinDir);

  delay(5000);
}

void loop(){
  // if(!isConnected){tryConnect();  }
  // if(!digitalRead(pinActionButton)){isConnected = false;}
  if(!digitalRead(pinActionButton)){goToValue(100);}

  int value = Serial.parseInt();
  if(value > 0){
    goToValue(value);
  }

  delay(250);
}

