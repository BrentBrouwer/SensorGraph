using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SensorGraph.PopUp
{
    /// <summary>
    /// Interaction logic for RawData.xaml
    /// </summary>
    public partial class RawData : Window
    {
        #region Properties
        private string ClassName = "RawData";

        // Reference to the MainWindow
        MainWindow mainWindow = null;
        #endregion

        #region Constructor
        public RawData(MainWindow mainWindowRef)
        {
            this.mainWindow = mainWindowRef;

            InitializeComponent();

            InitPageData();
        }
        #endregion

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string MethodName = "Window_Closing";

            try
            {
                mainWindow.rawDataWindow = null;
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }
        #endregion

        #region Methods
        public void InitPageData()
        {
            string MethodName = "InitPageData()";

            try
            {
                // DataDisplay
                DataDisplayHeader.Text = "Data";
                ArduinoConnectText.Text = "Arduino State";
                ArduinoRawDataText.Text = "Raw Data";
                ArduinoRawDataValue.Text = "-";
                ArduinoConnectValue.Text = "-";
                SensorA0DataText.Text = "Sensor A0\nValue";
                SensorA0DataValue.Text = "-";
                SensorA1DataText.Text = "Sensor A1\nValue";
                SensorA1DataValue.Text = "-";
                PressureText.Text = "Pressure [Bar]";
                PressureValue.Text = "-";
                FlowText.Text = "Flow [L/S]";
                FlowValue.Text = "-";
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName, ClassName);
            }
        }

        public void UpdateUI()
        {

        }
        #endregion

    }
}
