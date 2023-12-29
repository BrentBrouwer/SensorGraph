// Libraries
#include <Ethernet.h>

// Loop Enable
bool LoopDisableInit = true;

// Sample Interval

// Socket Server Properties (PC)
IPAddress ServerIP(192, 168, 53, 19);
// IPAddress ServerIP(127, 0, 0, 1);
EthernetClient client;
const int ServerPort = 8267;

void setup() 
{
  // Initialize Serial Communication
  Serial.begin(9600);
  
  // Wait for the Serial Port to Open
  while (!Serial)
  {
    // Prevent High CPU Usage
    delay(1);
  }

  Serial.println("\nSerial Port Opened");

  // Initialize the Ethernet Device, if the Required Hardware is present
  InitSocket();
}

void loop() 
{
  // if (digitalRead(8))
  if (true)
  {
    LoopDisableInit = true;

    // First check the Hardware, then try to connect
    if (CheckHardWare() && ConnectToServer())
    {
      // Read the Sensor Input and Send it to the Server
      SendDataToServer(analogRead(A0), analogRead(A1));

      // Write the Data to the Server
      // client.write(SensorValue);
    }
  }
  else 
  {
    if (LoopDisableInit)
    {
      // client.stop();
      Serial.println("Loop Disabled");
      LoopDisableInit = false;
    }
    
    // Prevent High CPU Usage
    delay(10);
  }

  // Debug, Always send the sensor value
  // SendDataToServer(analogRead(A0), analogRead(A1));
}
