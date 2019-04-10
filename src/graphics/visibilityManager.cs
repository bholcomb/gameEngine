using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Util;

namespace Graphics
{
	public class VisibilityManager
	{
		Dictionary<Camera, List<Renderable>> myCameraVisibles;
      List<Renderable> myNullList = new List<Renderable>();

		public VisibilityManager()
		{
			myCameraVisibles = new Dictionary<Camera, List<Renderable>>();
		}

		public List<Renderable> camaraVisibles(Camera c)
		{
         if (myCameraVisibles.ContainsKey(c) == true)
         {
            return myCameraVisibles[c];
         }

         return myNullList;
		}

		public void initializeCamaraVisibleLists(List<View> activeViews)
		{
			myCameraVisibles.Clear();

			foreach (View view in activeViews)
			{
            if(view.processRenderables == false)
            {
               //we don't want to process it
               continue;
            }

            List<Renderable> cameraVisibleList = null;
				if (myCameraVisibles.TryGetValue(view.camera, out cameraVisibleList) == false)
				{
               cameraVisibleList = new List<Renderable>();
               myCameraVisibles[view.camera] = cameraVisibleList;
				}
			}
		}

		public int renderableCount(Camera c)
		{
         if (myCameraVisibles.ContainsKey(c) == true)
         {
            return myCameraVisibles[c].Count;
         }
         else
         {
            return 0;
         }
		}

		public void cullRenderablesPerCamera(List<Renderable> renderables, List<View> activeViews)
		{
         //generate lists for each active camera
         initializeCamaraVisibleLists(activeViews);

#if false
			foreach (KeyValuePair<Camera, List<Renderable>> cameraList in myCameraVisibles)
			{
				//breakup into several tasks
				var degreeOfParallelism = Environment.ProcessorCount;
				var tasks = new Task<List<Renderable>>[degreeOfParallelism];
            int taskRenderablesCount = renderables.Count / degreeOfParallelism;

				for (int taskNumber = 0; taskNumber < degreeOfParallelism; taskNumber++)
				{
               //get the task number as a local up-value (closure)
					int taskNumberCopy = taskNumber;
               
					tasks[taskNumber] = Task.Factory.StartNew(
						 () =>
						 {
                      List<Renderable> ret = new List<Renderable>();
							 for (int i = 0; i < taskRenderablesCount; i++)
							 {
                         int rIdx = i + (taskRenderablesCount * taskNumberCopy);
                         if(rIdx < renderables.Count)
                         {
                            Renderable r = renderables[rIdx];
                            if(r.isVisible(cameraList.Key) == true)
                            {
                               ret.Add(r);
                               //cameraList.Value.Add(r);
                            }
                         }
							 }

                      return ret;
						 });
				}

				Task.WaitAll(tasks);
            foreach(var t in tasks)
            {
               cameraList.Value.AddRange(t.Result);
            }
			}
#endif

#if false
			foreach (KeyValuePair<Camera, ConcurrentBag<Renderable>> cameraList in myCameraVisibles)
			{
				Parallel.ForEach(renderables, (r) =>
					{
						if (r.isVisible(cameraList.Key) == true)
						{
							cameraList.Value.Add(r);
						}
					});
			}
#endif

#if true
			foreach (KeyValuePair<Camera, List<Renderable>> cameraList in myCameraVisibles)
			{
				foreach(Renderable r in renderables)
				{
					if (r.isVisible(cameraList.Key) == true)
					{
						cameraList.Value.Add(r);
					}
				}
			}
#endif
      }
   }
}