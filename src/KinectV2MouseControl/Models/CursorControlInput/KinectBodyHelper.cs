using Microsoft.Kinect;
using System;

namespace KinectV2MouseControl
{
    public static class KinectBodyHelper
    {
        // public static double PI = 3.141592653589793;
        // public static double degrees30toRads = 0.523598775598299; // PI * 30.0 / 180.0;
        // public static double degrees60toRads = 1.047197551196598; // PI * 60.0 / 180.0;
        public static double tan30degrees = 0.57735026919; // = Math.Tan(degrees30toRads);
        public static double tan60degrees = 1.73205080757; // = Math.Tan(degrees60toRads);

        const double DEFAULT_FOREARM_RATIO_FOR_DEADZONE = 2.1;
        public static double _ForearmRatioForDeadzone = DEFAULT_FOREARM_RATIO_FOR_DEADZONE;
        public static double ForearmRatioForDeadzone
        {
            get
            {
                return _ForearmRatioForDeadzone;
            }
            set
            {
                if (value <= 1.0) 
                    _ForearmRatioForDeadzone = 1.0;
                else if (value >= 2.5)
                    _ForearmRatioForDeadzone = 2.5;
                else
                    _ForearmRatioForDeadzone = value;
            }
        }

        public static CameraSpacePoint GetRelativePosition(in CameraSpacePoint fromPos, in CameraSpacePoint toPos)
        {
            CameraSpacePoint relativePosition;
            relativePosition.X = toPos.X - fromPos.X;
            relativePosition.Y = toPos.Y - fromPos.Y;
            relativePosition.Z = toPos.Z - fromPos.Z;
            return relativePosition;
        }

        public static double GetDistance(in CameraSpacePoint fromPos, in CameraSpacePoint toPos)
        {
            CameraSpacePoint relativePosition = GetRelativePosition(fromPos, toPos);
            double distanceSquared = (relativePosition.X * relativePosition.X) +
                                     (relativePosition.Y * relativePosition.Y) +
                                     (relativePosition.Z * relativePosition.Z);

            double distance = 0.0;
            if (distanceSquared != 0.0)
                distance = Math.Sqrt(distanceSquared);
            return distance;
        }

        public static double GetDistanceBetweenHandTipAndThumb(this Body body, bool isLeft)
        {
            CameraSpacePoint handTipPos = body.Joints[isLeft ? JointType.HandTipLeft : JointType.HandTipRight].Position;
            CameraSpacePoint thumbPos = body.Joints[isLeft ? JointType.ThumbLeft : JointType.ThumbRight].Position;
            double distance = GetDistance(handTipPos, thumbPos);
            return distance;
        }

        public static bool IsThumbClick(this Body body, bool isLeft)
        {
            double distance = GetDistanceBetweenHandTipAndThumb(body, isLeft);
            if (distance <= 0.005)
                return true;
            return false;
        }

        public static bool IsWristOutsideDeadzone(this Body body, bool isLeft)
        {
            /* calculate length of forearm */
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint elbowBase = body.Joints[isLeft ? JointType.ElbowLeft : JointType.ElbowRight].Position;
            double forearmLength = GetDistance(elbowBase, wristPos);
            //Console.WriteLine("IsWristOutsideDeadzone: wrist=[ {0:F}, {1:F}, {2:F} ] elbow=[ {3:F}, {4:F}, {5:F} ] forearm={6:F}",
            //    wristPos.X, wristPos.Y, wristPos.Z, elbowBase.X, elbowBase.Y, elbowBase.Z, forearmLength);
            double deadzoneDistance = forearmLength * _ForearmRatioForDeadzone;
            if (deadzoneDistance == 0.0)
                return false;

            CameraSpacePoint spineShoulderPos = body.Joints[JointType.SpineShoulder].Position;
            CameraSpacePoint relativePosition = GetRelativePosition(spineShoulderPos, wristPos);
            //Console.WriteLine("IsWristOutsideDeadzone: wrist-spinebase=[ {0:F}, {1:F}, {2:F} ] ",
            //    relativePosition.X, relativePosition.Y, relativePosition.Z);
            if (relativePosition.Z >  -deadzoneDistance)
                return false;

            //Console.WriteLine("IsWristOutsideDeadzone: true\n");
            return true;
        }

        public static HandState GetHandState(this Body body, bool isLeft)
        {
            /* return HandState of active hand */
            return isLeft ? body.HandLeftState : body.HandRightState;
        }

        public static MVector2 GetWristRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active shoulder to active hand in XY plane */ 
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint shoulderBase = body.Joints[isLeft ? JointType.ShoulderLeft : JointType.ShoulderRight].Position;

            return wristPos.ToMVector2() - shoulderBase.ToMVector2();
        }

        public static MVector2 GetHandTipRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active shoulder to active handtip in XY plane */
            CameraSpacePoint handTipPos = body.Joints[isLeft ? JointType.HandTipLeft : JointType.HandTipRight].Position;
            CameraSpacePoint shoulderBase = body.Joints[isLeft ? JointType.ShoulderLeft : JointType.ShoulderRight].Position;

