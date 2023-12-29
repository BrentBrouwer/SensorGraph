using InteractiveDataDisplay.WPF;
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

        // Data to Display
        const int MaxDataPoints = 10000;
        double[] Xvalues = null;
        double[] Yvalues = null;
        LineGraph DataLine = null;
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

                // Calculate to the Correct Units
                double Pressure = ConvertToPressure(A0TakenSample);
                double Flow = ConvertToFlow(A1TakenSample);

                // Update the UI
                thisClassRef.Dispatcher.Invoke(new Action(() => 
                {
                    SensorA0DataValue.Text = A0TakenSample.ToString();
                    SensorA1DataValue.Text = A1TakenSample.ToString();
                    PressureValue.Text = Pressure.ToString();
                    FlowValue.Text = Flow.ToString();
                }));

                // Add to the Chart
                AddDataPoint(Pressure, Flow);
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
                Xvalues = new double[1] { 0 };
                Yvalues = new double[1] { 0 };

                // Create the ChartLine
                CreateChartLines();

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
                PressureText.Text = "Pressure [Bar]";
                PressureValue.Text = "-";
                FlowText.Text = "Flow [L/S]";
                FlowValue.Text = "-";

                // Chart
                // Titles
                SensorChart.Title = "Pressure vs Flow";
                SensorChart.BottomTitle = "Pressure [Bar] (A0)";
                SensorChart.LeftTitle = "Flow [L/S] (A1)";
                // Axis Properties
                SensorChart.PlotOriginX = 0;
                SensorChart.PlotOriginY = 0;
                SensorChart.PlotWidth = 10;
                SensorChart.PlotHeight = 20;
                SensorChart.IsAutoFitEnabled = false;

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

        private void CreateChartLines()
        {
            string MethodName = "CreateChartLines()";

            try
            {
                DataLine = new InteractiveDataDisplay.WPF.LineGraph
                {
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 1
                };
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void AddDataPoint(double Xvalue, double Yvalue)
        {
            string MethodName = "SetDataPoint()";

            try
            {
                // Create the New Arrays
                double[] NewXDataArray = new double[Xvalues.Length + 1];
                double[] NewYDataArray = new double[Yvalues.Length + 1];

                // Add the New Values
                for (int i = 0; i <= Xvalues.Length; i++)
                {
                    if (i == Xvalues.Length)
                    {
                        NewXDataArray[i] = Xvalue;
                        NewYDataArray[i] = Yvalue;
                    }
                    else
                    {
                        NewXDataArray[i] = Xvalues[i];
                        NewYDataArray[i] = Yvalues[i];
                    }
                }

                // Load Data to the Chart
                if (DataLine != null)
                {
                    thisClassRef.Dispatcher.Invoke(new Action(() => 
                    {
                        DataLine.Plot(NewXDataArray, NewYDataArray);

                        // Add the Lines to the Grid
                        SensorGrid.Children.Clear();
                        SensorGrid.Children.Add(DataLine);
                    }));
                }

                // Assign the New Array as the Old Array
                Xvalues = new double[NewXDataArray.Length];
                Yvalues = new double[NewYDataArray.Length];

                for (int i = 0; i < NewXDataArray.Length; i++)
                {
                    Xvalues[i] = NewXDataArray[i];
                    Yvalues[i] = NewYDataArray[i];
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private double ConvertToPressure(int AnalogValue)
        {
            string MethodName = "ConvertToPressure()";
            double Pressure = 0;

            // Sensor Properties
            double MinPressure = 0;
            double MaxPressure = 10;

            // Analog Properties
            double MinAnalog = 0;
            double MaxAnalog = 1023;

            try
            {
                if (AnalogValue > 0)
                {
                    // Ratio of Bar/
                    double Ratio = (MaxPressure - MinPressure) / (MaxAnalog - MinAnalog);

                    // Calculate the Pressure
                    Pressure = (double)(Ratio * AnalogValue);

                    // Two Decimals
                    Pressure = Math.Round(Pressure, 2);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return Pressure;
        }

        private double ConvertToFlow(int AnalogValue)
        {
            string MethodName = "ConvertToFlow()";
            double Flow = 0;

            // Sensor Properties
            double MinFlow = 0.02;
            double MaxFlow = 20;

            // Analog Properties
            double MinAnalog = 0;
            double MaxAnalog = 1023;

            try
            {
                if (AnalogValue > 0)
                {
                    // Ratio of Bar/
                    double Ratio = (MaxFlow - MinFlow) / (MaxAnalog - MinAnalog);

                    // Calculate the Flow
                    Flow = (double)(Ratio * AnalogValue);

                    // Two Decimals
                    Flow = Math.Round(Flow, 2);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return Flow;
        }
        #endregion
    }
}
