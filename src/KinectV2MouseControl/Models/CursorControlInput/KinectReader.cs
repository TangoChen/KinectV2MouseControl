using Microsoft.Kinect;
using System;

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
            //You can set up your own way to select tracked person in this function.
            bool hasTrackedBody = false;
            foreach (Body body in bodies)
            {
                // Skip untracked body.
                if (body.IsTracked)
                {
                    hasTrackedBody = true;
                    lostTrackingFrames = 0;
                    if (OnTrackedBody != null)
                    {
                        OnTrackedBody.Invoke(this, new BodyEventArgs(body));
                    }

                    //Use first tracked body in list only.
                    return;
                }
            }
            
            if (!hasTrackedBody && lostTrackingFrames != NO_LOST_FRAME_TRACK && ++lostTrackingFrames > MAX_LOST_TRACKING_FRAME_ALLOWED)
            {
                lostTrackingFrames = NO_LOST_FRAME_TRACK;
                if (OnLostTracking != null)
                {
                    OnLostTracking.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Open()
        {
            if (sensor != null && !sensor.IsOpen)
            {
                sensor.Open();
            }
        }

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
