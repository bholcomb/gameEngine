using System;
using System.Collections.Generic;

using Util;

namespace Graphics
{
	public class BaseRenderQueue
	{
		public RenderCommandList commands;
		public Visualizer visualizer;
		public PipelineState myPipeline;

		public BaseRenderQueue(PipelineState pipeline)
		{
			myPipeline = pipeline;
			commands = new RenderCommandList();
		}

		public virtual void reset()
		{

			commands.Clear();
		}

		public void addCommand(RenderCommand cmd)
		{
			commands.Add(cmd);
		}

		public virtual void sort()
		{

		}

		public virtual void generateRenderCommands()
		{
         addCommand(new SetPipelineCommand(myPipeline));
      }
   }

	public class RenderQueue<T> : BaseRenderQueue where T: RenderInfo, new()
	{
		public List<T> myInfos = new List<T>();
		public int myInfoCount = 0;

		public RenderQueue(PipelineState pipeline) : base(pipeline)
		{
		}

		public T nextInfo()
		{
			if (myInfoCount >= myInfos.Count)
			{
				T newInfo = new T();
				newInfo.pipeline = myPipeline;
				myInfos.Add(newInfo);
			}

			T info = myInfos[myInfoCount++];
			info.renderState.reset();
			return info;
		}

		public override void reset()
		{
			base.reset();
			myInfoCount = 0;
		}

		public class InfoComparer : IComparer<T>
		{
			public int Compare(T a, T b)
			{
				return a.sortId.CompareTo(b.sortId);
			}
		}

		static InfoComparer theComparer = new InfoComparer();

		public override void sort()
		{
			myInfos.Sort(0, myInfoCount, theComparer);
		}

		public override void generateRenderCommands()
		{
         base.generateRenderCommands();

         visualizer.generateRenderCommandsBegin(this);

			for (int i = 0; i < myInfoCount; i++)
			{
				visualizer.generateRenderCommand(myInfos[i], this);
			}

         visualizer.generateRenderCommandsFinalize(this);
		}
	}
}