using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
            if (CursorViewModel.Minimized != 0)
            {
                this.WindowState = WindowState.Minimized;
            }

        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            CursorViewModel.ResetToDefault();
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = sender as System.Windows.Controls.Button;

            int Enable = CursorViewModel.Enable;
            if (Enable == (int)KinectCursor.ControlMode.Disabled)
            {
                CursorViewModel.Enable = ~(int)KinectCursor.ControlMode.Disabled;
                b.Content = "Stop";
            }
            else
            {
                CursorViewModel.Enable = (int)KinectCursor.ControlMode.Disabled;
                b.Content = "Start";
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CursorViewModel.Quit();
        }


    }
}
