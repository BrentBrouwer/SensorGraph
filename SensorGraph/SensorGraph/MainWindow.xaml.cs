using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorGraph
{
    /// <summary>
    /// Error Handling for the Application
    /// </summary>
    public class ErrorHandling
    {
        public static void ShowException(Exception Ex, string MethodName, string ClassName)
        {
            string ShowMessage = string.Format("Exception at: {0}.{1}. \nMessage: {2}", ClassName, MethodName, Ex.Message);

            MessageBox.Show(ShowMessage);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        // Own Reference
        MainWindow thisClassRef = null;
        string ClassName = "MainWindow";

        // Reference to the Class Manager
        ClassManager classManager = null;

        // Page Loaded flag
        bool PageLoadedFlag = false;

        // The UI Update Timer
        System.Timers.Timer UIUpdateTimer = null;
        #endregion

        #region Constructor
        public MainWindow()
        {
            // Set the Reference
            thisClassRef = this;

            Init();

            InitializeComponent();                     
        }
        #endregion

        #region InitExit
        private void Init()
        {
            string MethodName = "Init()";

            try
            {
                if (CreateInstances())
                {
                    classManager.Init();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void Exit()
        {
            string MethodName = "Exit()";

            try
            {
                // Stop the UIUpdate Timer
                if (UIUpdateTimer != null)
                {
                    UIUpdateTimer.Stop();
                    UIUpdateTimer.Dispose();
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Page Events
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            string MethodName = "PageLoaded()";

            try
            {
                PageLoadedFlag = true;

                InitPageData();
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void PageClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string MethodName = "PageClosing()";

            try
            {
                Exit();
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void UIUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string MethodName = "UIUpdateTimer_Elapsed()";

            try
            {
                // Socket Communication
                if (PageLoadedFlag && classManager.socketCommunication != null)
                {
                    thisClassRef.Dispatcher.Invoke(new Action(() => 
                    {
                        ArduinoConnectValue.Text = classManager.socketCommunication.ClientConnected ? "Connected" : "Disconnected";
                        SensorDataValue.Text = classManager.socketCommunication.SensorValue.ToString();
                    }));
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Methods
        private bool CreateInstances()
        {
            string MethodName = "CreateInstances()";
            bool RetValue = false;

            try
            {
                // Create the Objects
                classManager = new ClassManager(thisClassRef);

                // Create the Update Timer
                UIUpdateTimer = new System.Timers.Timer();
                UIUpdateTimer.Interval = TimeSpan.FromMilliseconds(50).TotalMilliseconds;
                UIUpdateTimer.AutoReset = true;
                UIUpdateTimer.Elapsed += (sender, e) => UIUpdateTimer_Elapsed(sender, e);
                UIUpdateTimer.Start();

                RetValue = true;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return RetValue;
        }

        private void InitPageData()
        {
            string MethodName = "InitPageData()";

            try
            {
                // TextBoxes
                ArduinoConnectText.Text = "Arduino State";
                ArduinoConnectValue.Text = "-";
                SensorDataText.Text = "Sensor Value";
                SensorDataValue.Text = "-";

                // Chart
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        
    }
}
