#include "configurations.h";
#include "Wire.h";

int pinEnable = 33;
int pinStep = 25;
int pinDir = 26;

int pinPot = 27;
int pinActionButton = 4;
// int pinEndSwitch = 15;
// int pinStartSwitch = 16;

int stepsPerCicle = 100;

int TOLERANCE = 2;

int potenciometerValue = 0;
int potenciometerMinValue = 0;
int potenciometerMaxValue = 4095;

volatile byte changes = 0;
volatile bool changesHasSended = false;

// return a percentage of potenciometer in current value
int getCurrentPositionValue(){
  int currentValue = analogRead(pinPot);
  int value = (currentValue * 100) / potenciometerMaxValue;
  potenciometerValue = value;
  return value; 
}


// percent is an percentage of potenciometer
void goToValue(int percent ){
  Serial.println("going to position");
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

void SendMessage()
{
  if(changesHasSended){
    Wire.write(-1);
    return;
  }
  // notes: Serial.write dont convert the value to string, the method send the pure byte value and Serial.print convert the value into string to view data (100 -> '1', '0', '0')
  Wire.write(changes);
  changesHasSended = true;
}

void onChange(byte action, byte value)
{
  Serial.write(1); // OUT
  Serial.write(action); // Action
  Serial.write(value); // Value of action
  changes = action + value;
  changesHasSended = false;
}

int checkPotentiometer()
{
  int oldValue = potenciometerValue;
  int newValue = getCurrentPositionValue();
  if(newValue < (oldValue - TOLERANCE) 
      || newValue > (oldValue + TOLERANCE) ){
    return potenciometerValue;
  }
  return -1;
}
bool checkActionButton()
{
  if(!digitalRead(pinActionButton))
    return true;
  return false;
}
bool checkChanges()
{
  if(int value = checkPotentiometer(); value > 0 ){
    onChange(2, value);
  }
  if(checkActionButton()){
    onChange(1, 0);
  }
  
  return false; // Retorno padrão caso nenhuma alteração aconteça
}
