using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using Util;
using GUI;
using Graphics;
using GpuNoise;

namespace WorldEditor
{
   public class MainWindow
   {
      GameWindow myGameWindow;

      enum Layers { Elevation, Heat, Moisture, Biome };
      Layers myEditLayer = Layers.Elevation;
      Layers myViewLayer = Layers.Biome;
      bool myShowWater;

      float myScale = 1.0f;
      Vector2 myPos = Vector2.Zero;

      ShaderProgram myDisplayBiomeShader;

      World myWorld;
      
      public MainWindow(GameWindow window, World world)
      {
         myGameWindow = window;
         myWorld = world;

         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "World Editor.shaders.display-biome-ps.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myDisplayBiomeShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void onGui()
      {
         UI.currentWindow.flags |= Window.Flags.MenuBar;
         if (UI.beginMenuBar() == true)
         {
            if (UI.beginMenu("File") == true)
            {
               if (UI.menuItem("Load") == true)
               {
                  myWorld.myGenerator.load("../data/terrain/worldDefinitions/terrain.lua");
               }

               if (UI.menuItem("Save") == true)
               {
                  myWorld.myGenerator.save("../data/terrain/worldDefinitions/terrain.lua");
               }

               if (UI.menuItem("Exit") == true)
               {
                  DialogResult res = MessageBox.Show("Are you sure", "Quit", MessageBoxButtons.OKCancel);
                  if (res == DialogResult.OK)
                  {
                     myGameWindow.Exit();
                  }
               }

               UI.endMenu();
            }

            if (UI.beginMenu("Edit") == true)
            {
               UI.endMenu();
            }

            UI.endMenuBar();

            float p = UI.percent(30.0f, Layout.Direction.Horizontal);
            Vector2 s = UI.currentWindow.size;

            UI.beginWindow("left panel", Window.Flags.Borders | Window.Flags.Inputs | Window.Flags.Background | Window.Flags.MenuBar);
            UI.setWindowPosition(new Vector2(0, 20));
            UI.setWindowSize(new Vector2(p, s.Y - 20));

            UI.beginLayout(Layout.Direction.Horizontal);
            UI.label("Edit Layer");
            UI.combo("editlayers", ref myEditLayer);
            UI.spacer(20.0f);
            UI.label("View Layer");
            UI.combo("viewLayers", ref myViewLayer);
            UI.endLayout();

            UI.checkbox("show water", ref myShowWater);

            UI.separator();
            switch (myEditLayer)
            {
               case Layers.Elevation:
                  nodeUI(myWorld.myGenerator.elevation);
                  break;
               case Layers.Heat:
                  nodeUI(myWorld.myGenerator.heat);
                  break;
               case Layers.Moisture:
                  nodeUI(myWorld.myGenerator.moisture);
                  break;
               case Layers.Biome:
                  break;
            }


            UI.endWindow();

            UI.beginWindow("right panel", Window.Flags.Borders | Window.Flags.Inputs | Window.Flags.Background | Window.Flags.MenuBar);
            UI.setWindowPosition(new Vector2(p, 20));
            UI.setWindowSize(new Vector2(s.X - p, s.Y - 20));

            drawMap();

            UI.endWindow();
         }

         void drawMap()
         {
            Texture t = myWorld.myGenerator.myBiomeMap;
            Vector2 tSize = new Vector2(t.width, t.height);

            Window win = UI.currentWindow;
            if(win.rect.containsPoint(UI.mouse.pos) == true)
            {
               if (UI.mouse.buttonIsDown(MouseButton.Left) == true)
               {
                  Vector2 move = UI.mouse.delta;
                  move.Y = -move.Y;
                  myPos += move;

                  myPos.X = MathHelper.Clamp(myPos.X, -tSize.X * myScale, tSize.X * myScale);
                  myPos.Y = MathHelper.Clamp(myPos.Y, -tSize.Y * myScale, tSize.Y * myScale);
               }

               myScale += UI.mouse.wheelDelta * 0.1f;
               myScale = MathHelper.Clamp(myScale, 0.1f, 1000.0f);
            }

            
            Vector2 start = myPos + UI.currentWindow.cursorScreenPosition;
            Vector2 end = start + new Vector2(t.width, t.height) * myScale;
            RenderTexture2dCommand cmd = new RenderTexture2dCommand(start, end, t);
            cmd.pipelineState.shaderState.shaderProgram = myDisplayBiomeShader;
            cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Int, myViewLayer)); //show specific layer
            cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Bool, myShowWater)); //show water

            UI.currentWindow.canvas.addCustomRenderCommand(cmd);
         }

         void nodeUI(ModuleTree tree)
         {
            foreach (Module m in tree.moduleOrderedList())
            {
               String s = m.myName;
               UI.idStack.push(s);
               UI.label(s);

               if (m is Fractal2d)
               {
                  Fractal2d f = m as Fractal2d;
                  UI.slider("Function", ref f.method);
                  UI.slider("Octaves", ref f.octaves, 1, 10);
                  UI.slider("Frequency", ref f.frequency, 0.1f, 10.0f);
                  UI.slider("lacunarity", ref f.lacunarity, 1.0f, 3.0f);
                  UI.slider("Gain", ref f.gain, 0.1f, 2.0f);
                  UI.slider("Offset", ref f.offset, -1.0f, 1.0f);
                  UI.slider("H", ref f.H, 0.1f, 2.0f);
               }

               if (m is Gradient)
               {
                  Gradient g = m as Gradient;
                  UI.slider("X0", ref g.x0, 0.0f, 1.0f);
                  UI.slider("X1", ref g.x1, 0.0f, 1.0f);
                  UI.slider("Y0", ref g.y0, 0.0f, 1.0f);
                  UI.slider("Y1", ref g.y1, 0.0f, 1.0f);
               }

               if (m is Pow)
               {
                  Pow p = m as Pow;
                  UI.slider("Power", ref p.pow, -2.0f, 2.0f);
               }

               if (m is Constant)
               {
                  Constant c = m as Constant;
                  UI.slider("Constant", ref c.val, -1.0f, 1.0f);
               }

               UI.separator();
               UI.idStack.pop();
            }
         }
      }
   }
}
