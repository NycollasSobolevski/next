
#include "configurations.h";

int pinStep = 25;
int pinDir = 26;

int pinPot = 27;
int pinActionButton = 4;
int pinEnable = 33;
// int pinEndSwitch = 15;
// int pinStartSwitch = 16;

int stepsPerCicle = 100;

int TOLERANCE = 3;
bool isConnected = false;

int potenciometerMinValue = 0;
int potenciometerMaxValue = 4095;

void tryConnect(){
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
}

// return a percentage of potenciometer in current value
int getCurrentPositionValue(){
  int currentValue = analogRead(pinPot);
  return (currentValue * 100) / potenciometerMaxValue; 
}


// percent is an percentage of potenciometer
void goToValue(int percent ){
  Serial.println("going to posityion");
  bool toTop = percent > getCurrentPositionValue();
  float valueMax = percent + TOLERANCE;
  float valueMin = percent - TOLERANCE;
  digitalWrite(pinDir, toTop);
  digitalWrite(pinEnable, 0);
  while(!(getCurrentPositionValue() <= valueMax && getCurrentPositionValue() >= valueMin)){
    digitalWrite(pinStep, 1);
    delay(2);
    digitalWrite(pinStep, 0);
    delay(2);
  }
  digitalWrite(pinEnable, 1);
  return;
}

void calibratePotenciometer() {
  int min, max;  
  goToExtremity(true);
  max = digitalRead(pinPot);
  goToExtremity(false);
  min = digitalRead(pinPot);
  potenciometerMinValue = min;
  potenciometerMaxValue = max;
}

void SendMessage(char[] action, char[] value)
{

}




bool checkPotentiometer()
{
  if(!digitalRead(pinActionButton))
    
}
bool checkActionButton()
{

}
bool checkChanges()
{

}
