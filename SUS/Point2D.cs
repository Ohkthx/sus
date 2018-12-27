using System;

namespace SUS
{
    public struct Point2D : IPoint2D
    {
        private int m_Xaxis;
        private int m_Yaxis;

        public Point2D(int x, int y, bool invalid = false)
        {
            if (!invalid)
            {
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
            }

            m_Xaxis = x;
            m_Yaxis = y;
        }

        #region Getters / Setters
        public int X
        {
            get { return m_Xaxis; }
            set
            {
                if (value < 0 || value == X)
                    return;

                m_Xaxis = value;
            }
        }

        public int Y
        {
            get { return m_Yaxis; }
            set
            {
                if (value < 0 || value == Y)
                    return;

                m_Yaxis = value;
            }
        }

        public bool IsValid { get { return X >= 0 && Y >= 0; } }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, X) ? X.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Y) ? Y.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Point2D c1, Point2D c2)
        {
            if (Object.ReferenceEquals(c1, c2)) return true;
            if (Object.ReferenceEquals(null, c1)) return false;
            return (c1.Equals(c2));
        }

        public static bool operator !=(Point2D c1, Point2D c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Point2D)value);
        }

        public bool Equals(Point2D mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(Point2D value)
        {
            return (value != null)
                && (X == value.X)
                && (Y == value.Y);
        }
        #endregion

        public void Invalidate() { m_Xaxis = -1; m_Yaxis = -1; }

        public static int Distance(IEntity from, IEntity to) { return Distance(from.Location, to.Location); }
        public static int Distance(IPoint2D from, IPoint2D to)
        {
            return (int)Math.Sqrt(
                    Math.Pow(from.X - to.X, 2)
                    + Math.Pow(from.Y - to.Y, 2)
                    );
        }

        public static IPoint2D Midpoint(IEntity from, IEntity to) { return Midpoint(from.Location, to.Location); }
        public static IPoint2D Midpoint(IPoint2D from, IPoint2D to)
        {
            return new Point2D((from.X + to.X) / 2, (from.X + to.Y) / 2);
        }

        public static Point2D MoveTowards(IEntity from, IEntity to, int speed) { return MoveTowards(from.Location, to.Location, speed); }
        public static Point2D MoveTowards(IPoint2D from, IPoint2D to, int speed)
        {   // Bresenham's line algorithm; provided by: Frank Lioty @StackOverflow
            int w = to.X - from.X;
            int h = to.Y - from.Y;
            int newX = from.X;
            int newY = from.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;
            for (int i = 0; i < speed; i++)
            {
                if (i == longest)
                    break;

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    newX += dx1;
                    newY += dy1;
                }
                else
                {
                    newX += dx2;
                    newY += dy2;
                }
            }

            return new Point2D(newX, newY);
        }
    }
}
