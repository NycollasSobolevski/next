extern int pinPot;

extern int pinStep;
extern int pinDir;
extern int pinEndSwitch;
extern int pinStartSwitch;
extern int pinActionButton;
extern int TOLERANCE;
extern bool isConnected;

extern int stepsPerCicle;

int getCurrentPositionValue();
void goToValue(int percent );
void goToExtremity(bool toTop);
void calibratePotenciometer();
void runCicle(bool toTop);
void tryConnect();