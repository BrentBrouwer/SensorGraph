using SensorGraph.Arduino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        const int MaxRecBufferSize = 2048;
        public bool ClientConnected = false;
        public int SensorValue = 0;

        // Client Properties (Devices that connect to this Server)
        TcpClient tcpClient = null;
        // Array of Devices that are able to connect, maximum of 10
        //TcpClient[] tcpClients = new TcpClient[10];
        #endregion

        #region Constructor
        public SocketCommunication(ClassManager classManager)
        {
            // Set the References
            thisClassRef = this;
            this.classManager = classManager;
        }
        #endregion

        #region InitExit
        public void Init()
        {
            string MethodName = "Init()";

            try
            {
                if (CreateInstances())
                {
                    // Start the Server
                    tcpServer.Start();

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
                    EnableCheckClients = true;
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

        bool AsyncReadClient()
        {
            string MethodName = "AsyncReadClient()";
            bool RetValue = false;

            try
            {
                while (EnableCheckClients)
                {
                    // Check for Connected Clients
                    tcpClient = tcpServer.AcceptTcpClient();

                    // Get the NetworkStream to Send/Receive Messages
                    NetworkStream networkStream = tcpClient.GetStream();

                    // Check if the Client is Connected
                    if (tcpClient.Connected)
                    {
                        ClientConnected = true;

                        // Create the Buffer
                        byte[] buffer = new byte[MaxRecBufferSize];

                        // Read the Clients Buffer
                        networkStream.Read(buffer, 0, MaxRecBufferSize);

                        string ReceivedMsg = Encoding.UTF8.GetString(buffer);

                        if (ReceivedMsg.Length > 0)
                        {
                            // Parse the Message
                            ParseMessage(ReceivedMsg);
                        }
                    }
                    else
                    {
                        ClientConnected = false;
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

        private void ParseMessage(string ReceivedMsg)
        {
            string MethodName = "ParseMessage()";
            
            try
            {
                // Done Parsing, Read the Buffer again
                
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Reading Data
        
        #endregion
        #endregion
    }
}
