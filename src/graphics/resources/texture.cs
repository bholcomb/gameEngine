using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class TextureDescriptor : ResourceDescriptor
   {
      bool myFlip = false;
      String myObjName = "";

      public TextureDescriptor(string filename, String objName="") : this(filename, false, objName) { }
      public TextureDescriptor(string filename, bool flip, String objName="")
         : base(filename)
      {
         myFlip = flip;
         myObjName = objName;
      }

      public override IResource create(ResourceManager mgr)
      {
         Texture t = new Texture(name, myFlip);
         if (t.isValid == false)
         {
            return null;
         }

         if (myObjName != "")
            t.setName(myObjName);

         return t;
      }
   }

   public class TextureSampler
   {
      static float theMaxAniso;
      Texture myTexture;
      protected int myUnit = 0;

      public TextureSampler(Texture tex) : this(tex, 0) { }
      public TextureSampler(Texture tex, int unit)
      {
         myTexture = tex;
         myUnit = unit;

         if (myTexture != null)
         {
            minFilter = TextureMinFilter.LinearMipmapLinear;
            magFilter = TextureMagFilter.Linear;
            wrapS = TextureWrapMode.Repeat;
            wrapT = TextureWrapMode.Repeat;
            wrapR = TextureWrapMode.Repeat;
            anistropicFilter = theMaxAniso;
         }
      }

      static TextureSampler()
      {
         GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out theMaxAniso);
      }

      public TextureMinFilter minFilter { get; set; }
      public TextureMagFilter magFilter { get; set; }
      public TextureWrapMode wrapS { get; set; }
      public TextureWrapMode wrapT { get; set; }
      public TextureWrapMode wrapR { get; set; }
      public float anistropicFilter { get; set; }

      public virtual void bind()
      {
         GL.ActiveTexture(TextureUnit.Texture0 + myUnit);
         myTexture.bind();
         GL.TexParameter(myTexture.target, TextureParameterName.TextureMinFilter, (int)minFilter);
         GL.TexParameter(myTexture.target, TextureParameterName.TextureMagFilter, (int)magFilter);
         GL.TexParameter(myTexture.target, TextureParameterName.TextureWrapS, (int)wrapS);
         GL.TexParameter(myTexture.target, TextureParameterName.TextureWrapT, (int)wrapT);
         GL.TexParameter(myTexture.target, TextureParameterName.TextureWrapR, (int)wrapR);
         GL.TexParameter(myTexture.target, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, anistropicFilter);
      }

      public virtual void unbind()
      {
         GL.ActiveTexture(TextureUnit.Texture0 + myUnit);
         //GL.Disable(textureMode);

         //shouldn't need to do this, but until I get everything using the render system, this is safe
         GL.ActiveTexture(TextureUnit.Texture0);
      }

      public virtual Int32 id()
      {
         return myTexture.id();
      }

      public int unit { get { return myUnit; } }
   }

   public class Texture : IResource
   {
      protected int myId;
      protected Byte[] myData;
      protected int myHeight;
      protected int myWidth;
      protected bool myFlip = false;
      protected OGL.PixelInternalFormat myPixelFormat;
      protected OGL.PixelType myDataType;

      public bool hasAlpha { get; set; }
      public TextureTarget target { get; set; }
      public OGL.PixelInternalFormat pixelFormat {get {return myPixelFormat;} }

      public class PixelData
      {
         public OGL.PixelFormat pixelFormat = OGL.PixelFormat.Rgba;
         public OGL.PixelType dataType = OGL.PixelType.Byte;
         public Byte[] data = null;

         public PixelData()
         {

         }
      };

      public Texture() { }
      public Texture(int id) 
      { 
         myId = id;
         hasAlpha = false;
         target = TextureTarget.Texture2D;


			bind();
         GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
         GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
         GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

         GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out myWidth);
         GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out myHeight);
			unbind();
		}

      public Texture(String filename) : this(filename, false) { }
      public Texture(String filename, bool flip)
      {
         myFlip = flip;
         hasAlpha = false;
         target = TextureTarget.Texture2D;
         if(LoadFromDisk(filename)==false)
         {
            GL.DeleteTexture(myId);
            myId = 0;
            System.Console.WriteLine("Cannot create texture from file: {0}", filename);
         }
		}

      public Texture(int texWidth, int texHeight, OGL.PixelInternalFormat pif = PixelInternalFormat.Rgba8, PixelData pixels = null , bool generateMipmaps=false)
      {
         target = TextureTarget.Texture2D;
         myWidth = texWidth;
         myHeight = texHeight;
         myPixelFormat = pif;

         GL.GenTextures(1, out myId);
			bind();

         if (pixels != null)
         {
            myData = pixels.data;
            myDataType = pixels.dataType;
            GL.TexImage2D(target, 0, myPixelFormat, myWidth, myHeight, 0, pixels.pixelFormat, myDataType, myData);
         }
         else
         {
            myData = null;
            OGL.PixelFormat pf;
            findPixelType(myPixelFormat, out pf, out myDataType);
            GL.TexImage2D(target, 0, myPixelFormat, myWidth, myHeight, 0, pf, myDataType, myData);
         }

         GL.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
         if (generateMipmaps == false)
         {
            GL.TexParameter(target, TextureParameterName.TextureMaxLevel, 0); //needed since no mip maps are created
				setMinMagFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
			}

			setWrapping(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
         
         if(myData==null)
            Info.print("Created empty texture with id: " + myId);
         else
            Info.print("Created user defined texture with id: " + myId);

			if (generateMipmaps)
			{
				createMipMaps();
				setMinMagFilters(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
			}

			unbind();
		}

		public void setMinMagFilters(TextureMinFilter min, TextureMagFilter mag)
		{
			bind();
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)min);
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)mag);
			unbind();
		}

		public void setWrapping(TextureWrapMode s, TextureWrapMode t, TextureWrapMode r = TextureWrapMode.Repeat)
		{
			bind();
			GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)s);
			GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)t);
			GL.TexParameter(target, TextureParameterName.TextureWrapR, (int)r);
			unbind();
		}

      protected void findPixelType(OGL.PixelInternalFormat pif, out OGL.PixelFormat pf, out OGL.PixelType pt)
      {
         switch (myPixelFormat)
         {
            case PixelInternalFormat.DepthComponent32f:
               pt = PixelType.Float;
               pf = OGL.PixelFormat.DepthComponent;
               break;
            case PixelInternalFormat.DepthComponent:
               pt = PixelType.Float;
               pf = OGL.PixelFormat.DepthComponent;
               break;
            case PixelInternalFormat.R32f:
               pt = PixelType.Float;
               pf = OGL.PixelFormat.Red;
               break;
            case PixelInternalFormat.Rgb32f:
               pt = PixelType.Float;
               pf = OGL.PixelFormat.Rgb;
               break;
            case PixelInternalFormat.Rgba32f:
               pt = PixelType.Float;
               pf = OGL.PixelFormat.Rgba;
               break;
            case PixelInternalFormat.Rgb8:
               pt = PixelType.Byte;
               pf = OGL.PixelFormat.Rgb;
               break;
            case PixelInternalFormat.Rgba8:
               pt = PixelType.Byte;
               pf = OGL.PixelFormat.Rgba;
               break;
            default:
               pt = PixelType.Byte;
               pf = OGL.PixelFormat.Rgba;
               break;
         }
      }

      public void createMipMaps()
      {
			bind();
         GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			unbind();
		}

      public void Dispose()
      {
         GL.DeleteTexture(myId);
      }

      public int id()
      {
         return myId;
      }

      public bool isValid
      {
         get { return myId!=0; }
      }

      public int width
      {
         get { return myWidth; }
      }

      public int height
      {
         get { return myHeight; }
      }
      public void setName(String name)
      {
         GL.ObjectLabel(ObjectLabelIdentifier.Texture, myId, name.Length, name);
      }

      public virtual bool bind()
      {
         if (myId == 0)
         {
            return false;
         }

         GL.BindTexture(target, myId);

         return true;
      }

		public virtual bool unbind()
		{
			if (myId == 0)
			{
				return false;
			}

			GL.BindTexture(target, 0);

			return true;
		}

      public bool LoadFromDisk(string filename)
      {
         GL.GenTextures(1, out myId);
			bind();

         System.Console.Write("Loading Texture {0} with {1}...", myId, filename);

         OGL.PixelFormat pf;
         OGL.PixelType pt;
         Bitmap bm = loadBitmap(filename, out myPixelFormat, out pf, out pt);
         if(bm!=null)
         {
            BitmapData Data = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            myWidth = bm.Width;
            myHeight = bm.Height;

            GL.TexImage2D(target, 0, myPixelFormat, myWidth, myHeight, 0, pf, pt, Data.Scan0);
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            bm.UnlockBits(Data);
         }

         GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			System.Console.WriteLine("Done!");
			unbind();

         return true;
      }

      public bool loadFromStream(Stream stream)
      {
         target = TextureTarget.Texture2D;
         GL.GenTextures(1, out myId);
			bind();

         System.Console.Write("Loading Texture {0} from stream...", myId);

         OGL.PixelFormat pf;
         OGL.PixelType pt;
         Bitmap bm = new Bitmap(stream);

         //flip it since we want the origin to be the bottom left and the image has the origin in the top left
         bm.RotateFlip(RotateFlipType.RotateNoneFlipY);

         evalBitmap(bm, out myPixelFormat, out pf, out pt);

         BitmapData Data = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);

         myWidth = Data.Width;
         myHeight = Data.Height;

         GL.TexImage2D(target, 0, myPixelFormat, myWidth, myHeight, 0, pf, pt, Data.Scan0);
         bm.UnlockBits(Data);

         GL.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
         GL.TexParameter(target, TextureParameterName.TextureMaxLevel, 0);
         GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
         GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
         GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

         GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			
         System.Console.WriteLine("Done!");
			unbind();

         return true;

      }

      public void evalBitmap(Bitmap bm, out OGL.PixelInternalFormat pif, out OGL.PixelFormat pf, out OGL.PixelType pt)
      {
         switch (bm.PixelFormat)
         {
            case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: // misses glColorTable setup
               pif = OGL.PixelInternalFormat.Rgb8;
               pf = OGL.PixelFormat.ColorIndex;
               pt = OGL.PixelType.Bitmap;
               break;
            case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
            case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: // does not work
               pif = OGL.PixelInternalFormat.Rgb5A1;
               pf = OGL.PixelFormat.Bgr;
               pt = OGL.PixelType.UnsignedShort5551Ext;
               hasAlpha = true;
               break;
            /*  case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                  pif = OGL.PixelInternalFormat.R5G6B5IccSgix;
                  pf = OGL.PixelFormat.R5G6B5IccSgix;
                  pt = OGL.PixelType.UnsignedByte;
                  break;
            */
            case System.Drawing.Imaging.PixelFormat.Format24bppRgb: // works
               pif = OGL.PixelInternalFormat.Rgb8;
               pf = OGL.PixelFormat.Bgr;
               pt = OGL.PixelType.UnsignedByte;
               break;
            case System.Drawing.Imaging.PixelFormat.Format32bppRgb: // has alpha too? wtf?
            case System.Drawing.Imaging.PixelFormat.Canonical:
            case System.Drawing.Imaging.PixelFormat.Format32bppArgb: // works
               pif = OGL.PixelInternalFormat.Rgba;
               pf = OGL.PixelFormat.Bgra;
               pt = OGL.PixelType.UnsignedByte;
               hasAlpha = true;
               break;
            default:
               throw new ArgumentException("ERROR: Unsupported Pixel Format " + bm.PixelFormat);
         }
      }

      public Bitmap loadBitmap(string filename, out OGL.PixelInternalFormat pif, out OGL.PixelFormat pf, out OGL.PixelType pt)
      {
         filename = Path.GetFullPath(filename);
         if (File.Exists(filename) == false)
         {
            throw new Exception("File " + filename + " does not exist");
         }

         Bitmap CurrentBitmap = null;

         try // Exceptions will be thrown if any Problem occurs while working on the file. 
         {
            if (Path.GetExtension(filename) == ".pcx")
               CurrentBitmap = PCX.load(filename);
            else
               CurrentBitmap = new Bitmap(filename);

            evalBitmap(CurrentBitmap, out pif, out pf, out pt);

            //flip the image since it's backwards from what opengl expects
            if (myFlip == true)
            {
               CurrentBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return CurrentBitmap;
         }

         catch (Exception e)
         {
            throw new Exception("Texture Loading Error: Failed to read file " + filename + ": " + e.Message);
         }
      }

      public void clear()
      {
         for (int y = 0; y < myHeight; y++)
         {
            for (int x = 0; x < myWidth; x++)
            {
               myData[((y * 4) * myWidth) + (x * 4) + 0] = 0;
               myData[((y * 4) * myWidth) + (x * 4) + 1] = 0;
               myData[((y * 4) * myWidth) + (x * 4) + 2] = 0;
               myData[((y * 4) * myWidth) + (x * 4) + 3] = 0;
            }
         }

         updateData();
      }

      public void updateData()
      {
         GL.Enable(EnableCap.Texture2D);
			bind();

         OGL.PixelInternalFormat pif = OGL.PixelInternalFormat.Rgba;
         OGL.PixelFormat pf = OGL.PixelFormat.Rgba;
         OGL.PixelType pt = OGL.PixelType.UnsignedByte;

         GL.TexImage2D<Byte>(target, 0, pif, myWidth, myHeight, 0, pf, pt, myData);

			unbind();
      }

      public bool paste(Byte[] source, Vector2 loc, Vector2 size, OGL.PixelFormat pf)
      {
         if (myId == 0)
         {
            return false;
         }

         GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

			bind();

         OGL.PixelType pt = OGL.PixelType.UnsignedByte;
         GL.TexSubImage2D<Byte>(target, 0, (int)loc.X, (int)loc.Y, (int)size.X, (int)size.Y, pf, pt, source);

			unbind();
         return true;
      }

      public bool paste(float[] source, Vector2 loc, Vector2 size, OGL.PixelFormat pf)
      {
         if (myId == 0)
         {
            return false;
         }

         GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

			bind();

         OGL.PixelType pt = OGL.PixelType.Float;
         GL.TexSubImage2D<float>(target, 0, (int)loc.X, (int)loc.Y, (int)size.X, (int)size.Y, pf, pt, source);

			unbind();
         return true;
      }

      public void setPixels(ref Color4[,] pixels)
      {
         Vector2 size = new Vector2(pixels.GetLength(0), pixels.GetLength(1));
         float[] data = new float[(int)size.X * (int)size.Y * 4];

         for(int y=0; y<size.Y; y++)
         {
            for (int x = 0; x < size.X; x++)
            {
               int offset = ((y * (int)size.X + x) * 4);
               data[offset + 0] = pixels[x, y].R;
               data[offset + 1] = pixels[x, y].G;
               data[offset + 2] = pixels[x, y].B;
               data[offset + 3] = pixels[x, y].A;
            }
         }

         paste(data, Vector2.Zero, size, OGL.PixelFormat.Rgba);
      }

      public virtual bool saveData(string filename)
      {
         bind();
         int depth = 3;
         byte[] data = new byte[width * height * depth];
         GL.PixelStore(PixelStoreParameter.PackAlignment, 1);

         GL.GetTexImage(target, 0, OGL.PixelFormat.Rgb, PixelType.Byte, data);

         Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
         int pos = 0;
         for (int y = 0; y < height; y++)
         {
            for (int x = 0; x < width; x++)
            {
               bmp.SetPixel(x, y, Color.FromArgb(data[pos], data[pos + 1], data[pos + 2]));
               pos += 3;
            }
         }

         bmp.Save(filename);
         unbind();

         return true;
      }
   }
}