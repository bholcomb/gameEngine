using System;

using OpenTK;

namespace Util
{
    public struct Vector3i
    {
       public Vector3i(Vector3i orig) : this(orig.X, orig.Y, orig.Z) { }

        public Vector3i(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public int X;
        public int Y;
        public int Z;
        public override bool Equals(object obj)
        {
            if (obj is Vector3i)
            {
                Vector3i other = (Vector3i)obj;
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            unchecked
            {
                hash = hash * 37 + X;
                hash = hash * 37 + Y;
                hash = hash * 37 + Z;
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", X, Y, Z);
        }

        //implicit conversion to OpenTK Vector3
        public static implicit operator Vector3(Vector3i orig)
        {
           return new Vector3(orig.X, orig.Y, orig.Z);
        }

        //implicit conversion from OpenTK Vector3
        public static implicit operator Vector3i(Vector3 d)
        {
           return new Vector3i(Convert.ToInt32(d.X), Convert.ToInt32(d.Y), Convert.ToInt32(d.Z));
        }
    }
}