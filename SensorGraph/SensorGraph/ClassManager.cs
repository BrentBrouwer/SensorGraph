using SensorGraph.Arduino;
using SensorGraph.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SensorGraph
{
    public class ClassManager
    {
        #region Properties
        // Own Reference
        ClassManager thisClassRef = null;
        string ClassName = "ClassManager";

        // Reference to the Main
        MainWindow mainWindow = null;

        // The Instances Controlled by the ClassManager
        public ArduinoCOM arduinoCOM = null;
        //public SocketCommunication socketCommunication = null;
        public SocketClient socketClient = null;
        #endregion

        #region Constructor
        public ClassManager(MainWindow mainWindow)
        {
            // Set the References
            thisClassRef = this;
            this.mainWindow = mainWindow;
        }
        #endregion

        #region InitExit
        public void Init()
        {
            string MethodName = "Init()";

            try
            {
                // Create the Instances
                if (CreateInstances())
                {
                    // Initialize the Instances
                    //arduinoCOM.Init();
                    //socketCommunication.Init();
                    //socketClient.Init();
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
                // Dispose the Object
                if (arduinoCOM != null)
                {
                    arduinoCOM.Exit();
                    arduinoCOM = null;
                }

                if (socketClient != null) 
                {
                    socketClient.Exit();
                    socketClient = null;
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
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
                // Create the Instances that are Controlled by the Class Manager
                //socketCommunication = new SocketCommunication(thisClassRef);
                arduinoCOM = new ArduinoCOM(thisClassRef);
                socketClient = new SocketClient(thisClassRef);

                RetValue = true;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }
        #endregion
    }
}
