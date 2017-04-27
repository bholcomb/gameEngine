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

		public VisibilityManager()
		{
			myCameraVisibles = new Dictionary<Camera, List<Renderable>>();
		}

		public IEnumerable<Renderable> camaraVisibles(Camera c)
		{
			return myCameraVisibles[c];
		}

		public void updateCameraVisibilityList(List<View> sceneviews)
		{
			myCameraVisibles.Clear();

			foreach (View view in sceneviews)
			{
				List<Renderable> cameraList = null;
				if (myCameraVisibles.TryGetValue(view.camera, out cameraList) == false)
				{
					myCameraVisibles[view.camera] = new List<Renderable>();
				}
			}
		}

		public int renderableCount(Camera c)
		{
			return myCameraVisibles[c].Count;
		}

		public void updateCameraVisibleObjects(List<Renderable> renderables)
		{
#if false
			foreach (KeyValuePair<Camera, ConcurrentBag<Renderable>> cameraList in myCameraVisibles)
			{
				//breakup into several tasks
				var degreeOfParallelism = Environment.ProcessorCount;
				var tasks = new Task[degreeOfParallelism];

				for (int taskNumber = 0; taskNumber < degreeOfParallelism; taskNumber++)
				{
					// capturing taskNumber in lambda wouldn't work correctly
					int taskNumberCopy = taskNumber;

					tasks[taskNumber] = Task.Factory.StartNew(
						 () =>
						 {
							 for (int i = renderables.Count * taskNumberCopy / degreeOfParallelism;
									i < renderables.Count * (taskNumberCopy + 1) / degreeOfParallelism;
									i++)
							 {
								 Renderable r = renderables[i];
								 if (r.isVisible(cameraList.Key) == true)
								 {
									 cameraList.Value.Add(r);
								 }
							 }
						 });
				}

				Task.WaitAll(tasks);
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