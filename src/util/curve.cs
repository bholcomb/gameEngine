using System;
using System.Collections.Generic;

using OpenTK;

namespace Util
{
	public class Curve
	{     
      List<Vector2> myPoints = new List<Vector2>();

      public Curve()
      {
         Vector2 p1 = new Vector2(0.0f, 0.0f);
         Vector2 p2 = new Vector2(1.0f, 1.0f);
         myPoints.Add(p1);
         myPoints.Add(p2);
      }

      public List<Vector2> points
      {
         get { return myPoints; }
      }

      public double value(double at)
      {
         if (at < 0.0) at= 0.0;
         if (at > 1.0) at= 1.0;

         int p2= 0;

         //find the point where it 
         while (at > myPoints[p2].X && p2 != myPoints.Count - 1)
         {
            p2++;
         }

         if (p2 == 0)
         {
            return 0.0;
         }


         //  percent     =    Distance past p1     /  total distance between p1 and p2
         double t = (double)((at - myPoints[p2-1].X) / (myPoints[p2].X - myPoints[p2-1].X));
         double val=lerp((double)myPoints[p2-1].Y, (double)myPoints[p2].Y, t);

         return val;
      }

      public int addPoint(Vector2 at)
      {
         int p1 = 0;
         int p2=myPoints.Count-1;
         while (at.X > myPoints[p1].X &&  p1!=p2)
         {
            p1++;
         }

         myPoints.Insert(p1, at);

         return p2;
      }

      public void setPoint(int point, Vector2 val)
      {
         myPoints[point] = val;
         myPoints.Sort((a,b) =>a.X.CompareTo(b.X) );
      }

      public void removePoint(int point)
      {
         myPoints.RemoveAt(point);
      }

      double lerp(double value1, double value2, double amt)
      {
         double val = (value1 * (1 - amt)) + (value2 * amt);
         return val;
      }

      public int hitPoint(Vector2 at)
      {
         for(int i=0; i< myPoints.Count; i++)
         {
            if ((myPoints[i] - at).Length < 0.01)
            {
               return i;
            }
         }

         return -1;
      }
	}
}

