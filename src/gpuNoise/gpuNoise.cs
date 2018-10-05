using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;


namespace GpuNoise
{
   public class RenderCubemapSphere : StatelessRenderCommand
   {
      static ShaderProgram theShader;
      static VertexBufferObject theVBO;
      static IndexBufferObject theIBO;
      static VertexArrayObject theVAO;
      static Matrix4 theOrientation;
      static PipelineState thePipeline;

      bool myIsSpinning;
      Vector3 myPos;
      float mySize;

      static RenderCubemapSphere()
      {
         theOrientation = Matrix4.CreateFromQuaternion(new Quaternion(0f, 0f, 0f));

         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.VertexShader, "GpuNoise.shaders.cube-vs.glsl"));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "GpuNoise.shaders.cube-ps.glsl"));

         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         theVBO = new VertexBufferObject(BufferUsageHint.StaticDraw);
         theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

         int lats = 25;
         int longs = 25;
         int vertIdx = 0;

         V3[] verts = new V3[(lats + 1) * (longs + 1)];
         ushort[] index = new ushort[lats * longs * 6];

         for (int i = 0; i <= lats; i++)
         {
            float theta = (float)i * (float)Math.PI / lats;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= longs; j++)
            {
               float phi = (float)j * (float)(Math.PI * 2) / longs;
               float sinPhi = (float)Math.Sin(phi);
               float cosPhi = (float)Math.Cos(phi);

               float x = cosPhi * sinTheta;
               float y = sinPhi * sinTheta;
               float z = cosTheta;
               //          float u = 1 - (j / longs);
               //          float v = 1- (i / lats);

               V3 temp = new V3();
               temp.Position = new Vector3(x, y, z);
               verts[vertIdx++] = temp;
            }
         }

         int indexIdx = 0;
         for (int i = 0; i < lats; i++)
         {
            for (int j = 0; j < longs; j++)
            {
               ushort first = (ushort)((i * (longs + 1)) + j);
               ushort second = (ushort)(first + longs + 1);
               index[indexIdx++] = first; index[indexIdx++] = second; index[indexIdx++] = (ushort)(first + 1);
               index[indexIdx++] = second; index[indexIdx++] = (ushort)(second + 1); index[indexIdx++] = (ushort)(first + 1);
            }
         }

         theVBO.setData(verts);
         theIBO.setData(index);

         theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3.bindings());

         thePipeline = new PipelineState();
         thePipeline.shaderState.shaderProgram = theShader;
         thePipeline.vaoState.vao = theVAO;
         thePipeline.generateId();


      }

      public RenderCubemapSphere(Vector3 pos, float size, CubemapTexture tex, bool spining = false)
         : base()
      {
         myIsSpinning = spining;
         myPos = pos;
         mySize = size;

         if (myIsSpinning == true)
         {
            float rot = (float)(TimeSource.currentTime() / 2.0);
            theOrientation = Matrix4.CreateFromQuaternion(new Quaternion(0, rot, 0));
            renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, theOrientation * Matrix4.CreateTranslation(myPos)));
         }
         else
         {
            renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, Matrix4.CreateTranslation(myPos)));
         }
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, size));

         renderState.setTexture(tex.id(), 0, tex.target);
         renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));

         renderState.setVertexBuffer(theVBO.id, 0, 0, V3.stride);
         renderState.setIndexBuffer(theIBO.id);

         pipelineState = thePipeline;
      }

      public override void execute()
      {
         base.execute();

         GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
      }
   }

   public class GpuNoise2dMap
   {
      int myWidth;
      int myHeight;
      float mySeed;

      public Texture texture;
      public int octaves = 5;
      public float frequency = 1.0f;
      public float offset = 0.0f;
      public float lacunarity = 2.0f;
      public float gain = 1.0f;
      public float H = 1.0f;
      public Fractal2d.Method function = Fractal2d.Method.multiFractal;

      Fractal2d myFractal;
      AutoCorrect myAutoCorrect;

      public GpuNoise2dMap(int width, int height, float seed = 0.0f)
      {
         myWidth = width;
         myHeight = height;
         mySeed = seed;

         texture = new Texture(myWidth, myHeight, PixelInternalFormat.R32f);
         myFractal = new Fractal2d(myWidth, myHeight);
         myAutoCorrect = new AutoCorrect(myWidth, myHeight);
         myAutoCorrect.source = myFractal;

         generateTexture();
      }

      void generateTexture()
      {
         myFractal.seed = mySeed;
         myFractal.method = function;
         myFractal.octaves = octaves;
         myFractal.frequency = frequency;
         myFractal.lacunarity = lacunarity;
         myFractal.H = H;
         myFractal.gain = gain;
         myFractal.offset = offset;

         myAutoCorrect.update();
      }

      public void update()
      {
         generateTexture();
      }
   }



   public class GpuNoiseCubeMap
   {
      int myWidth;
      int myHeight;
      float mySeed;

      public Texture[] myTextures = new Texture[6];
      public CubemapTexture myCubemap;
      public int octaves = 5;
      public float frequency = 1.0f;
      public float offset = 0.0f;
      public float lacunarity = 2.0f;
      public float gain = 1.0f;
      public float H = 1.0f;
      public Fractal3d.Method function = Fractal3d.Method.multiFractal;

      Fractal3d[] myFractal = new Fractal3d[6];
      AutoCorrect myAutoCorrect;

      public GpuNoiseCubeMap(int width, int height, float seed = 0.0f)
      {
         myWidth = width;
         myHeight = height;
         mySeed = seed;

         for (int i = 0; i < 6; i++)
         {
            myTextures[i] = new Texture(myWidth, myHeight, PixelInternalFormat.R32f);
            myFractal[i] = new Fractal3d(myWidth, myHeight);
            myFractal[i].face = i;
         }

         myCubemap = new CubemapTexture(width, height, PixelInternalFormat.R32f);
         myCubemap.setName("Terrain Cubemap");

         myAutoCorrect = new AutoCorrect(myWidth, myHeight);

         generateTextures();
      }

      void generateTextures()
      {
         myAutoCorrect.reset();

         //generate each face and update the min/max
         for (int i = 0; i < 6; i++)
         {
            myFractal[i].seed = mySeed;
            myFractal[i].method = function;
            myFractal[i].octaves = octaves;
            myFractal[i].frequency = frequency;
            myFractal[i].lacunarity = lacunarity;
            myFractal[i].H = H;
            myFractal[i].gain = gain;
            myFractal[i].offset = offset;
            myFractal[i].face = i;
            myFractal[i].update();

            myAutoCorrect.findMinMax(myFractal[i].output);
         }
         
         //correct all the images with the same min/max
         for (int i = 0; i < 6; i++)
         {
            myAutoCorrect.output = myTextures[i];
            myAutoCorrect.correct(myFractal[i].output);
         }
         
         myCubemap.updateFaces(myTextures);
      }

      public void update()
      {
         generateTextures();
      }
   }


}