using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
//using QliqFlowBase.ValorUtils.Logging;
using SensorGraph.PopUp;
using SensorGraph.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        

        public static void ShowException(Exception Ex, string MethodName, string ClassName, string LogMessage = "")
        {
            ExceptionMsg = LogMessage;
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

                    // Reset the Button visualisation
                    StartStopBtn.Content = "Start";
                    StartStopBtn.Background = Brushes.LightGreen;
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
        InteractiveDataDisplay.WPF.LineGraph DataLine = null;

        // The Voltage Point Collection Instances for storing the datapoints
        VoltagePointCollection voltagePointCollectionA0 = null;
        VoltagePointCollection voltagePointCollectionA1 = null;

        // The Raw Data Window
        public RawData rawDataWindow = null;

        // Data to Store in Excel
        Dictionary<DateTime, double[]> DataCollection = null;

        // Scale minimum and maximum values
        int GraphScaleMin = 0;
        int GraphScaleMax = 5000;
        #endregion

        #region Constructor
        public MainWindow()
        {
            // Set the Reference
            thisClassRef = this;

            InitializeComponent();

            Init();
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
                    classManager.socketClient.Init();
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

                //loggerMain.Info("MW002 Exited mainwindow");
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
                if (PageLoadedFlag && classManager.socketClient != null)
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
                        //if (rawDataWindow != null)
                        //{
                        //    rawDataWindow.ArduinoConnectValue.Text = classManager.socketClient.ClientConnected ? "Connected" : "Disconnected";
                        //    rawDataWindow.ArduinoRawDataValue.Text = classManager.socketCommunication.IncomingData;
                        //}
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
            string MethodName = "SampleIntervalTimer_Elapsed()";

            try
            {
                // Take the Current Value of Sensors A0 and A1
                A0TakenSample = classManager.socketClient.SensorA0Value;
                A1TakenSample = classManager.socketClient.SensorA1Value;

                // Take the TimeStamp
                DateTime CurrentTimeStamp = DateTime.Now;
                string CurrentDayTime = CurrentTimeStamp.ToString("MM/dd/yyyy hh:mm:ss.fff tt");

                // Calculate to the Correct Units
                double Pressure = ConvertToPressure(A0TakenSample);
                double Flow = ConvertToFlow(A1TakenSample);
                double A0MilliVolts = ConvertToMilliVolts(A0TakenSample);
                double A1MilliVolts = ConvertToMilliVolts(A1TakenSample);

                // Update the UI
                if (rawDataWindow != null)
                {
                    rawDataWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        SensorA0DataValue.Text = A0TakenSample.ToString();
                        SensorA1DataValue.Text = A1TakenSample.ToString();
                        PressureValue.Text = Pressure.ToString();
                        FlowValue.Text = Flow.ToString();
                    }));
                }

                // Add to the Chart
                //AddDataPoint(Pressure, Flow);

                thisClassRef.Dispatcher.Invoke(new Action(() =>
                {
                    voltagePointCollectionA0.Add(new VoltagePoint(DateTime.Now, ConvertToMilliVolts(A0TakenSample)));
                    voltagePointCollectionA1.Add(new VoltagePoint(DateTime.Now, ConvertToMilliVolts(A1TakenSample)));
                }));

                // Add the Data to the Dictionary
                DataCollection.Add(CurrentTimeStamp, new double[2] { Pressure, Flow });
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

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            string MethodName = "StartStopBtn_CLick";

            try
            {
                if (StartStopBtn.Content == "Start")
                {
                    StartStopBtn.Content = "Stop";
                    StartStopBtn.Background = Brushes.IndianRed;
                    SampleIntervalTimer.Start();
                }
                else
                {
                    SampleIntervalTimer.Stop();
                    StartStopBtn.Content = "Start";
                    StartStopBtn.Background = Brushes.LightGreen;
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void ShowRawDataBtn_Click(object sender, RoutedEventArgs e)
        {
            string MethodName = "ShowRawDataBtn";

            try
            {
                if (rawDataWindow == null)
                {
                    rawDataWindow = new RawData(thisClassRef);
                    rawDataWindow.Show();
                }
                else
                {
                    Exception WindowEx = new Exception("Data window already open");
                    ErrorHandling.ShowException(WindowEx, MethodName, ClassName);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void ExportToCSVBtn_Click(object sender, RoutedEventArgs e)
        {
            string MethodName = "ExportToCSVBtn_Click";

            try
            {
                // Create the Object for the SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
                saveFileDialog.Title = "Save The Parameter File";
                DateTime date = DateTime.Now;
                string FileName = string.Format("DataCollection_{0}-{1}-{2}.{3}", date.Day, date.Month, date.Year, "csv");
                saveFileDialog.FileName = FileName;

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Create the File async
                    ExportToCSV(saveFileDialog.FileName);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        private void ConfirmScaleSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            string MethodName = "ConfirmScaleSettingsBtn_Click";

            try
            {
                // Get the Current Values
                if (int.TryParse(ScaleMinValue.Text, out int MinValue) &&
                    int.TryParse(ScaleMaxValue.Text, out int MaxValue))
                {
                    Exit();
                    Init();
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
                voltagePointCollectionA0 = new VoltagePointCollection();
                voltagePointCollectionA1 = new VoltagePointCollection();

                // Create the Dictionary
                DataCollection = new Dictionary<DateTime, double[]>();

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
                SampleIntervalTimer.Interval = TimeSpan.FromMilliseconds(1000).TotalMilliseconds;
                SampleIntervalTimer.AutoReset = true;
                SampleIntervalTimer.Elapsed += (sender, e) => SampleIntervalTimer_Elapsed(sender, e);

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
                StartStopBtn.Content = "Start";
                StartStopBtn.Background = Brushes.LightGreen;
                ShowRawDataBtn.Content = "Show Raw Data";
                ExportToCSVBtn.Content = "Export To CSV File";
                ScaleMinText.Text = "Scale Min Value";
                ScaleMinValue.Text = "0";
                ScaleMaxText.Text = "Scale Max Value";
                ScaleMaxValue.Text = "5000";
                ConfirmScaleSettingsBtn.Content = "Confirm Scale\nSettings";
                ScaleMinValueSP.Visibility = Visibility.Collapsed;
                ScaleMaxValueSP.Visibility = Visibility.Collapsed;
                ConfirmScaleSettingsBtn.Visibility = Visibility.Collapsed;

                // Chart
                //SensorChart.Title = "Time vs MilliVolts";
                //SensorChart.BottomTitle = "Time";
                //SensorChart.LeftTitle = "Voltage [mV]";

                //// Axis Properties
                //SensorChart.PlotOriginX = 0;
                //SensorChart.PlotOriginY = 0;
                //SensorChart.PlotWidth = 10000;
                //SensorChart.PlotHeight = 20;
                //SensorChart.IsAutoFitEnabled = false;

                // Exception
                ExceptionSourceText.Text = "Source";
                ExceptionSourceValue.Text = "-";
                ExceptionTimeText.Text = "Time";
                ExceptionTimeValue.Text = "-";
                ExceptionText.Text = "Exception\nMessage";
                ExceptionValue.Text = "-";

                //loggerMain.Info("MW006 Mainwindow initialized");
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

                // Dynamic Data Display Chart
                var A0Line = new EnumerableDataSource<VoltagePoint>(voltagePointCollectionA0);
                A0Line.SetXMapping(x => DateTimeAxis.ConvertToDouble(x.Date));
                A0Line.SetYMapping(y => y.Voltage);
                TimeChart.AddLineGraph(A0Line, Colors.Blue, 2, "Pressure");

                var A1Line = new EnumerableDataSource<VoltagePoint>(voltagePointCollectionA1);
                A1Line.SetXMapping(x => DateTimeAxis.ConvertToDouble(x.Date));
                A1Line.SetYMapping(y => y.Voltage);
                TimeChart.AddLineGraph(A1Line, Colors.Red, 2, "Flow");
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
                        //SensorGrid.Children.Clear();
                        //SensorGrid.Children.Add(DataLine);


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

        private double ConvertToMilliVolts(int AnalogValue)
        {
            string MethodName = "ConvertToMilliVolts()";
            double MilliVolts = 0;

            // Sensor Properties
            double MinVolts = 0;
            double MaxVolts = 5000;

            // Analog Properties
            double MinAnalog = 0;
            double MaxAnalog = 1023;

            try
            {
                if (AnalogValue > 0)
                {
                    // Ratio of Bar/
                    double Ratio = (MaxVolts - MinVolts) / (MaxAnalog - MinAnalog);

                    // Calculate the Voltage
                    MilliVolts = (double)(Ratio * AnalogValue);

                    // Two Decimals
                    MilliVolts = Math.Round(MilliVolts, 2);
                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }

            return MilliVolts;
        }

        async void ExportToCSV(string Path)
        {
            string MethodName = "ExportToCSV()";
            string WriteLineData = string.Empty;

            try
            {
                _ = Task.Run(() =>
                {
                    // Always create a new file
                    if (File.Exists(Path))
                    {
                        File.Delete(Path);
                    }

                    // Write the Parameter Data per Line
                    using (var parfileStream = new FileStream(Path, FileMode.CreateNew, FileAccess.Write))
                    {
                        // Line Layout
                        foreach (KeyValuePair<DateTime, double[]> DataPoint in DataCollection)
                        {
                            WriteLineData = string.Format("{0};{1};{2}",
                                            DataPoint.Key,
                                            DataPoint.Value[0],
                                            DataPoint.Value[1]);

                            WriteLineData = WriteLineData + Environment.NewLine;

                            // Write the Data as a Byte Array
                            byte[] WriteLinebytes = Encoding.ASCII.GetBytes(WriteLineData);
                            parfileStream.Write(WriteLinebytes, 0, WriteLineData.Length);
                        }
                    }
                });
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion
    }
}
