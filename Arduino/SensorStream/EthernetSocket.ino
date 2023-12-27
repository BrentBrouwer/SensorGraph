// Check if the Required Hardware is Present
bool CheckHardWare() 
{
  // Debug
  // Serial.println("Hardware Check");

  // No Hardware Connected, stay in the Loop
  bool InitShield = true;
  while (Ethernet.hardwareStatus() == EthernetNoHardware) 
  {
    if (InitShield) 
    {
      Serial.println("No EthernetShield Found");
      InitShield = false;
    }

    // Prevent High CPU Usage
    delay(100);
  }

  if (!InitShield) 
  {
    Serial.println("EthernetShield Found");
  }

  // Check if the Ethernet Cable is Connected
  bool InitCable = true;
  while (Ethernet.linkStatus() == LinkOFF) 
  {
    if (InitCable) 
    {
      Serial.println("Ethernet Cable not connected.");
      InitCable = false;
    }

    // Prevent High CPU Usage
    delay(100);
  }

  if (!InitCable) 
  {
    Serial.println("Ethernet Cable Connected");
  }

  // Return Value
  if (InitShield && InitCable) 
  {
    return true;
  } 
  else 
  {
    return false;
  }
}

// Initialize the Socket Client
void InitSocket()
{
  // Arduino Client Properties
  byte ArduinoMAC[] = { 0xA8, 0x61, 0x0A, 0xAE, 0xDF, 0x26 };
  IPAddress ArduinoIP(192, 168, 53, 101);
  IPAddress ArduinoDns(192, 168, 1, 1);
  IPAddress ArduinoGateway(192, 168, 1, 254);
  IPAddress ArduinoSubnet(255, 255, 192, 0);

  // Configure the CS Pin
  //Ethernet.init(10);

  // Set the Properties
  Ethernet.begin(ArduinoMAC, ArduinoIP, ArduinoDns, ArduinoGateway, ArduinoSubnet);

  if (CheckHardWare()) 
  {
    // Start Listening for Clients
    Serial.print("IP-Address Arduino: ");
    Serial.println(Ethernet.localIP());
  }
}

// Connect to the Socket Server
bool ConnectToServer()
{
  

  // Check if the Client is already Connected
  if (client.connected())
  {
    // Serial.println("Already Connected");
    return true;
  }
  else if (client.connect(ServerIP, ServerPort))
  {
    Serial.println("Connected to Server");
    return true;
  }
  else 
  {
    // Connection Failed
    Serial.println("Connection Failed");
    return false;
  }
}