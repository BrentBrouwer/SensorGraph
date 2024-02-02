using SensorGraph.Arduino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SensorGraph.Communication
{
    public class SocketCommunication
    {
        #region Properties
        // Own Reference
        SocketCommunication thisClassRef = null;
        string ClassName = "SocketCommunication";

        // Reference to the ClassManager
        ClassManager classManager = null;

        // Task Definitation
        Task<bool> TaskCheckClients = null;
        bool EnableCheckClients = false;

        // Server Properties (this program)
        TcpListener tcpServer = null;
        IPAddress ServerIPAddress = IPAddress.Parse("192.168.53.19");
        const int ServerPort = 8267;
        public bool ClientConnected = false;

        // Client Properties (Devices that connect to this Server)
        TcpClient tcpClient = null;
        NetworkStream networkStream = null;
        const int ReadTimeOut = 100;              // Client Read TimeOut in MilliSeconds
        const int WriteTimeOut = 100;             // Client Write TimeOut in MilliSeconds
        public string IncomingData = string.Empty;

        // Buffer Properties
        byte[] DataBuffer = null;
        const int MaxRecBufferSize = 11;

        // IncomingData
        public int SensorA0Value = 0;
        public int SensorA1Value = 0;

        // Logger Instance
        //Logger loggerSocketCom = null;
        #endregion

        #region Constructor
        public SocketCommunication(ClassManager classManager)
        {
            // Set the References
            thisClassRef = this;
            this.classManager = classManager;
            //loggerSocketCom = LoggingManager.Instance.GetLogger("SocketHandler");
        }
        #endregion

        #region InitExit
        public void Init()
        {
            string MethodName = "Init()";

            try
            {
                //loggerSocketCom.Info("SCM000 Initialize socket communication");

                if (CreateInstances())
                {
                    //loggerSocketCom.Info("SCM001 Instances created, start the TCP server");

                    // Start the Server
                    tcpServer.Start();

                    //loggerSocketCom.Info("SCM002 TCP server started");

                    // Start Checking for Incoming Clients
                    StartCheckingClients();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        public void Exit()
        {
            string MethodName = "Exit()";

            try
            {
                //loggerSocketCom.Info("SCM003 Exit socket communication");
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Events

        #endregion

        #region Methods
        bool CreateInstances()
        {
            string MethodName = "CreateInstances()";
            bool RetValue = false;

            try
            {
                //loggerSocketCom.Info("SCM004 Create instances");

                // Create the Buffer that stores Client Data
                DataBuffer = new byte[MaxRecBufferSize];

                // Create the Server Object, that listens to all network interfaces
                tcpServer = new TcpListener(IPAddress.Any, ServerPort);

                RetValue = true;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }

        #region Connection
        async void StartCheckingClients()
        {
            string MethodName = "StartCheckingClients()";

            try
            {
                if (TaskCheckClients == null || TaskCheckClients.Status != TaskStatus.Running)
                {
                    //loggerSocketCom.Info("SCM005 Start checking clients");

                    EnableCheckClients = true;
                    ClientConnected = false;
                    SensorA0Value = 0;
                    SensorA1Value = 0;
                    TaskCheckClients = new Task<bool>(() => AsyncReadClient());
                    TaskCheckClients.Start();
                }

                // Await the Result
                bool TaskResult = await TaskCheckClients;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        void StopCheckingClients()
        {
            string MethodName = "StopCheckingClients()";

            try
            {
                //loggerSocketCom.Info("SCM005 Stop checking clients");

                // Stop the Loop
                EnableCheckClients = false;

                // Cleanup the Task
                TaskCheckClients = null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        bool AsyncReadClient()
        {
            string MethodName = "AsyncReadClient()";
            bool RetValue = false;

            try
            {
                //loggerSocketCom.Info("SCM007 Initialize socket communication");

                while (EnableCheckClients)
                {
                    //loggerSocketCom.Info("SCM008 Search Clients");

                    // Check for Connected Clients
                    tcpClient = tcpServer.AcceptTcpClient();

                    // Set the TCP Client Properties
                    tcpClient.ReceiveBufferSize = MaxRecBufferSize;
                    tcpClient.SendBufferSize = MaxRecBufferSize;
                    //tcpClient.ReceiveTimeout = ReadTimeOut;
                    //tcpClient.SendTimeout = WriteTimeOut;

                    // Get the NetworkStream to Send/Receive Messages
                    networkStream = tcpClient.GetStream();

                    // Check if the Client is Connected
                    if (tcpClient.Connected)
                    {
                        IPEndPoint ipEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                        //loggerSocketCom.Info(string.Format("SCM009 Client with IP-Address {0} connected", ipEndPoint.Address));

                        ClientConnected = true;

                        // Stop the Loop
                        EnableCheckClients = false;

                        RetValue = true;

                        // Read the Client
                        ReadClient();
                    }
                    else
                    {
                        ClientConnected = false;
                        //loggerSocketCom.Info(string.Format("SCM010 New client not connected"));
                    }
                }
            }
            catch (Exception Ex)
            {
                EnableCheckClients = false;
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }
        #endregion

        #region Reading Data
        private void ReadClient()
        {
            string MethodName = "ReadClient()";

            try
            {
                // Check for an Active Client and NetworkStream
                if (tcpClient != null && networkStream != null)
                {
                    // Check for a Valid Connection
                    if (tcpClient.Connected)
                    {
                        //loggerSocketCom.Info(string.Format("SCM011 Async read client"));

                        // Clear the Buffer
                        DataBuffer = new byte[MaxRecBufferSize];

                        // Read the Clients Buffer
                        IAsyncResult ar = networkStream.BeginRead(DataBuffer, 0, MaxRecBufferSize, new AsyncCallback(ReadClientCallBack), tcpClient);
                    }
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);

                // Try to Reconnect
                StartCheckingClients();
            }
        }

        private void ReadClientCallBack(IAsyncResult ar)
        {
            string MethodName = "ReadClientCallBack()";

            try
            {
                // Read the Data
                int BytesRead = networkStream.EndRead(ar);

                // Retrieve the TCPClient Object
                TcpClient tcpClientCB = (TcpClient)ar.AsyncState;

                if (BytesRead > 0)
                {
                    // Read the Buffer
                    string ReceivedMsg = Encoding.UTF8.GetString(DataBuffer);

                    //loggerSocketCom.Info(string.Format("SCM012 Read from client: {0}", ReceivedMsg));

                    // Parse the Message
                    if (ReceivedMsg.Length > 0)
                    {
                        IncomingData = ReceivedMsg;
                        ParseMessage(ReceivedMsg);
                    }

                    // Start Reading the Client Again
                    if (tcpClientCB.Connected)
                    {
                        // Read the Clients Buffer
                        tcpClientCB.GetStream().BeginRead(DataBuffer, 0, MaxRecBufferSize, new AsyncCallback(ReadClientCallBack), tcpClient);
                    }
                    else
                    {
                        // Try to Reconnect
                        StartCheckingClients();
                    }
                }                
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);

                // Try to Reconnect
                StartCheckingClients();
            }
        }

        private void ParseMessage(string ReceivedMsg)
        {
            string MethodName = "ParseMessage()";

            // Message Properties
            char STX = (char)2;
            char ETX = (char)3;
            char Seperator = (char)124;
            string IngoreMsg = "\0\0";

            try
            {
                // Check for STX and ETX
                if (ReceivedMsg.Contains(STX) && ReceivedMsg.Contains(ETX)) 
                {
                    // Get the Indices of STX and ETX
                    int STXIndex = ReceivedMsg.IndexOf(STX);
                    int ETXIndex = ReceivedMsg.IndexOf(ETX);

                    // Check for Valid STX and ETX Positions
                    if (STXIndex != -1 && ETXIndex != -1 && STXIndex < ETXIndex) 
                    { 
                        // Get the String between STX and ETX
                        string ValidMsg = ReceivedMsg.Substring(STXIndex + 1, ETXIndex - STXIndex - 1);

                        // Split both Sensor Values
                        string[] SensorData = ValidMsg.Split(Seperator);

                        if (int.TryParse(SensorData[0], out int A0Value) &&
                            int.TryParse(SensorData[1], out int A1Value))
                        {
                            SensorA0Value = A0Value;
                            SensorA1Value = A1Value;
                        }
                    }
                    else
                    {
                        SensorA0Value = 0;
                        SensorA1Value = 0;
                    } 
                }
                else if (ReceivedMsg.Contains(IngoreMsg))
                {
                    //SensorA0Value = 0;
                    //SensorA1Value = 0;

                    // Stop the Task
                    //StopCheckingClients();

                    // Try to Reconnect
                    //StartCheckingClients();
                }
                else
                {
                    SensorA0Value = 0;
                    SensorA1Value = 0;
                }

                // Done Parsing, Read the Buffer again
                //ReadClient();
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion
        #endregion
    }
}
