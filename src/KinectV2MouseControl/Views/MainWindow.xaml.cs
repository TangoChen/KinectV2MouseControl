using System.Windows;

namespace KinectV2MouseControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CursorViewModel.LoadSettings();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            CursorViewModel.ResetToDefault();
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            int Enable = CursorViewModel.Enable;
            if (Enable == (int)KinectCursor.ControlMode.Disabled)
               CursorViewModel.Enable = ~(int)KinectCursor.ControlMode.Disabled;
            else
               CursorViewModel.Enable = (int)KinectCursor.ControlMode.Disabled;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CursorViewModel.Quit();
        }


    }
}
