using SensorGraph.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorGraph.Arduino
{
    public  class ArduinoCOM
    {
        #region Properties
        // Own Reference
        ArduinoCOM thisClassRef = null;
        string ClassName = "ArduinoCOM";

        // Reference to the ClassManager
        ClassManager classManager = null;
        #endregion

        #region Constructor
        public ArduinoCOM(ClassManager classManager)
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

        #region Methods
        bool CreateInstances()
        {
            string MethodName = "CreateInstances()";
            bool RetValue = false;

            try
            {
                // Create the Instances that are Controlled by the Class Manager


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
