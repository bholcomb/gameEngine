using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

namespace Util
{
   public class Rect
   {
      protected float myLeft;
      protected float myBottom;
      protected float myRight;
      protected float myTop;

      public Rect() : this(20.0f) { }
      public Rect(float square) : this(square, square) { }
      public Rect(float width, float height) : this(0.0f, 0.0f, width, height) { }
      public Rect(Rect r) : this(r.left, r.bottom, r.right, r.top) { }

      public static Rect fromMinMax(Vector2 min, Vector2 max)
      {
         return new Rect(min, max);
      }

      public static Rect fromPosSize(Vector2 pos, Vector2 size)
      {
         return new Rect(pos, pos + size);
      }

      public Rect(float x1, float y1, float x2, float y2)
      {
         myLeft = x1;
         myBottom = y1;
         myRight = x2;
         myTop = y2;
         width = x2 - x1;
         height = y2 - y1;
      }

      public Rect(Vector2 a, Vector2 b)
      {
         myLeft = a.X;
         myBottom = a.Y;
         myRight = b.X;
         myTop = b.Y;
         width = myRight - myLeft;
         height = myTop - myBottom;
      }

      public Rect(Vector4 corners)
      {
         myLeft = corners.X;
         myBottom = corners.Y;
         myRight = corners.Z;
         myTop = corners.W;
         width = myRight - myLeft;
         height = myTop - myBottom;
      }

      public float left
      {
         get { return myLeft; }
         set { myLeft = value; }
      }

      public float right
      {
         get { return myRight; }
         set { myRight = value; }
      }

      public float top
      {
         get { return myTop; }
         set { myTop = value; }
      }

      public float bottom
      {
         get { return myBottom; }
         set { myBottom = value; }
      }

      public float width
      {
         get { return myRight - myLeft; }
         set { myRight = value + myLeft; }
      }

      public float height
      {
         get { return myTop - myBottom; }
         set { myTop =value + myBottom;}
      }

      public Vector2 SW
      {
         get { return new Vector2(myLeft, myBottom); }
         set { myLeft = value.X; ;myBottom = value.Y; }
      }

      public Vector2 SE
      {
         get { return new Vector2(right, myBottom); }
         set { right = value.X; ;myBottom = value.Y; }
      }

      public Vector2 NW
      {
         get { return new Vector2(myLeft, top); }
         set { myLeft = value.X; top = value.Y; }
      }

      public Vector2 NE
      {
         get { return new Vector2(right, top); }
         set { right = value.X; top = value.Y; }
      }

      public virtual Vector2 center
      {
         get {return new Vector2(myLeft + (width / 2.0f), myBottom + (height / 2.0f));}
         set {
            myLeft = value.X - width / 2.0f;
            myBottom = value.Y - height / 2.0f;
         }
      }

      public virtual Vector2 position
      {
         get { return new Vector2(myLeft, myBottom); }
         set { myLeft = value.X; myBottom = value.Y; }
      }

      public virtual Vector2 size
      {
         get { return new Vector2(width, height); }
         set { width = value.X; height = value.Y; }
      }

      public float area()
      {
         return width * height;
      }

      public virtual void setSize(float x, float y)
      {
         size = new Vector2(x, y);
      }

      public virtual void setPosition(float x, float y)
      {
         myLeft = x;
         myBottom = y;
      }

      public virtual void setPositionAbove(Rect r, float by)
      {
         myBottom = r.top + by;
         myLeft = r.left;
      }

      public virtual void setPositionUnder(Rect r, float by)
      {
         myBottom = r.bottom - height - by;
         myLeft = r.left;
      }

      public virtual void setPositionRightOf(Rect r, float by)
      {
         myBottom = r.bottom;
         myLeft = r.right + by;
      }

      public virtual void setPositionLeftOf(Rect r, float by)
      {
         myBottom = r.bottom;
         myLeft = r.left - width - by;
      }

      public virtual void move(float x, float y)
      {
         myLeft += x;
         myRight += x;
         myTop += y;
         myBottom += y;
      }

      public virtual void resizeLeftTo(float v)
      {
         myLeft = v;
      }

      public virtual void resizeTopTo(float v)
      {
         height = v - myBottom;
      }

      public virtual void resizeRightTo(float v)
      {
         width = v - myLeft;
      }

      public virtual void resizeBottomTo(float v)
      {
         myBottom = v;
      }

      public virtual void shrink(float amount)
      {
         grow(-amount);
      }

      public virtual void grow(float amount)
      {
         left -= amount/2.0f;
         bottom -= amount/2.0f;
         width += amount;
         height += amount;
      }

      public virtual void transpose()
      {
         float temp = width;
         width = height;
         height = temp;
      }

      public void insetFrom(Rect from, float inset)
      {
         set(from.left, from.bottom, from.width, from.height);
         shrink(inset);
      }

      public void set(float left, float bottom, float w, float h)
      {
         myLeft = left;
         myBottom = bottom;
         width = w;
         height = h;
      }

      public bool containsPoint(Vector2 pos)
      {
         return containsPoint(pos.X, pos.Y);
      }

      public bool containsPoint(float x, float y)
      {
         if (x >= myLeft && x <= right &&
            y >= myBottom && y <= top)
         {
            return true;
         }

         return false;
      }

      public bool isInXRange(float xmin, float xmax)
      {
         if (myLeft > xmin && myRight < xmax)
         {
            return true;
         }

         return false;
      }

      public bool isInYRange(float ymin, float ymax)
      {
         if (myBottom > ymin && myTop < ymax)
         {
            return true;
         }

         return false;
      }

      public bool intersects(Rect other)
      {
         //determined by if distances from centers of each rectangles is less than
         //combined size of rectangles on either axis.  Works only for axis aligned
         //rectangles (which these are)

         //calculate distance (Manhattan distance is faster and suites needs)
         float distance;
         distance = center.Length - (other.center.Length);


         //check vertical distance AND horizontal distance
         if (distance <= (myLeft + width) - (other.myLeft + other.width) &&
             distance <= (myBottom + height) - (other.myBottom + other.height))
         {
            return true;
         }

         return false;
      }
   }
}