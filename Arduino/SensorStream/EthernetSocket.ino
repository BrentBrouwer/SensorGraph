// Check if the Required Hardware is Present
bool CheckHardWare() 
{
  // Debug
  // Serial.println("Hardware Check");

  // No Hardware Connected, stay in the Loop
  bool InitShield = true;
  while (Ethernet.hardwareStatus() == EthernetNoHardware) 
  {
    // Prevent High CPU Usage
    BlinkLED(false);

    if (InitShield) 
    {
      Serial.println("No EthernetShield Found");
      InitShield = false;
    }
  }

  if (!InitShield) 
  {
    Serial.println("EthernetShield Found");
  }

  // Check if the Ethernet Cable is Connected
  bool InitCable = true;
  while (Ethernet.linkStatus() == LinkOFF) 
  {
    // Prevent High CPU Usage
    BlinkLED(false);

    if (InitCable) 
    {
      Serial.println("Ethernet Cable not connected.");
      InitCable = false;
    }
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

// Initialize the Socket Server
void InitSocketServer()
{
  // Arduino Client Properties
  byte ArduinoMAC[] = { 0xA8, 0x61, 0x0A, 0xAE, 0xDF, 0x26 };
  IPAddress ArduinoIP(192, 168, 53, 101);
  IPAddress ArduinoDns(192, 168, 53, 1);
  IPAddress ArduinoGateway(192, 168, 53, 1);
  IPAddress ArduinoSubnet(255, 255, 192, 0);

  // Configure the CS Pin
  //Ethernet.init(10);

  // Set the Properties
  Ethernet.begin(ArduinoMAC, ArduinoIP, ArduinoDns, ArduinoGateway, ArduinoSubnet);
  server.begin();

  if (CheckHardWare()) 
  {
    // Start Listening for Clients
    Serial.print("TCP Server Arduino: ");
    Serial.print(Ethernet.localIP());
    Serial.print(":");
    Serial.println(PortNr);
  }
}

// Wait for Clients to Connect
bool WaitForClients()
{
  bool InitConnect = false;

  // Wait for a new Client
  if (!ConnectedClient)
  {
    Serial.println("Start Checking Clients");

    EthernetClient IncomingClient = server.accept();

    // Assign the New Client to the Global Client
    if (IncomingClient.connected())
    {
      ComClient = IncomingClient;
      ConnectedClient = true;
      InitConnect = true;
    }
  }

  // Check if the Client is already Connected
  if (ConnectedClient && ComClient.connected())
  {
    ComClient.flush();
    
    if (InitConnect)
    {
      InitConnect = false;
      ComClient.println("Connected to the Arduino Socket Server");
    }
    Serial.println("Connected");
    return true;
  }
  else if (LogFailedConnection)
  {
    // Connection Failed
    LogFailedConnection = false;
    Serial.println("Connection Failed");
    ConnectedClient = false;
    return false;
  }
  else 
  {
    ConnectedClient = false;
    return false;
  }
}

void SendDataToClient(int A0Value, int A1Value)
{
  char msg[20];
  sprintf(msg, "%c%i|%i%c", (char)2, A0Value, A1Value, (char)3);
  Serial.println(msg);
  ComClient.println(msg);

  // Debug
  delay(1000);
}