            return handTipPos.ToMVector2() - shoulderBase.ToMVector2();
        }


        public static bool isXYplaneDiagonal(in CameraSpacePoint vectorParam)
        {
            if ((vectorParam.X == 0.0) || (vectorParam.Y == 0.0))
                return false;

            /* check if horizontal */
            if (Math.Abs(vectorParam.Y) <= tan30degrees * Math.Abs(vectorParam.X))
                return false;
            
            /* check if vertical */
            if (Math.Abs(vectorParam.Y) >= tan60degrees * Math.Abs(vectorParam.X))
                return false;

            //Console.WriteLine("isXYplaneDiagonal: true");
            /* otherwise diagonal */
            return true;
        }

        const double IS_VERTICAL_TOLERANCE = 0.9;
        public static bool isVertical(in CameraSpacePoint vectorParam)
        {
            /* check if vertical */
            if (Math.Abs(vectorParam.Y) < Math.Abs(vectorParam.X) * IS_VERTICAL_TOLERANCE)
                return false;
            if (Math.Abs(vectorParam.Y) < Math.Abs(vectorParam.Z) * IS_VERTICAL_TOLERANCE)
                return false;

            //Console.WriteLine("isVertical: true");
            return true;
        }

        public static bool IsStopGesture(this Body body)
        {
            /* IsStopGesture where arms are diagonaaly crossed in front at wrists with open hands */

            /* check if wrists are more than 10cm apart */
            CameraSpacePoint wristLeftPos = body.Joints[JointType.WristLeft].Position;
            CameraSpacePoint wristRightPos = body.Joints[JointType.WristRight].Position;
            double wristToWristDistance = GetDistance(wristLeftPos, wristRightPos);
            if (wristToWristDistance > 0.10)
                return false;
            //Console.WriteLine("IsStopGesture: wristToWristDistance <= 0.10");

            /* check if hands are not open */
            HandState handLeftState = body.HandLeftState;
            HandState handRightState = body.HandRightState;
            Console.WriteLine("IsStopGesture: handLeftState=={0:D} handRightState=={1:D}", handLeftState, handRightState);
            if (!((handLeftState == HandState.NotTracked) && (handRightState == HandState.NotTracked)) ||
                 ((handLeftState == HandState.Open) && (handRightState == HandState.Open)))
                return false;
            //Console.WriteLine("IsStopGesture: handLeftState==HandState.Open && handRightState==HandState.Open");

            /*  check if left forearm is not diagonal */
            CameraSpacePoint elbowLeftBase = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint leftForearmVector = GetRelativePosition(elbowLeftBase, wristLeftPos);
            //Console.WriteLine("IsStopGesture: leftForearmVector=[ {0:F}, {1:F}, {2:F} ] ",
            //    leftForearmVector.X, leftForearmVector.Y, leftForearmVector.Z);
            if (!isXYplaneDiagonal(leftForearmVector))
                return false;
            //Console.WriteLine("IsStopGesture: leftForearmVector is diagonal");
            if (!isVertical(leftForearmVector))
                return false;
            //Console.WriteLine("IsStopGesture: leftForearmVector is vertical");

            /* check if right forearm is not diagonal */
            CameraSpacePoint elbowRightBase = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint rightForearmVector = GetRelativePosition(elbowRightBase, wristRightPos);
            //Console.WriteLine("IsStopGesture: rightForearmVector=[ {0:F}, {1:F}, {2:F} ] ",
            //    rightForearmVector.X, rightForearmVector.Y, rightForearmVector.Z);
            if (!isXYplaneDiagonal(rightForearmVector))
                return false;
            //Console.WriteLine("IsStopGesture: rightForearmVector is diagonal");
            if (!isVertical(rightForearmVector))
                return false;
            //Console.WriteLine("IsStopGesture: leftForearmVector is vertical");

            //Console.WriteLine("IsStopGesture: true");
            return true;
        }

        public static MVector2 GetWristRelativeRect(this Body body, bool isLeft)
        {
            /* obtain forearm length */
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint elbowBase = body.Joints[isLeft ? JointType.ElbowLeft : JointType.ElbowRight].Position;
            double forearmLength = GetDistance(elbowBase, wristPos);
            //Console.WriteLine("GetWristRelativeRect: wrist=[ {0:F}, {1:F}, {2:F} ] elbow=[ {3:F}, {4:F}, {5:F} ] forearm={6:F}",
            //    wristPos.X, wristPos.Y, wristPos.Z, elbowBase.X, elbowBase.Y, elbowBase.Z, forearmLength);

            /* return 0.25 forearm length for input rectangle boundaries */
            MVector2 wristRelativeRect = new MVector2(forearmLength * 0.25, forearmLength * 0.25 );
            //Console.WriteLine("GetWristRelativeRect: wristRelativeRect=[ {0:F}, {1:F}, {2:F} ]",
            //    wristRelativeRect.X, wristRelativeRect.Y, wristRelativeRect.Z,);
            return wristRelativeRect;
        }

        public static CameraSpacePoint GetThumbRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active hand to active thumb in relative coordinates XYZ */
            CameraSpacePoint thumbPos = body.Joints[isLeft ? JointType.ThumbLeft : JointType.ThumbRight].Position;
            CameraSpacePoint handPos = body.Joints[isLeft ? JointType.HandLeft : JointType.HandRight].Position;

            CameraSpacePoint thumbRelativePosition = GetRelativePosition(handPos, thumbPos);
            //Console.WriteLine("GetThumbRelativePosition: thumbRelativePosition=[ {0:F}, {1:F}, {2:F} ]",
            //    thumbRelativePosition.X, thumbRelativePosition.Y, thumbRelativePosition.Z,);
            return thumbRelativePosition;
        }

        public static MVector2 ToMVector2(this CameraSpacePoint jointPoint)
        {
            /* Return vector in XY plane */
            return new MVector2(jointPoint.X, jointPoint.Y);
        }

    }
}
