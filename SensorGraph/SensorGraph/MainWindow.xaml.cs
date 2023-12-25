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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorGraph
{
    /// <summary>
    /// Error Handling for the Application
    /// </summary>
    public class ErrorHandling
    {
        public static void ShowException(Exception Ex, string MethodName)
        {
            string ShowMessage = string.Format("Exception: {0}. Occured at {1}", Ex.Message, MethodName);

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

        // Reference to the Class Manager
        ClassManager classManager = null;
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

                }
            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }
        }

        private void Exit()
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

        #region Page Events
        private void PageLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void PageClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

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

            }
            catch (Exception Ex)
            {
                ErrorHandling.ShowException(Ex, MethodName);
            }

            return RetValue;
        }
        #endregion

        
    }
}
