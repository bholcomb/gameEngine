using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class RenderStage
	{
		public delegate void RenderStageFunction(RenderStage stage);

		public List<View> views;
		public bool isActive { get; set; }
		public String name { get; set; }
		public String passType { get; set; }

		public List<RenderCommand> preStageCommands;
		public List<RenderCommand> postStageCommands;

		public event RenderStageFunction onPreExecute;
		public event RenderStageFunction onPostExecute;

		public RenderStage(String stageName, string pass)
		{
			name = stageName;
			passType = pass;
			isActive = true;
			views = new List<View>();

			preStageCommands = new List<RenderCommand>();
			postStageCommands = new List<RenderCommand>();
		}

		public virtual void init(InitTable config)
		{

		}

		public void preExectue()
		{
			if (onPreExecute != null)
				onPreExecute(this);
		}

		public void postExectue()
		{
			if (onPostExecute != null)
				onPostExecute(this);
		}

		public virtual void registerView(View v)
      {
			v.passType = passType;
			views.Add(v);
      }

      public virtual void unregisterView(View v)
      {
			views.Remove(v);
      }
   }
}