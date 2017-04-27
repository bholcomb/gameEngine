using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Graphics
{
   public class BufferMemoryManager
   {
      public class Page
      {
         public Page(IntPtr s, int sz)
         {
            start = s;
            size = sz;
         }

         public IntPtr start;
         public int size;
         public IntPtr end { get { return start + size; } }
      }

      int myBufferSize;
      PersistentBufferObject myBuffer;
      IntPtr myBufferMemory;
      int myBytesUsed;

      List<Page> myFreePages = new List<Page>();
      List<Page> myUsedPages = new List<Page>();

      private Object myLock = new Object();

      public BufferMemoryManager(int maxBufferSize)
      {
         myBufferSize = maxBufferSize;
         myBuffer = new PersistentBufferObject(BufferUsageHint.StreamDraw, (UInt32)myBufferSize);
         myBufferMemory = myBuffer.ptr;

         clear();
      }

      public PersistentBufferObject buffer { get { return myBuffer; } }
      public int free { get { return myBufferSize - myBytesUsed; } }
      public int used { get { return myBytesUsed; } }

      public void clear()
      {
         lock (myLock)
         {
            myUsedPages.Clear();
            myFreePages.Clear();
            myFreePages.Add(new Page(myBufferMemory, (int)myBufferSize));
            myBytesUsed = 0;
         }
      }

      public Page alloc(int size)
      {
         Page ret = null;
         lock (myLock)
         {
            Page bestFit = null;
            foreach (Page p in myFreePages)
            {
               //it fits!
               if (p.size >= size)
               {
                  if (bestFit == null)
                  {
                     bestFit = p;
                  }
                  else
                  {
                     if (p.size < bestFit.size)
                     {
                        bestFit = p;
                     }
                  }
               }
            }

            //failed to allocate
            if (bestFit == null)
               return null;

            ret = new Page(bestFit.start, size);
            myUsedPages.Add(ret);
            bestFit.start += size;
            bestFit.size -= size;
            myBytesUsed += size;
         }

         return ret;
      }

      public void dealloc(Page p)
      {
         lock (myLock)
         {
            myBytesUsed -= p.size;
            if (myUsedPages.Remove(p) == false)
            {
               Warn.print("Failed to find page in used list");
            }
            myFreePages.Add(p);

            combineFreePages();
         }
      }

      public void visualDebug()
      {
         lock (myLock)
         {
            Vector2 size = new Vector2(1280, 800); // Renderer.context.viewport.size;
            float boundary = size.X / 20;
            size.X -= boundary * 2; //on both sides
            float pixelRatio = (float)size.X / (float)myBufferSize;
            float yLoc = 200;

            foreach (Page p in myFreePages)
            {
               float e = p.size * pixelRatio;
               float s = boundary + (p.start.ToInt64() - myBufferMemory.ToInt64()) * pixelRatio;
               Rect r = new Rect(s, yLoc, s + e, yLoc + 20);
               DebugRenderer.addRect2D(r, Color4.Aqua, Fill.SOLID, false, 0.0);
            }

            yLoc = 220;
            foreach (Page p in myUsedPages)
            {
               float e = p.size * pixelRatio;
               float s = boundary + (p.start.ToInt64() - myBufferMemory.ToInt64()) * pixelRatio;
               Rect r = new Rect(s, yLoc, s + e, yLoc + 20);
               DebugRenderer.addRect2D(r, Color4.Green, Fill.SOLID, false, 0.0);
            }
         }
      }

      void combineFreePages()
      {
         //sort into ascending order by memory location
         myFreePages.Sort((p1, p2) => p1.start.ToInt64().CompareTo(p2.start.ToInt64()));
         List<Page> toRemove = new List<Page>();

         int i = 0;
         int next = 1;
         bool done = false;
         while (!done)
         {
            if (i >= myFreePages.Count || next >= myFreePages.Count)
            {
               done = true;
               continue;
            }

            if (myFreePages[i].end == myFreePages[next].start) //pages run up together
            {
               myFreePages[i].size += myFreePages[next].size;
               myFreePages[next].size = 0;
               toRemove.Add(myFreePages[next]);
               next++;
            }
            else //there's a break
            {
               i = next;
               next = i + 1;
            }
         }

         //cleanup free pages
         foreach (Page j in toRemove)
         {
            myFreePages.Remove(j);
         }
      }
   }
}