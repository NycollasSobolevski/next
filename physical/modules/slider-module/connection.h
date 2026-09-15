extern String SERIAL_ID;

extern int CONNECTION_REQUEST_PORT;
extern int CONNECTION_REQUEST_STATUS_PORT;
extern bool CONNECTION_STATUS;

extern int wirePort;
extern bool portChanged;

void configureGPIOPorts();

void requestId();
void receiveConnection(int bytes);

void receiveEvent(int bytes);
void configureWire();

void tryConnect();
