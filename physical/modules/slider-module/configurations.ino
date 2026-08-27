
#include "configurations.h";

int pinStep = 25;
int pinDir = 26;

int pinPot = 27;
int pinActionButton = 4;

// int pinEndSwitch = 15;
// int pinStartSwitch = 16;

int stepsPerCicle = 100;

int TOLERANCE = 0.3;

int potenciometerMinValue = 0;
int potenciometerMaxValue = 4095;
int potentiometerValue = 0;

// return a percentage of potenciometer in current value
int getCurrentPositionValue(){
  int currentValue = analogRead(pinPot);
  return (currentValue * 100) / potenciometerMaxValue; 
}

void updatePotentiometerValue(){
  potentiometerValue = getCurrentPositionValue();
  Serial.println(potentiometerValue );
}


// percent is an percentage of potenciometer
void goToValue(int percent ){
  bool toTop = percent > getCurrentPositionValue();
  float valueMax = percent + TOLERANCE;
  float valueMin = percent - TOLERANCE;
  digitalWrite(pinDir, !toTop);
  while(potentiometerValue <= valueMax && potentiometerValue >= valueMin){
    digitalWrite(pinStep, 1);
    delay(10);
    digitalWrite(pinStep, 0);
    delay(10);
    updatePotentiometerValue();
  }
  return;
}

void goToExtremity(bool toTop = true){ 
  int cursorEndPressed = digitalRead(pinEndSwitch);
  while(cursorEndPressed < 1){
    // runCicle(toTop);
    if(toTop){}
  }
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

void runCicle(bool toTop = true) {
  digitalWrite(pinDir, toTop);
  // validate if the cursor is on the end of potenciometer (endSwitch/StartSwitch is on HIGH)
  for(int step = 0; step < stepsPerCicle; step++){
    digitalWrite(pinStep, 1);        
    delay(10);
    digitalWrite(pinStep, 0);        
    delay(10);
  }
}