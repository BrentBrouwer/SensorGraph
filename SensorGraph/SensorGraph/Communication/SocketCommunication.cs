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

        // Reference to the ClassManager
        ClassManager classManager = null;

        // Task Properties
        Task<bool> TaskAsyncServerReading = null;
        bool EnableServerReading = false;

        // TCP Socket Properties
        Socket ClientSocket = null;

        // Socket Settings
        IPAddress ipAddress = null;
        int PortNr = 8267;
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
                    EstablishConnection();

                    // Start the Async Reading of the Clients
                    StartServerReading();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
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
                ErrorHandling.ShowException(Ex, MethodName);
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


                RetValue = true;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }

            return RetValue;
        }

        void StartServerReading()
        {
            string MethodName = "AsyncServerReading()";

            try
            {
                if (TaskAsyncServerReading == null || TaskAsyncServerReading.Status != TaskStatus.Running)
                {
                    EnableServerReading = true;
                    TaskAsyncServerReading = new Task<bool>(() => AsyncServerReading());
                    TaskAsyncServerReading.Start();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }
        }

        bool AsyncServerReading()
        {
            string MethodName = "AsyncServerReading()";
            bool RetValue = false;

            bool WaitForConnection = true;

            try
            {
                while (EnableServerReading)
                {
                    do
                    {

                    }
                    while (WaitForConnection);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }

            return RetValue;
        }

        bool EstablishConnection()
        {
            string MethodName = "AsyncServerReading()";
            bool RetValue = false;

            bool WaitForConnection = true;

            try
            {
                // Get the First IP Address of the LocalHost List
                IPHostEntry host = Dns.GetHostEntry("localhost");
                ipAddress = host.AddressList[1];
                IPEndPoint endPoint = new IPEndPoint(ipAddress, PortNr);

                // Create the Socket Object
                ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Try to Connect to the EndPoint
                IAsyncResult asyncConnectionResult = ClientSocket.BeginConnect(endPoint, new AsyncCallback(SocketConnectCallBack), ClientSocket);

                // Check from QliqFlowBase --> ValorClientSocketIO.cs
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }

            return RetValue;
        }

        void SocketConnectCallBack(IAsyncResult ar)
        {

        }
        #endregion
    }
}
