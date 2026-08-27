#include "configurations.h";

bool isConnected = false;

void setup(){
  pinMode(pinActionButton, INPUT_PULLUP);
  pinMode(pinPot, INPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  pinMode(pinStep, OUTPUT);
  pinMode(pinDir, OUTPUT);

  Serial.begin(115200);

  while(!isConnected) {
    if(Serial.available() > 0){
      String command = Serial.readStringUntil('\n');
      command.trim();

      if(command == "next_module") {
        Serial.println("next_module");
        isConnected = true;
      }
    }
    digitalWrite(LED_BUILTIN, 1);
    delay(1000);
    digitalWrite(LED_BUILTIN, 0);
    delay(1000);
  }

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
  // definindo posicao para baixo ao infinito;
  digitalWrite(pinDir, 0);

  if(!digitalRead(pinActionButton)){
    Serial.println("ACTION BUTTON PRESSED");
    digitalWrite(LED_BUILTIN, 1);
    
    // definindo rotacao para o sentido contrario
    digitalWrite(pinDir, 1);

    for(int i =0; i < 1000; i++){
      digitalWrite(pinStep, 1);
      delay(2);
      digitalWrite(pinStep, 0);
      delay(2);
    }

    digitalWrite(LED_BUILTIN, 0);
  }
  // Serial.print(digitalRead(pinActionButton));
  // Serial.print(" - ");
  // Serial.println(analogRead(pinPot));

  
  // digitalWrite(pinStep, 1);
  // delay(100);
  // digitalWrite(pinStep, 0);
  // delay(100);
}

