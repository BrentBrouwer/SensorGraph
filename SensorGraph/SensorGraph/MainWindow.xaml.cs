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
        public static string ExceptionMsg;

        public static string ExceptionSource;
        public static DateTime ExceptionDateTime;
        

        public static void ShowException(Exception Ex, string MethodName, string ClassName)
        {
            ExceptionMsg = Ex.Message;
            ExceptionDateTime = DateTime.Now;
            ExceptionSource = string.Format("{0}.\n{1}", ClassName, MethodName);

            //string ShowMessage = string.Format("Exception at: {0}.{1}. \nMessage: {2}", ClassName, MethodName, Ex.Message);
            //MessageBox.Show(ShowMessage);
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

        // Sample Interval
        long sampleInterval = 0;
        public long SampleInterval
        {
            get
            {
                return sampleInterval;
            }

            set
            {
                if (value > 0)
                {
                    sampleInterval = value;

                    // Set the Timer Interval
                    SampleIntervalTimer.Stop();
                    SampleIntervalTimer.Interval = sampleInterval;
                    SampleIntervalTimer.Start();
                }
            }
        }

        // Sample Interval Timer
        System.Timers.Timer SampleIntervalTimer = null;

        // Sample that is Taken
        int A0TakenSample = 0;
        int A1TakenSample = 0;
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
                        // Exception
                        if (ErrorHandling.ExceptionMsg != null &&
                            ErrorHandling.ExceptionDateTime != null &&
                            ErrorHandling.ExceptionMsg != null)
                        {
                            ExceptionSourceValue.Text = ErrorHandling.ExceptionSource;
                            ExceptionTimeValue.Text = ErrorHandling.ExceptionDateTime.ToString();
                            ExceptionValue.Text = ErrorHandling.ExceptionMsg;
                        }
                        else
                        {
                            ExceptionSourceValue.Text = string.Empty;
                            ExceptionTimeValue.Text = string.Empty;
                            ExceptionValue.Text = string.Empty;
                        }

                        // Data
                        ArduinoConnectValue.Text = classManager.socketCommunication.ClientConnected ? "Connected" : "Disconnected";
                        ArduinoRawDataValue.Text = classManager.socketCommunication.IncomingData;
                        //SensorA0DataValue.Text = classManager.socketCommunication.SensorA0Value.ToString();
                        //SensorA1DataValue.Text = classManager.socketCommunication.SensorA1Value.ToString();
                    }));
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void SampleIntervalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string MethodName = "UIUpdateTimer_Elapsed()";

            try
            {
                // Take the Current Value of Sensors A0 and A1
                A0TakenSample = classManager.socketCommunication.SensorA0Value;
                A1TakenSample = classManager.socketCommunication.SensorA1Value;

                thisClassRef.Dispatcher.Invoke(new Action(() => 
                {
                    SensorA0DataValue.Text = A0TakenSample.ToString();
                    SensorA1DataValue.Text = A1TakenSample.ToString();
                }));
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void SampleValue_Changed(object sender, KeyEventArgs e)
        {
            string MethodName = "SampleValue_Changed";

            try
            {
                // Only Continue when the Enter-Key is Pressed
                if (e.Key == Key.Enter)
                {
                    string SampleIntervalText = SampleIntervalValue.Text;

                    if (int.TryParse(SampleIntervalText, out int SampleIntervalms))
                    {
                        SampleInterval = SampleIntervalms;

                        // Update the UI
                        SampleIntervalValue.Text = SampleInterval.ToString();
                    }
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

                // Create the Sample Interval Timer
                SampleIntervalTimer = new System.Timers.Timer();
                SampleIntervalTimer.Interval = TimeSpan.FromMilliseconds(50).TotalMilliseconds;
                SampleIntervalTimer.AutoReset = true;
                SampleIntervalTimer.Elapsed += (sender, e) => SampleIntervalTimer_Elapsed(sender, e);
                SampleIntervalTimer.Start();

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
                // Settings
                SettingsHeader.Text = "Settings";
                SampleIntervalText.Text = "Sample Interval\n[ms]";
                SampleIntervalValue.Text = "1000";

                // DataDisplay
                DataDisplayHeader.Text = "Data";
                ArduinoConnectText.Text = "Arduino State";
                ArduinoRawDataText.Text = "Raw Data";
                ArduinoRawDataValue.Text = "-";
                ArduinoConnectValue.Text = "-";
                SensorA0DataText.Text = "Sensor A0 Value";
                SensorA0DataValue.Text = "-";
                SensorA1DataText.Text = "Sensor A1 Value";
                SensorA1DataValue.Text = "-";

                // Chart

                // Exception
                ExceptionSourceText.Text = "Source";
                ExceptionSourceValue.Text = "-";
                ExceptionTimeText.Text = "Time";
                ExceptionTimeValue.Text = "-";
                ExceptionText.Text = "Exception\nMessage";
                ExceptionValue.Text = "-";
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        #endregion
    }
}
