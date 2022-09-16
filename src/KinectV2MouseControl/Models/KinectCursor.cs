using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Kinect;


namespace KinectV2MouseControl
{
    public class KinectCursor
    {
        private KinectReader sensorReader;
        private CursorMapper cursorMapper;

        // public static double PI = 3.141592653589793;
        // public static double degrees30toRads = 0.523598775598299; // PI * 30.0 / 180.0;
        // public static double degrees60toRads = 1.047197551196598; // PI * 60.0 / 180.0;
        public static double tan30degrees = 0.57735026919; // = Math.Tan(degrees30toRads);
        public static double tan60degrees = 1.73205080757; // = Math.Tan(degrees60toRads);

        public enum ControlMode
        {
            Disabled = 0,
            MoveOnly,
            GripToPress,
            HoverToClick,
            MoveGripPressing,
            MoveLiftClicking,
            LeftOrRightMouseDownAndUp_WristPosition,
            LeftOrRightMouseDownAndUp_HandTipPosition
        }

        private ControlMode _mode = ControlMode.Disabled;
        public ControlMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (value == ControlMode.Disabled)
                {
//                    ToggleHoverTimer(false);
//                    sensorReader.Close();
                }
                else
                {
//                    sensorReader.Close();
//                    sensorReader.Open();
                }
            }
        }

        private ControlMode _enable = ControlMode.Disabled;
        public ControlMode Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
                if (value == ControlMode.Disabled)
                {
                    ToggleHoverTimer(false);
                    sensorReader.Close();
                }
                else
                {
                    sensorReader.Close();
                    sensorReader.Open();
                }
            }
        }

        public double Smoothing
        {
            get
            {
                return cursorMapper.Smoothing;
            }
            set
            {
                cursorMapper.Smoothing = value;
            }
        }

        public double ForearmRatioForDeadzone
        {
            get
            {
                return KinectBodyHelper.ForearmRatioForDeadzone;
            }
            set
            {
                KinectBodyHelper.ForearmRatioForDeadzone = value;
            }
        }

        /// <summary>
        /// Rect value worked out by pointing to left, top, right, bottom spot with my hand as the ideal edges, and noting down the x, y values got from GetHandRelativePosition.
        /// This may only fit more for me, and you can test out your rect value. Approximately it works fine for most people.
        /// </summary>
        private MRect gestureRect = new MRect(-0.18, 1.65, 0.18, -1.65);

        private bool[] handGrips = new bool[2] { false, false };

        private double _hoverDuration = 2;

        private uint[] mouseButtonPressed = new uint[2] { (uint)MouseControl.MouseEventFlag.LeftDown, (uint)MouseControl.MouseEventFlag.LeftDown };

        /// <summary>
        /// Wait time for a hover gesture.
        /// </summary>
        public double HoverDuration {
            get
            {
                return _hoverDuration;
            }
            set
            {
                _hoverDuration = value;
                hoverTimer.Interval = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Hand moves further than distance will cause a hover fail.
        /// Default as 20, this needs to be modified according to usual movement distance/speed in your specific case.
        /// </summary>
        public double HoverRange { get; set; } = 20;
        
        public double MoveScale
        {
            get
            {
                return cursorMapper.MoveScale;
            }
            set
            {
                cursorMapper.MoveScale = value;
            }
        }

        public double HandLiftYForClick { get; set; } = 0.02f;

        private MVector2 lastCursorPos = MVector2.Zero;

        /// <summary>
        /// Timer for hover detection.
        /// </summary>
        private DispatcherTimer hoverTimer = new DispatcherTimer();

        private const int NONE_USED = -1;

        /// <summary>
        /// Used to keep track of the controlling hand. So when another hand lift up forward, the cursor would still follow the first controlling hand.
        /// </summary>
        private int usedHandIndex = NONE_USED;
        private bool hoverClicked = false;

        public KinectCursor()
        {
            MRect screenRect = new MRect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            cursorMapper = new CursorMapper(gestureRect, screenRect, CursorMapper.ScaleAlignment.LongerRange);

            sensorReader = new KinectReader(false);
            sensorReader.OnTrackedBody += Kinect_OnTrackedBody;
            sensorReader.OnLostTracking += Kinect_OnLostTracking;
            hoverTimer.Interval = TimeSpan.FromSeconds(HoverDuration);
            hoverTimer.Tick += new EventHandler(HoverTimer_Tick);


        }

        private void Kinect_OnLostTracking(object sender, EventArgs e)
        {
            ToggleHoverTimer(false);
            ReleaseGrip(0);
            ReleaseGrip(1);
            usedHandIndex = NONE_USED;
        }

        private void Kinect_OnTrackedBody(object sender, BodyEventArgs e)
        {
            Body body = e.BodyData;

            if (Mode == ControlMode.Disabled)
            {
                return;
            }
            if (Enable == ControlMode.Disabled)
            {
                return;
            }
            if (body.IsStopGesture())
            {
                Enable = ControlMode.Disabled;
            }

            for (int i = 1; i >= 0; i--) // Starts looking from right hand.
            {
                bool isLeft = (i == 0);
                if (body.IsWristOutsideDeadzone(isLeft))
                {
                    if (usedHandIndex == -1)
                    {
                        usedHandIndex = i;
                    }
                    else if (usedHandIndex != i)
                    {
                        // In two-hand control mode, non-used hand would be used for pressing/releasing mouse button.
                        if (Mode == ControlMode.MoveGripPressing)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft));
                        }

                        continue;
                    }

                    if ((Mode == ControlMode.LeftOrRightMouseDownAndUp_WristPosition) ||
                        (Mode == ControlMode.LeftOrRightMouseDownAndUp_HandTipPosition))
                    {
                        MVector2 inputRect = body.GetWristRelativeRect(isLeft); // calculate XY distance boundary of elbow to wrist
                        MRect InputRect = new MRect(-inputRect.X, -inputRect.Y, inputRect.X, inputRect.Y); // convert to MVector2 to MRect, only map wrist above elbow in Y axis to accomodate seated position
                        cursorMapper.InputRect = InputRect; // set cursor XY input boundaries of active hand
                        Console.WriteLine("inputRect=[ {0:F}, {1:F} ]", inputRect.X, inputRect.Y);
                    }

                    MVector2 targetPos;
                    if (Mode == ControlMode.LeftOrRightMouseDownAndUp_HandTipPosition)
                    {
                        MVector2 handTipPos = body.GetHandTipRelativePosition(isLeft); // get XY plane vector from shoulder to hand tip                       handTipPos.Y = -handTipPos.Y; // invert Y coordinate to map from Kinect to screen coordinates
                        handTipPos.Y = -handTipPos.Y; // invert Y coordinate to map from Kinect to screen coordinates
                        targetPos = cursorMapper.GetSmoothedOutputPosition(handTipPos); // map handtip position in XY plane to screen coordinates
                                                                                        //System.Diagnostics.Trace.WriteLine(wristPos.ToString());
                    }
                    else
                    {
                        MVector2 wristPos = body.GetWristRelativePosition(isLeft); // get XY plane vector from shoulder to wrist
                        wristPos.Y = -wristPos.Y; // invert Y coordinate to map from Kinect to screen coordinates
                        targetPos = cursorMapper.GetSmoothedOutputPosition(wristPos); // map wrist position in XY plane to screen coordinates
                                                                                      //System.Diagnostics.Trace.WriteLine(wristPos.ToString());
                    }
                    Console.WriteLine("TargetPos=[ {0:F}, {1:F} ]", targetPos.X, targetPos.Y);
                    MouseControl.MoveTo(targetPos.X, targetPos.Y);

                    if (Mode == ControlMode.GripToPress)
                    {
                        DoMouseControlByHandState(i, body.GetHandState(isLeft));
                    }
                    else if (Mode == ControlMode.HoverToClick)
                    {
                        if ((targetPos - lastCursorPos).Length() > HoverRange)
                        {
                            ToggleHoverTimer(false);
                            hoverClicked = false;
                        }

                        lastCursorPos = targetPos;
                    }
                    else if (Mode == ControlMode.LeftOrRightMouseDownAndUp_WristPosition)
                    {
                        bool isClick = body.IsThumbClick(isLeft);
                        CameraSpacePoint thumbRelativePosition = body.GetThumbRelativePosition(isLeft);
                        double XYplane_tangent = Math.Abs(thumbRelativePosition.Y) / Math.Abs(thumbRelativePosition.X);
                        if (XYplane_tangent <= tan30degrees)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.LeftDown, isClick); // thumb is horizontal, consider as finger over left mouse button
                        }
                        else if (XYplane_tangent >= tan60degrees)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.RightDown, isClick); // thumb is vertical, consider as finger over right mouse button
                        }
                        else
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.MiddleDown, isClick); // thumb is diagnonal, consider as finger over middle mouse button
                        }
                    }
                    else if (Mode == ControlMode.LeftOrRightMouseDownAndUp_HandTipPosition)
                    {
                        bool isClick = body.IsThumbClick(isLeft);
                        CameraSpacePoint thumbRelativePosition = body.GetThumbRelativePosition(isLeft);
                        double XYplane_tangent = Math.Abs(thumbRelativePosition.Y) / Math.Abs(thumbRelativePosition.X);
                        if (XYplane_tangent <= tan30degrees)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.LeftDown, isClick); // thumb is horizontal, consider as finger over left mouse button
                        }
                        else if (XYplane_tangent >= tan60degrees)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.RightDown, isClick); // thumb is vertical, consider as finger over right mouse button
                        }
                        else
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft), (uint)MouseControl.MouseEventFlag.MiddleDown, isClick); // thumb is diagnonal, consider as finger over middle mouse button
                        }
                    }
                }
                else
                {
                    if (usedHandIndex == i)
                    {
                        // Reset to none.
                        usedHandIndex = NONE_USED;
                        ReleaseGrip(i);
                    }
                    else if (Mode == ControlMode.MoveLiftClicking)
                    {
                        DoMouseClickByHandLifting(i, body.GetWristRelativePosition(isLeft));
                        //System.Diagnostics.Trace.WriteLine(body.GetHandRelativePosition(isLeft).Y);
                    }
                    else // Release mouse button when it's not regularly released, such as hand tracking lost.
                    {
                        ReleaseGrip(i);
                    }
                }
            }

            ToggleHoverTimer(Mode == ControlMode.HoverToClick && usedHandIndex != -1);
        }

        private void DoMouseControlByHandState(int handIndex, HandState handState, uint mouseEventFlag = (uint)MouseControl.MouseEventFlag.LeftDown, bool isClick = false)
        {
            MouseControlState controlState;
            switch (handState)
            {
                case HandState.Closed:
                    controlState = MouseControlState.ShouldPress;
                    break;
                case HandState.Open:
                    controlState = MouseControlState.ShouldRelease;
                    break;
                default:
                    controlState = MouseControlState.None;
                    break;
            }
            UpdateHandMouseControl(handIndex, controlState, mouseEventFlag, isClick);
        }

        private void DoMouseClickByHandLifting(int handIndex, in MVector2 handRelativePos)
        {
            UpdateHandMouseControl(handIndex, handRelativePos.Y > HandLiftYForClick ? MouseControlState.ShouldClick : MouseControlState.ShouldRelease);
            
            //DoMouseControlByHandLifting(with press and releas rather than just a click):
            //MouseControlState controlState = handRelativePos.Y > HandLiftYForClick ? MouseControlState.ShouldPress : MouseControlState.ShouldRelease;
            //UpdateHandMouseControl(handIndex, controlState);
        }

        private enum MouseControlState
        {
            None,
            ShouldPress,
            ShouldRelease,
            ShouldClick,
        }

       
        private void UpdateHandMouseControl(int handIndex, MouseControlState controlState, uint mouseEventFlag = (uint)MouseControl.MouseEventFlag.LeftDown, bool isClick = false)
        {
            if ((Mode == ControlMode.LeftOrRightMouseDownAndUp_WristPosition) ||
                (Mode == ControlMode.LeftOrRightMouseDownAndUp_HandTipPosition))
            {
                if ((mouseEventFlag == (uint)MouseControl.MouseEventFlag.LeftDown) ||
                    (mouseEventFlag == (uint)MouseControl.MouseEventFlag.RightDown) ||
                    (mouseEventFlag == (uint)MouseControl.MouseEventFlag.MiddleDown))
                {
                    if (controlState == MouseControlState.ShouldPress)
                    {
                        if (!handGrips[handIndex])
                        {
                            if (isClick)
                                MouseControl.Click((MouseControl.MouseEventFlag)mouseEventFlag);
                            else
                                MouseControl.PressDown((MouseControl.MouseEventFlag)mouseEventFlag);
                            mouseButtonPressed[handIndex] = mouseEventFlag;
                            handGrips[handIndex] = true;
                        }
                    }
                    else if (controlState == MouseControlState.ShouldRelease && handGrips[handIndex])
                    {
                        ReleaseGrip(handIndex);
                        handGrips[handIndex] = false;
                    }
                }
            }
            else if (controlState == MouseControlState.ShouldClick)
            {
                if (!handGrips[handIndex])
                {
                    MouseControl.Click();
                    handGrips[handIndex] = true;
                }
            }else if (controlState == MouseControlState.ShouldPress)
            {
                if (!handGrips[handIndex])
                {
                    MouseControl.PressDown();
                    handGrips[handIndex] = true;
                }
            }
            else if (controlState == MouseControlState.ShouldRelease && handGrips[handIndex])
            {
                ReleaseGrip(handIndex);
                handGrips[handIndex] = false;
            }
        }

        private void ReleaseGrip(int handIndex)
        {
            if ((Mode == ControlMode.LeftOrRightMouseDownAndUp_WristPosition) ||
                (Mode == ControlMode.LeftOrRightMouseDownAndUp_HandTipPosition))

            {
                /* check if mouse button was pressed */
               if (handGrips[handIndex])
               {
                    MouseControl.Click((MouseControl.MouseEventFlag)mouseButtonPressed[handIndex]);
                    MouseControl.PressUp((MouseControl.MouseEventFlag)mouseButtonPressed[handIndex]);
               }
            }
            else if (handGrips[handIndex])
            {
                MouseControl.PressUp();
            }
        }

        private void ToggleHoverTimer(bool isOn)
        {
            if(hoverTimer.IsEnabled != isOn)
            {
                if (isOn)
                {
                    hoverTimer.Start();
                }
                else
                {
                    hoverTimer.Stop();
                }
            }
        }

        void HoverTimer_Tick(object sender, EventArgs e)
        {
            if (!hoverClicked)
            {
                MouseControl.Click();
                hoverTimer.Stop();
                hoverClicked = true;
            }
        }

    }
    
}
