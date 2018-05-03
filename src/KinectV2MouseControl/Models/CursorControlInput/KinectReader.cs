using Microsoft.Kinect;
using System;
using System.Linq;

namespace KinectV2MouseControl
{
    /// <summary>
    /// Read Kinect sensor body data.
    /// </summary>
    public class KinectReader
    {
        public EventHandler<BodyEventArgs> OnTrackedBody;
        public EventHandler OnLostTracking;

        const int NO_LOST_FRAME_TRACK = -1;
        const int MAX_LOST_TRACKING_FRAME_ALLOWED = 5;

        /// <summary>
        /// Allowing some tracking lost frames before raising OnLostTracking events.
        /// So the tracking effected result won't get stuck for instant small frames loses, and will be seen more continuous.
        /// (Especially when there're more changes happen between tracking and losing tracking.)
        /// </summary>
        int lostTrackingFrames = NO_LOST_FRAME_TRACK;

        KinectSensor sensor;

        /// <summary>
        /// Reader for body frames.
        /// </summary>
        BodyFrameReader bodyFrameReader;

        /// <summary>
        /// Array for bodies data.
        /// </summary>
        Body[] bodies = null;

        ulong usedTrackingId = 0;

        public KinectReader(bool openSensor = false)
        {
            sensor = KinectSensor.GetDefault();
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            if (openSensor)
            {
                Open();
            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool refreshedBodyData = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(bodies);
                    refreshedBodyData = true;
                }
            }

            if (refreshedBodyData)
            {
                HandleBodyData();
            }
        }

        private void HandleBodyData()
        {
            /*
             *  Use the first tracked body data for cursor controlling, until it loses tracking.
             *  You can also make your own ways of selecting tracked person in this function.
             */

            bool hasTrackedBody = false;

            if (usedTrackingId != 0)
            {
                Body trackedBody = bodies.FirstOrDefault<Body>(body => body.TrackingId == usedTrackingId);
                if (trackedBody != null)
                {
                    GetTrackedBody(trackedBody);
                    hasTrackedBody = true;
                }
            }
            else
            {
                Body newBody = bodies.FirstOrDefault<Body>(body => body.IsTracked);
                if (newBody != null)
                {
                    GetTrackedBody(newBody);
                    usedTrackingId = newBody.TrackingId;
                    hasTrackedBody = true;
                }
            }


            if (!hasTrackedBody && lostTrackingFrames != NO_LOST_FRAME_TRACK && ++lostTrackingFrames > MAX_LOST_TRACKING_FRAME_ALLOWED)
            {
                lostTrackingFrames = NO_LOST_FRAME_TRACK;
                usedTrackingId = 0;
                if (OnLostTracking != null)
                {
                    OnLostTracking.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void GetTrackedBody(Body body)
        {
            lostTrackingFrames = 0;
            if (OnTrackedBody != null)
            {
                OnTrackedBody.Invoke(this, new BodyEventArgs(body));
            }
        }

        /// <summary>
        /// Open sensor.
        /// </summary>
        public void Open()
        {
            if (sensor != null && !sensor.IsOpen)
            {
                sensor.Open();
            }
        }

        /// <summary>
        /// Close sensor.
        /// </summary>
        public void Close()
        {
            if (sensor != null && sensor.IsOpen)
            {
                sensor.Close();
            }
        }
    }

    public class BodyEventArgs : EventArgs
    {
        public Body BodyData { get; private set; }

        public BodyEventArgs(Body bodyData)
        {
            BodyData = bodyData;
        }
    }

}
