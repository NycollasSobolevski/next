extern int pinPot;

extern int pinStep;
extern int pinDir;
extern int pinEndSwitch;
extern int pinStartSwitch;
extern int pinActionButton;

extern int potentiometerValue;

extern int stepsPerCicle;

int getCurrentPositionValue();
void updatePotentiometerValue();
void goToValue(int percent );
void goToExtremity(bool toTop);
void calibratePotenciometer();
void runCicle(bool toTop);
