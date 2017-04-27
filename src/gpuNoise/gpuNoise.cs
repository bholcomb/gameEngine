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
      static VertexBufferObject<V3> theVBO;
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

         theVBO = new VertexBufferObject<V3>(BufferUsageHint.StaticDraw);
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
         theVAO.bindVertexFormat<V3>(theShader);

			thePipeline = new PipelineState();
			thePipeline.shaderProgram = theShader;
			thePipeline.vao = theVAO;
			thePipeline.generateId();
      }

      public RenderCubemapSphere(Vector3 pos, float size, CubemapTexture tex, bool spining = false)
         :base()
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
      int myLastOctaves = 5;
      float myLastFrequency = 1.0f;
      float myLastOffset = 0.0f;
      float myLastLacunarity = 2.0f;
      float myLastGain = 1.0f;
      float myLastH = 1.0f;
		Fractal.Function myLastFunction = Fractal.Function.fBm;

		public Texture texture;
      public int octaves = 5;
      public float frequency = 1.0f;
      public float offset = 0.0f;
      public float lacunarity = 2.0f;
      public float gain = 1.0f;
      public float H = 1.0f;
		public Fractal.Function function = Fractal.Function.multiFractal;

		Fractal myFractal = new Fractal();
		AutoCorrect myAutoCorrect = new AutoCorrect();

		public GpuNoise2dMap(int width, int height, float seed = 0.0f)
      {
         myWidth = width;
         myHeight = height;
         mySeed = seed;

         myLastOctaves = octaves;
         myLastFrequency = frequency;
         myLastOffset = offset;
         myLastLacunarity = lacunarity;
         myLastGain = gain;
         myLastH = H;

         texture = new Texture(myWidth, myHeight, PixelInternalFormat.R32f);
         generateTexture();
      }

      bool didChange()
      {
         bool diff = octaves != myLastOctaves ||
                frequency != myLastFrequency ||
                offset != myLastOffset ||
                lacunarity != myLastLacunarity ||
                gain != myLastGain ||
                H != myLastH;

         if (diff)
         {
            myLastOctaves = octaves;
            myLastFrequency = frequency;
            myLastOffset = offset;
            myLastLacunarity = lacunarity;
            myLastGain = gain;
            myLastH = H;
         }

         return diff;
      }

      void generateTexture()
      {
			myAutoCorrect.reset();

			myFractal.seed = mySeed;
			myFractal.function = function;
			myFractal.octaves = octaves;
			myFractal.frequency = frequency;
			myFractal.lacunarity = lacunarity;
			myFractal.H = H;
			myFractal.gain = gain;
			myFractal.offset = offset;
			myFractal.face = 0;

			myFractal.generate(texture);
			myAutoCorrect.findMinMax(texture);
         myAutoCorrect.correct(texture);
      }

      public void update()
      {
         if (didChange() == false)
            return;

         generateTexture();
      }
   }

	

	public class GpuNoiseCubeMap
   {
      int myWidth;
      int myHeight;
      float mySeed;
      int myLastOctaves = 5;
      float myLastFrequency = 1.0f;
      float myLastOffset = 0.0f;
      float myLastLacunarity = 2.0f;
      float myLastGain = 1.0f;
      float myLastH = 1.0f;
      Fractal.Function myLastFunction = Fractal.Function.fBm;

      public Texture[] myTextures = new Texture[6];
      public CubemapTexture myCubemap;
      public int octaves = 5;
      public float frequency = 1.0f;
      public float offset = 0.0f;
      public float lacunarity = 2.0f;
      public float gain = 1.0f;
      public float H = 1.0f;
      public Fractal.Function function = Fractal.Function.multiFractal;

      Fractal myFractal = new Fractal();
      AutoCorrect myAutoCorrect = new AutoCorrect();

      public GpuNoiseCubeMap(int width, int height, float seed = 0.0f)
      {
         myWidth = width;
         myHeight = height;
         mySeed = seed;

         for (int i = 0; i < 6; i++)
         {
            myTextures[i] = new Texture(myWidth, myHeight, PixelInternalFormat.Rgba32f);
            myTextures[i].setName(String.Format("Face {0}", i));
         }

         myCubemap = new CubemapTexture(width, height, PixelInternalFormat.Rgba32f);
         myCubemap.setName("Terrain Cubemap");
         generateTextures();
      }

      bool didChange()
      {
         bool diff = octaves != myLastOctaves ||
                frequency != myLastFrequency ||
                offset != myLastOffset ||
                lacunarity != myLastLacunarity ||
                gain != myLastGain ||
                H != myLastH ||
                function != myLastFunction;

         if (diff)
         {
            myLastOctaves = octaves;
            myLastFrequency = frequency;
            myLastOffset = offset;
            myLastLacunarity = lacunarity;
            myLastGain = gain;
            myLastH = H;
            myLastFunction = function;
         }

         return diff;
      }

      void generateTextures()
      {
         myAutoCorrect.reset();

         myFractal.seed = mySeed;
         myFractal.function = function;
         myFractal.octaves = octaves;
         myFractal.frequency = frequency;
         myFractal.lacunarity = lacunarity;
         myFractal.H = H;
         myFractal.gain = gain;
         myFractal.offset = offset;

         //generate each face
         for (int i = 0; i < 6; i++)
         {
            myFractal.face = i;
            myFractal.generate(myTextures[i]);
            myAutoCorrect.findMinMax(myTextures[i]);
         }

          //correct all the images with the global min/max
          for(int i = 0; i < 6; i++)
          {
            myAutoCorrect.correct(myTextures[i]);
         }

         myCubemap.updateFaces(myTextures);
      }

      public void update()
      {
         if (didChange() == false)
            return;

         generateTextures();
      }
   }

	
}