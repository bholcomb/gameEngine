using System;

using OpenTK;

namespace Util
{
    public struct Vector2i
    {
       public Vector2i(Vector2i orig) : this(orig.X, orig.Y) { }

        public Vector2i(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public int X;
        public int Y;

        public override bool Equals(object obj)
        {
            if (obj is Vector2i)
            {
                Vector2i other = (Vector2i)obj;
                return this.X == other.X && this.Y == other.Y;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a.X == b.X && a.Y == b.Y);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            unchecked
            {
                hash = hash * 37 + X;
                hash = hash * 37 + Y;
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", X, Y);
        }

        //implicit conversion to OpenTK Vector2
        public static implicit operator Vector2(Vector2i orig)
        {
           return new Vector2(orig.X, orig.Y);
        }

        //implicit conversion from OpenTK Vector2
        public static implicit operator Vector2i(Vector2 d)
        {
           return new Vector2i(Convert.ToInt32(d.X), Convert.ToInt32(d.Y));
        }
    }
}