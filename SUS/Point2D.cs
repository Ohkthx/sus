using System;

namespace SUS
{
    public struct Point2D : IPoint2D
    {
        private int m_X;
        private int m_Y;

        public Point2D(int x, int y, bool invalid = false)
        {
            if (!invalid)
            {
                if (x < 0) x = 0;

                if (y < 0) y = 0;
            }

            m_X = x;
            m_Y = y;
        }

        #region Getters / Setters

        public int X
        {
            get => m_X;
            set
            {
                if (value < 0 || value == X) return;

                m_X = value;
            }
        }

        public int Y
        {
            get => m_Y;
            set
            {
                if (value < 0 || value == Y) return;

                m_Y = value;
            }
        }

        public bool IsValid => X >= 0 && Y >= 0;

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
                var hash = 13;
                hash = hash * 7 + X.GetHashCode();
                hash = hash * 7 + Y.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Point2D c1, Point2D c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Point2D c1, Point2D c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;

            return value.GetType() == GetType() && IsEqual((Point2D) value);
        }

        private bool Equals(Point2D mobile)
        {
            return IsEqual(mobile);
        }

        private bool IsEqual(Point2D value)
        {
            return X == value.X
                   && Y == value.Y;
        }

        #endregion

        public void Invalidate()
        {
            m_X = -1;
            m_Y = -1;
        }

        public static int Distance(IEntity from, IEntity to)
        {
            return Distance(from.Location, to.Location);
        }

        private static int Distance(IPoint2D from, IPoint2D to)
        {
            return (int) Math.Sqrt(
                Math.Pow(from.X - to.X, 2)
                + Math.Pow(from.Y - to.Y, 2)
            );
        }

        public static IPoint2D Midpoint(IEntity from, IEntity to)
        {
            return Midpoint(from.Location, to.Location);
        }

        private static IPoint2D Midpoint(IPoint2D from, IPoint2D to)
        {
            return new Point2D((from.X + to.X) / 2, (from.X + to.Y) / 2);
        }

        public static Point2D MoveTowards(IEntity from, IEntity to, int speed)
        {
            return MoveTowards(from.Location, to.Location, speed);
        }

        private static Point2D MoveTowards(IPoint2D from, IPoint2D to, int speed)
        {
            // Bresenham's line algorithm; provided by: Frank Lioty @StackOverflow
            var w = to.X - from.X;
            var h = to.Y - from.Y;
            var newX = from.X;
            var newY = from.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0)
                dx1 = -1;
            else if (w > 0) dx1 = 1;

            if (h < 0)
                dy1 = -1;
            else if (h > 0) dy1 = 1;

            if (w < 0)
                dx2 = -1;
            else if (w > 0) dx2 = 1;

            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0)
                    dy2 = -1;
                else if (h > 0) dy2 = 1;

                dx2 = 0;
            }

            var numerator = longest >> 1;
            for (var i = 0; i < speed; i++)
            {
                if (i == longest) break;

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