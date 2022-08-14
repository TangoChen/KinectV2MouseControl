using System;

namespace KinectV2MouseControl
{
    public struct MRect
    {
        public double Left { get; private set; }
        public double Top { get; private set; }
        public double Right { get; private set; }
        public double Bottom { get; private set; }

        public double DeltaX { get; private set; }
        public double DeltaY { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public MVector2 Center
        {
            get; private set;
        }

        public MRect(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;

            DeltaX = right - left;
            DeltaY = bottom - top;
            Width = Math.Abs(DeltaX);
            Height = Math.Abs(DeltaY);
            Center = new MVector2(Left + DeltaX * 0.5f, Top + DeltaY * 0.5f);
        }

        public override string ToString()
        {
            return Left.ToString() + ", " + Top.ToString() + ", " + Right.ToString() + ", " + Bottom.ToString();
        }
    }

    
    public struct MVector2
    {
        public double X, Y;
        public MVector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString();
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }


        public override bool Equals(object obj)
        {
            if (obj is MVector2)
            {
                return Equals((MVector2)this);
            }

            return false;
        }

        public bool Equals(in MVector2 other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        private static MVector2 zero = new MVector2(0, 0);
        public static MVector2 Zero
        {
            get
            {
                return zero;
            }
        }

        private static MVector2 one = new MVector2(1, 1);
        public static MVector2 One
        {
            get
            {
                return one;
            }
        }

        public static bool operator ==(in MVector2 value1, in MVector2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        public static bool operator !=(in MVector2 value1, in MVector2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }

        public static MVector2 operator -(MVector2 value1, in MVector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static MVector2 operator +(MVector2 value1, in MVector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }

        public static MVector2 operator *(MVector2 value1, in MVector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static MVector2 operator *(MVector2 value, double scaleFactor)
        {
            value.Y *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static MVector2 operator *(double scaleFactor, MVector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        public static MVector2 operator /(MVector2 value1, in MVector2 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            return value1;
        }

        public static MVector2 operator /(MVector2 value, double divider)
        {
            double factor = 1 / divider;
            value.X *= factor;
            value.Y *= factor;
            return value;
        }

    }

}
