const int pinStep = 18;
const int pinDir = 19;
const int pinEndSwitch = 15;
const int pinStartSwitch = 16;
const int pinActionButton = 14;
const int pinPot = 17;

const int stepsPerCicle = 100;

int potenciometerMinValue = 0;
int potenciometerMaxValue = 0;

void setup(){
    pinMode(pinStep, OUTPUT);
    pinMode(pinDir, OUTPUT);
    pinMode(pinEndSwitch, INPUT);
    pinMode(pinStartSwitch, INPUT);
    pinMode(pinActionButton, INPUT);
    pinMode(pinPot, INPUT);
    
    calibratePotenciometer()
}

void loop(){
  runCicle(true);
  delay(1500);
  runCicle(false);
}

// return a percentage of potenciometer in current value
int getCurrentPositionValue(){
  int currentValue = digitalRead(pinPot);
  return (currentValue * 100) / potenciometerMaxValue;
}

// percent is an percentage of potenciometer
void goToValue(int percent ){
  bool toTop = percent > getCurrentPosition();

  //! TODO
}

void goToExtremity(bool toTop = true){ 
  int cursorEndPressed = digitalRead(pinEndSwitch);
  while(cursorEndPressed < 1){
    runCicle(toTop);
  }
}

void calibratePotenciometer() {
  int min, max;  
  goToExtremity();
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
    delay(300);
    digitalWrite(pinStep, 0);        
    delay(300);
  }
}