/*********************************************************************************

Copyright (c) 2011 Robert C. Holcomb Jr.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


*********************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Planet
{
   public class PriorityQueue
   {
      List<LinkedList<Tri>> myBuckets;
      List<int> myMaxCount;

      public PriorityQueue(int buckets)
      {
         myMaxCount = new List<int>(buckets);
         myBuckets = new List<LinkedList<Tri>>(buckets);
         for (int i = 0; i < buckets; i++)
         {
            myBuckets.Add(new LinkedList<Tri>());
            myMaxCount.Add(0);
         }
      }

      public int buckets
      {
         get { return myBuckets.Count; }
      }

      public int bucketCount(int i)
      {
         return myMaxCount[i];
      }

      public void reset()
      {
         for (int i = 0; i < myBuckets.Count; i++)
         {
            myBuckets[i].Clear();
            myMaxCount[i] = 0;
         }
      }

      public void addTri(Tri t)
      {
         if (t.priority > 1.0) t.priority = 1.0f;
         if (t.priority < 0.0) t.priority = 0.0f;

         int b = (int)((1.0 - t.priority) * (myBuckets.Count - 1));
         myBuckets[b].AddLast(t);
         myMaxCount[b]++;
      }

      public void removeTri(Tri t)
      {
         int b = (int)((1.0 - t.priority) * (myBuckets.Count - 1));
         myBuckets[b].Remove(t);
      }

      public Tri getTop()
      {
         for (int i = 0; i < myBuckets.Count; i++)
         {
            if (myBuckets[i].Count > 0)
            {
               LinkedList<Tri>.Enumerator e = myBuckets[i].GetEnumerator();
               e.MoveNext();
               Tri t = e.Current;
               myBuckets[i].RemoveFirst();
               return t;
            }
         }

         return null;
      }
   }
}
