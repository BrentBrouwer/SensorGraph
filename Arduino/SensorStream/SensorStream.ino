// Libraries
#include <Ethernet.h>

// Socket Server Properties
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
  // First check the Hardware, then try to connect
  if (CheckHardWare() && ConnectToServer())
  {
    // Read the Sensor Input
    int SensorValue = analogRead(A0);
    Serial.print("Sensor Value: ");
    Serial.println(SensorValue);

    client.write(SensorValue);
  }

  // Debug, Always send the sensor value
  // Read the Sensor Input
  // int SensorValue = analogRead(A0);
  // Serial.print("Sensor Value: ");
  // Serial.println(SensorValue);
}
