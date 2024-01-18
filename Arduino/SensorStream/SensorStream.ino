// Libraries
#include <Ethernet.h>

// Loop Enable
bool LoopDisableInit = true;

// The Arduino Socket Server
const int PortNr = 8267;
EthernetServer server(PortNr);

// The Connected Client
EthernetClient ComClient;
bool ConnectedClient = false;

// Init Log Flag
bool LogFailedConnection = true;

void setup() 
{
  // Initialize Serial Communication
  Serial.begin(9600);

  // Assign Pin 13 as Output
  // Disable when Ethernet Shield is Placed
  pinMode(LED_BUILTIN, OUTPUT);
  
  // Wait for the Serial Port to Open
  while (!Serial)
  {
    // Prevent High CPU Usage
    delay(1);
  }

  Serial.println("\nSerial Port Opened");

  // Initialize the Ethernet Device, if the Required Hardware is present
  InitSocketServer();
}

void loop() 
{
  // First check the Hardware, then try to connect
  if (CheckHardWare() && WaitForClients())
  {
    // Read the Sensor Input and Send it to the Server
    SendDataToClient(analogRead(A0), analogRead(A1));

    BlinkLED(true);
  }
  else 
  {
    BlinkLED(false);
  }
}


void BlinkLED(bool ClientConnected)
{
  int LEDInterval = 10;
  if (ClientConnected)
  {
    LEDInterval = 10;
  }

  // Blink the LED
  digitalWrite(LED_BUILTIN, HIGH);
  // Serial.println("LED ON");
  delay(LEDInterval);
  digitalWrite(LED_BUILTIN, LOW);
  // Serial.println("LED OFF");
  delay(LEDInterval);
}