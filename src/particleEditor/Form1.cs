using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Graphics;
using Util;

namespace ParticleEditor
{
   public partial class Form1 : Form
   {
      Viewport myViewPort;
      Camera myCamera;
      CameraEventHandler myCameraEventHandler;
      Pipeline myPipeline;
      Renderer.Font myFont;
      Scene myScene;

      System.Windows.Forms.Timer myRenderTimer = new System.Windows.Forms.Timer();

      ParticleManager myParticleManager = new ParticleManager();
      ParticleSystem myParticleSystem = null;

      int x, y;

      bool myIsLoaded = false;

      public Form1()
      {
         InitializeComponent();
      }

      private void glControl1_Load(object sender, EventArgs e)
      {
         glControl1.VSync = true;

         myViewPort = new Viewport(0, 0, glControl1.Width, glControl1.Height);
         myCamera = new Camera(myViewPort);
         myCameraEventHandler = new CameraEventHandler(myCamera);

         myCamera.position = new Vector3(-1, 1, -1);
         myCamera.lookAt(Vector3.Zero);

         myParticleSystem = myParticleManager.loadDefinition("../data/particleSystems/ringOfFire.json");
         particleSystemPropGrid.SelectedObject = myParticleSystem;
         updateFeatureCollection();

         GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

         myPipeline = Renderer.Renderer.createDefaultPipeline(myCamera);
         myPipeline.addRenderStage(new ParticleRenderStage(myPipeline), 100);

         //create the default font
         myFont = FontManager.findFont("FREESANS");

         //create a simple scene
         myScene = new SimpleScene();

         //tell the pipeline to render this scene
         myPipeline.scene = myScene;

         //add the ground plane
         myScene.addInstance(new SimpleModel(SimpleModel.SimpleModelType.TEXURED_PLANE, new Vector3(0f, 0f, 0f), 10.0f, Color4.White));
         myScene.addInstance(myParticleSystem);

         //setup the timer
         myRenderTimer.Interval = 5;
         myRenderTimer.Tick += renderTimerElapsed;
         myRenderTimer.Start();

         myIsLoaded = true;
      }

      void renderTimerElapsed(object sender, EventArgs args)
      {
         //update the clock
         TimeSource.frameStep();

         //update the camera
         myCameraEventHandler.tick((float)TimeSource.timeThisFrame());

         //update the scene
         myScene.update((float)TimeSource.timeThisFrame());

         glControl1.Invalidate();
      }

      String serializeSystem(ParticleSystem ps)
      {
         string s = "";

         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["continuous"] = ps.continuous;
         obj["lifetime"] = ps.lifetime;
         obj["maxParticles"] = ps.maxParticles;
         obj["material"] = ResourceManager.resourceName(ps.material);
         JsonObject features = new JsonObject(JsonObject.JsonType.OBJECT);
         foreach (ParticleFeature pf in ps.features)
         {
            JsonObject f=FeatureSerializer.serialize(pf);
            features[pf.name] = f;
         }
         obj["features"]=features;

         s = obj.generateString();
         return s;
      }

      void updateFeatureCollection()
      {
         featureFlowLayout.Controls.Clear();

         foreach(ParticleFeature pf in myParticleSystem.features)
         {
            PropertyGrid pg=new PropertyGrid();
            pg.Name = pf.name;
            pg.SelectedObject=pf;
            pg.Width = featureFlowLayout.Width-17;
            pg.Height = 200;
            pg.PerformAutoScale();
            featureFlowLayout.Controls.Add(pg);
         }
      }

      private void glControl1_Resize(object sender, EventArgs e)
      {
         if (myIsLoaded == true)
         {
            myViewPort.width = glControl1.Width;
            myViewPort.height = glControl1.Height;
            myViewPort.apply();
         }
      }

      private void glControl1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
      {
         myCameraEventHandler.handleKeyboardDown(InputConvert.convert(e.KeyCode));
      }

      private void glControl1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
      {
         myCameraEventHandler.handleKeyboardUp(InputConvert.convert(e.KeyCode));
      }

      private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         x = e.X - x;
         y = e.Y - y;
         myCameraEventHandler.handleMouseMove(x,y);
         x = e.X;
         y = e.Y;
      }

      private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         myCameraEventHandler.handleMouseButtonDown(InputConvert.convert(e.Button));
      }

      private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         myCameraEventHandler.handleMouseButtonUp(InputConvert.convert(e.Button));
      }

      private void glControl1_Paint(object sender, PaintEventArgs e)
      {
         Renderer.render();

         //render some text
         myFont.print(20, 20, "FPS: {0:0.00}", TimeSource.avgFps());
         myFont.print(20, 40, "View Vector: {0}", myCamera.myViewDir);
         myFont.print(20, 60, "Eye Position: {0}", myCamera.myEye);

         glControl1.SwapBuffers();
      }

      private void loadToolStripMenuItem_Click(object sender, EventArgs e)
      {
         string filename = "";

         OpenFileDialog ofd = new OpenFileDialog();
         DialogResult dr=ofd.ShowDialog();

         if (dr == DialogResult.OK)
         {
            filename = ofd.FileName;
         }
         if (filename != "")
         {
            using (System.IO.StreamReader file = new System.IO.StreamReader(filename))
            {
               string def = file.ReadToEnd();
               if (myParticleSystem != null)
               {
                  myScene.removeInstance(myParticleSystem);
               }
               myParticleSystem = myParticleManager.createSystem(def);
               myScene.addInstance(myParticleSystem);
               particleSystemPropGrid.SelectedObject = myParticleSystem;
            }
         }
      }

      private void saveToolStripMenuItem_Click(object sender, EventArgs e)
      {
         string filename = "";

         SaveFileDialog ofd = new SaveFileDialog();
         DialogResult dr=ofd.ShowDialog();

         if (dr == DialogResult.OK)
         {
            filename = ofd.FileName;
         }
         if (filename != "")
         {
            String s=serializeSystem(myParticleSystem);
            using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(filename))
            {
               outfile.Write(s);
            }
         }
      }
   }
}
