using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlMode = KinectV2MouseControl.KinectCursor.ControlMode;

namespace KinectV2MouseControl
{
    public class KinectCursorViewModel : INotifyPropertyChanged
    {
        KinectCursor kinectCursor;

        const double DEFAULT_MOVE_SCALE = 1.0;
        const double DEFAULT_SMOOTHING = 0.80;
        const double DEFAULT_HOVER_RANGE = 20.0;
        const double DEFAULT_HOVER_DURATION = 2.0;
        const double DEFAULT_FOREARM_RATIO_FOR_DEADZONE = 2.1;

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

        public double ForearmRatioForDeadzone
        {
            get
            {
                return kinectCursor.ForearmRatioForDeadzone;
            }
            set
            {
                kinectCursor.ForearmRatioForDeadzone = value;
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

        public int Enable
        {
            get
            {
                return (int)kinectCursor.Enable;
            }
            set
            {
                kinectCursor.Enable = (ControlMode)value;
                RaisePropertyChanged();
            }
        }

        int _Minimized = 0;
        public int Minimized
        {
            get
            {
                return (int)_Minimized;
            }
            set
            {
                _Minimized = (int)value;
                RaisePropertyChanged();
            }
        }

        public void LoadSettings()
        {
            MoveScale = Properties.Settings.Default.MoveScale;
            HoverDuration = Properties.Settings.Default.HoverDuration;
            HoverRange = Properties.Settings.Default.HoverRange;
            Smoothing = Properties.Settings.Default.Smoothing;
            ForearmRatioForDeadzone = Properties.Settings.Default.ForearmRatioForDeadzone;

            ControlModeIndex = Properties.Settings.Default.Mode;
            Enable = Properties.Settings.Default.Enable;
            Minimized = Properties.Settings.Default.Minimized;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.MoveScale = MoveScale;
            Properties.Settings.Default.Smoothing = Smoothing;
            Properties.Settings.Default.HoverRange = HoverRange;
            Properties.Settings.Default.HoverDuration = HoverDuration;
            Properties.Settings.Default.ForearmRatioForDeadzone = ForearmRatioForDeadzone;
            Properties.Settings.Default.Mode = ControlModeIndex;
            Properties.Settings.Default.Enable = Enable;
            Properties.Settings.Default.Minimized = Minimized;

            Properties.Settings.Default.Save();
        }

        public void ResetToDefault()
        {
            MoveScale = DEFAULT_MOVE_SCALE;
            Smoothing = DEFAULT_SMOOTHING;
            HoverRange = DEFAULT_HOVER_RANGE;
            HoverDuration = DEFAULT_HOVER_DURATION;
            ForearmRatioForDeadzone = DEFAULT_FOREARM_RATIO_FOR_DEADZONE;
            Enable = 1;
            Minimized = 0;
        }

        public void Quit()
        {
            SaveSettings();
            ControlModeIndex = 0;
            Enable = 0;
        }

    }
}
