extern int pinPot;

extern int pinStep;
extern int pinDir;
extern int pinEndSwitch;
extern int pinStartSwitch;
extern int pinActionButton;
extern int TOLERANCE;
extern int pinEnable;
extern int stepsPerCicle;
extern bool isConnected;

int getCurrentPositionValue();
void goToValue(int percent );
void goToExtremity(bool toTop);
void calibratePotenciometer();
void runCicle(bool toTop);
void tryConnect();

int checkPotentiometer();
bool checkActionButton();

bool checkChanges();

