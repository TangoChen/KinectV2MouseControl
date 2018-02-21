using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlMode = KinectV2MouseControl.KinectCursor.ControlMode;

namespace KinectV2MouseControl
{
    public class KinectCursorViewModel : INotifyPropertyChanged
    {
        KinectCursor kinectCursor;

        const double DEFAULT_MOVE_SCALE = 1f;
        const double DEFAULT_SMOOTHING = 0.2f;
        const double DEFAULT_HOVER_RANGE = 20f;
        const double DEFAULT_HOVER_DURATION = 2;

        public KinectCursorViewModel()
        {
            kinectCursor = new KinectCursor();
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double MoveScale
        {
            get
            {
                return kinectCursor.MoveScale;
            }
            set
            {
                kinectCursor.MoveScale = value;
                RaisePropertyChanged();
            }
        }

        public double Smoothing
        {
            get
            {
                return kinectCursor.Smoothing;
            }
            set
            {
                kinectCursor.Smoothing = value;
                RaisePropertyChanged();
            }
        }

        public double HoverRange
        {
            get
            {
                return kinectCursor.HoverRange;
            }
            set
            {
                kinectCursor.HoverRange = value;
                RaisePropertyChanged();
            }
        }

        public double HoverDuration
        {
            get
            {
                return kinectCursor.HoverDuration;
            }
            set
            {
                kinectCursor.HoverDuration = value;
                RaisePropertyChanged();
            }
        }

        public int ControlModeIndex
        {
            get
            {
                return (int)kinectCursor.Mode;
            }
            set
            {
                kinectCursor.Mode = (ControlMode)value;
                RaisePropertyChanged();
            }
        }

        public void LoadSettings()
        {
            MoveScale = Properties.Settings.Default.MoveScale;
            HoverDuration = Properties.Settings.Default.HoverDuration;
            HoverRange = Properties.Settings.Default.HoverRange;
            Smoothing = Properties.Settings.Default.Smoothing;

            ControlModeIndex = Properties.Settings.Default.Mode;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.MoveScale = MoveScale;
            Properties.Settings.Default.Smoothing = Smoothing;
            Properties.Settings.Default.HoverRange = HoverRange;
            Properties.Settings.Default.HoverDuration = HoverDuration;
            Properties.Settings.Default.Mode = ControlModeIndex;

            Properties.Settings.Default.Save();
        }

        public void ResetToDefault()
        {
            MoveScale = DEFAULT_MOVE_SCALE;
            Smoothing = DEFAULT_SMOOTHING;
            HoverRange = DEFAULT_HOVER_RANGE;
            HoverDuration = DEFAULT_HOVER_DURATION;
        }

        public void Quit()
        {
            SaveSettings();
            ControlModeIndex = 0;
        }

    }
}
