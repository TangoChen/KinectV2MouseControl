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
                if ((value >= 1.0) && (value <= 3.0))
                    _ForearmRatioForDeadzone = value;
                else
                    _ForearmRatioForDeadzone = DEFAULT_FOREARM_RATIO_FOR_DEADZONE;
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
            double distanceSquared = relativePosition.X * relativePosition.X +
                                     relativePosition.Y * relativePosition.Y +
                                     relativePosition.Z * relativePosition.Z;

            double distance = Math.Sqrt(distanceSquared);
            return distance;
        }

        public static bool IsWristOutsideDeadzone(this Body body, bool isLeft)
        {
            /* calculate length of forearm */
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint elbowBase = body.Joints[isLeft ? JointType.ElbowLeft : JointType.ElbowRight].Position;
            double forearmLength = GetDistance(elbowBase, wristPos);
            double deadzoneDistance = forearmLength * _ForearmRatioForDeadzone;
            if (deadzoneDistance == 0.0)
                return false;

            CameraSpacePoint spineShoulderPos = body.Joints[JointType.SpineShoulder].Position;
            CameraSpacePoint relativePosition = GetRelativePosition(spineShoulderPos, wristPos);
            if (relativePosition.Z >  -deadzoneDistance)
                return false;

            return true;
        }

        public static HandState GetHandState(this Body body, bool isLeft)
        {
            /* return HandState of active hand */
            return isLeft ? body.HandLeftState : body.HandRightState;
        }

        public static MVector2 GetWristRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active elbow to active hand in XY plane */ 
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint shoulderBase = body.Joints[isLeft ? JointType.ShoulderLeft : JointType.ShoulderRight].Position;

            return wristPos.ToMVector2() - shoulderBase.ToMVector2();
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
  
            /* otherwise diagonal */
            return true;
        }

        public static bool isVertical(in CameraSpacePoint vectorParam)
        {
            if ((Math.Abs(vectorParam.Y) > tan60degrees * Math.Abs(vectorParam.X)) &&
                (Math.Abs(vectorParam.Y) > tan60degrees * Math.Abs(vectorParam.Z)))
                return true;

            return false;
        }

        public static bool IsStopGesture(this Body body)
        {
            /* crossed arms in front */

            /* check if wrists are more than 10cm apart */
            CameraSpacePoint wristLeftPos = body.Joints[JointType.WristLeft].Position;
            CameraSpacePoint wristRightPos = body.Joints[JointType.WristRight].Position;
            double wristToWristDistance = GetDistance(wristLeftPos, wristRightPos);
            if (wristToWristDistance > 0.10)
                return false;

            /* check if hands are not open */
            HandState handLeftState = body.HandLeftState;
            HandState handRightState = body.HandRightState;
            if ((handLeftState != HandState.Open) ||
                (handRightState != HandState.Open))
                return false;
            
            /*  check if left forearm is not diagonal */
            CameraSpacePoint elbowLeftBase = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint leftForearmVector = GetRelativePosition(elbowLeftBase, wristLeftPos);
            if (!isXYplaneDiagonal(leftForearmVector))
                return false;
            /* check if left forearm is not vertical */
            if (!isVertical(leftForearmVector))
                return false;

            /* check if right forearm is not diagonal */
            CameraSpacePoint elbowRightBase = body.Joints[JointType.ElbowRight].Position;
            CameraSpacePoint rightForearmVector = GetRelativePosition(elbowRightBase, wristRightPos);
            if (!isXYplaneDiagonal(rightForearmVector))
                return false;
            /* check if right forearm is not vertical */
            if (!isVertical(rightForearmVector))
                return false;

            return true;
        }

        public static MVector2 GetWristRelativeRect(this Body body, bool isLeft)
        {
            /* obtain forearm length */
            CameraSpacePoint wristPos = body.Joints[isLeft ? JointType.WristLeft : JointType.WristRight].Position;
            CameraSpacePoint elbowBase = body.Joints[isLeft ? JointType.ElbowLeft : JointType.ElbowRight].Position;
            double forearmLength = GetDistance(elbowBase, wristPos);

            /* return 0.25 forearm length for input rectangle boundaries */
            MVector2 wristRelativeRect = new MVector2(forearmLength * 0.25, forearmLength * 0.25 );
            return wristRelativeRect;
        }

        public static CameraSpacePoint GetHandTipRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active hand to active handtip in relative coordinates XYZ */
            CameraSpacePoint handTipPos = body.Joints[isLeft ? JointType.HandTipLeft : JointType.HandTipRight].Position;
            CameraSpacePoint handPos = body.Joints[isLeft ? JointType.HandLeft : JointType.HandRight].Position;

            CameraSpacePoint handTipRelativePosition = GetRelativePosition(handPos, handTipPos);
            return handTipRelativePosition;
        }

        public static CameraSpacePoint GetThumbRelativePosition(this Body body, bool isLeft)
        {
            /* return vector from active hand to active thumb in relative coordinates XYZ */
            CameraSpacePoint thumbPos = body.Joints[isLeft ? JointType.ThumbLeft : JointType.ThumbRight].Position;
            CameraSpacePoint handPos = body.Joints[isLeft ? JointType.HandLeft : JointType.HandRight].Position;

            CameraSpacePoint thumbRelativePosition = GetRelativePosition(handPos, thumbPos);
            return thumbRelativePosition;
        }

        public static MVector2 ToMVector2(this CameraSpacePoint jointPoint)
        {
            /* Return vector in XY plane */
            return new MVector2(jointPoint.X, jointPoint.Y);
        }

    }
}
