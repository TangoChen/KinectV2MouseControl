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

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CursorViewModel.Quit();
        }


    }
}
