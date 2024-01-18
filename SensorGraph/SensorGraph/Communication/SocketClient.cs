using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensorGraph.Communication
{
    public class SocketClient
    {
        #region Properties
        string ClassName = "SocketClient";
        SocketClient thisClassRef = null;

        // Class Manager Referenc
        ClassManager classManager = null;

        // The Socket Client Object
        Socket tcpSocket = null;
        const int BufferSize = 20;
        byte[] DataBuffer = new byte[BufferSize];

        // Arduino Server Properties
        IPAddress ArduinoIPAddress = IPAddress.Parse("192.168.53.101");
        const int ArduinoServerPort = 8267;

        // Task Definition
        Task<bool> TaskSocketClient = null;
        bool EnableTaskSocketClient = false;

        // Message Properties
        const char STX = (char)2;
        const char ETX = (char)3;
        const char Seperator = (char)124;
        const string IngoreMsg = "\0\0";

        // Sensor Data
        public int SensorA0Value = 0;
        public int SensorA1Value = 0;
        #endregion

        #region Constructor
        public SocketClient(ClassManager classManager)
        {
            thisClassRef = this;
            this.classManager = classManager;
        }
        #endregion

        #region Init Exit
        public void Init()
        {
            string MethodName = "Init()";

            try
            {
                if (CreateInstances())
                {
                    ConnectToServer();
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
                // Stop the Task
                EnableTaskSocketClient = false;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Events
        public EventHandler<ComDataReceivedDataArgs> OnNewSensorData;

        void NewSensorData(int A0Data, int A1Data)
        {
            if (OnNewSensorData != null)
            {
                OnNewSensorData.Invoke(thisClassRef, new ComDataReceivedDataArgs { A0SensorData = A0Data, A1SensorData = A1Data});
            }
        }
        #endregion

        #region Methods
        bool CreateInstances()
        {
            string MethodName = "CreateInstances()";
            bool RetValue = false;

            try
            {
                // Create the Socket Instance
                tcpSocket = new Socket(ArduinoIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                RetValue = true;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }

        private void ParseMessage(string ReceivedMsg)
        {
            string MethodName = "ParseMessage()";

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
                            NewSensorData(A0Value, A1Value);
                            SensorA0Value = A0Value;
                            SensorA1Value = A1Value;
                        }
                        else 
                        {
                            SensorA0Value = 0;
                            SensorA1Value = 0;
                        }
                    }
                    else
                    {
                        SensorA0Value = 0;
                        SensorA1Value = 0;
                    }
                }
                else
                {
                    SensorA0Value = 0;
                    SensorA1Value = 0;
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Socket Methods
        async void ConnectToServer()
        {
            string MethodName = "ConnectToServer()";

            try
            {
                // Create the EndPoint for the Arduino Socket Server
                IPEndPoint SocketClientEndPoint = new IPEndPoint(ArduinoIPAddress, ArduinoServerPort);

                IAsyncResult asyncConnectResult = null;

                await Task.Run(() => 
                {
                    // Connect to the EndPoint
                    asyncConnectResult = tcpSocket.BeginConnect(SocketClientEndPoint, new AsyncCallback(SocketConnectCallBack), tcpSocket);
                    asyncConnectResult.AsyncWaitHandle.WaitOne();
                });


                //// Async wait for the Socket Completion
                //if (asyncConnectResult != null)
                //{
                //    asyncConnectResult.AsyncWaitHandle.WaitOne();
                //}
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        void SocketConnectCallBack(IAsyncResult ar)
        {
            string MethodName = "SocketConnectCallBack()";

            try
            {
                // Retrieve the Socket Object
                Socket ConnectedSocket = (Socket)ar.AsyncState;

                // Complete the Connection
                ConnectedSocket.EndConnect(ar);

                if (ConnectedSocket.Connected)
                {
                    // Start the Reader
                    StartSocketClient();
                }
                else
                {
                    // Retry a Connection
                    ConnectToServer();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        async void StartSocketClient()
        {
            string MethodName = "StartSocketClient()";

            try
            {
                if (TaskSocketClient == null || TaskSocketClient.Status != TaskStatus.Running)
                {
                    // Enable the Loop
                    EnableTaskSocketClient = true;

                    // Create the Task
                    TaskSocketClient = new Task<bool>(() => ReadSocketServer());

                    // Start the Task and wait for the Result
                    TaskSocketClient.Start();
                    bool TaskFinished = await TaskSocketClient;
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        bool ReadSocketServer()
        {
            string MethodName = "ReadSocketServer()";
            bool RetValue = false;

            try
            {
                while (EnableTaskSocketClient)
                {
                    IAsyncResult asyncSocketResult = tcpSocket.BeginReceive(DataBuffer, 0, BufferSize, SocketFlags.None, new AsyncCallback(SocketReadCallBack), tcpSocket);
                    asyncSocketResult.AsyncWaitHandle.WaitOne();

                    // Prevent High CPU-Usage
                    Thread.Sleep(1);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }

        void SocketReadCallBack(IAsyncResult ar)
        {
            string MethodName = "SocketReadCallBack()";

            try
            {
                // Get the Socket Object
                Socket ReadClient = (Socket)ar.AsyncState;

                // Check for a Connection
                if (ReadClient.Connected)
                {

                    // Get the DataBuffer
                    int BytesReceived = ReadClient.EndReceive(ar);

                    // Check the Incoming Data
                    if (BytesReceived > 0)
                    {
                        string ReceivedMsg = Encoding.UTF8.GetString(DataBuffer, 0, BytesReceived);

                        // Get the STX and ETX Indeces
                        int STXIndex = ReceivedMsg.IndexOf(STX);
                        int ETXIndex = ReceivedMsg.IndexOf(ETX);

                        if (ReceivedMsg.Contains(STX) && ReceivedMsg.Contains(ETX) && (STXIndex < ETXIndex))
                        {
                            ParseMessage(ReceivedMsg);
                        }
                    }
                }
                else
                {
                    // Disconnected, Retry
                    ReadClient.EndReceive(ar);
                    StartSocketClient();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion
    }

    public class ComDataReceivedDataArgs : EventArgs
    {
        public IPAddress IPAddress { get; set; }
        public double A0SensorData { get; set; }
        public double A1SensorData { get;set; }
    }
}
