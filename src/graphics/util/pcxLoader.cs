using System;
using System.IO;
using System.Drawing; 
using System.Drawing.Imaging; 
using System.Drawing.Drawing2D; 
using System.Runtime.InteropServices;

namespace Graphics
{
   public static class PCX
   {
      public static Bitmap load(string ImagePath)
      {
         char bitsPerPixel;
         int xSize;
         int ySize;
         int hDPI;
         int vDPI;
         //int colorMap;
         Bitmap pcxImage;

         //read header
         byte[] fileHeader = new byte[128];
         FileStream fs = new FileStream(ImagePath,
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.None);
         fs.Read(fileHeader, 0, 128);
         byte isPCX = fileHeader[0];
         if (isPCX != 10)
         {
            return new Bitmap(0, 0);
         }
         byte version = fileHeader[1];
         bitsPerPixel = BitConverter.ToChar(fileHeader, 3);
         int xMin = BitConverter.ToInt16(fileHeader, 4);
         int yMin = BitConverter.ToInt16(fileHeader, 6);
         int xMax = BitConverter.ToInt16(fileHeader, 8);
         int yMax = BitConverter.ToInt16(fileHeader, 10);
         hDPI = BitConverter.ToInt16(fileHeader, 12);
         vDPI = BitConverter.ToInt16(fileHeader, 14);
         byte[] palette = new byte[768];
         //palette in header
         if (version < 5)
         {
            for (int i = 0; i < 48; i++)
            {
               palette[i] = fileHeader[i + 16];
            }
         }
         //palette at end
         if (version > 4)
         {
            fs.Seek(-768, SeekOrigin.End);
            for (int i = 0; i < 768; i++)
            {
               palette[i] = (byte)fs.ReadByte();
            }
            fs.Seek(128, SeekOrigin.Begin);
         }
         byte nPlanes = fileHeader[65];
         int bytesPerLine = BitConverter.ToInt16(fileHeader, 66);


         xSize = xMax - xMin + 1;
         ySize = yMax - yMin + 1;
         int totalBytesPerLine = nPlanes * bytesPerLine;
         //***
         int linePaddingSize = ((bytesPerLine * nPlanes) *
                                          (8 / bitsPerPixel)) -
                                          ((xMax - xMin) + 1);

         byte[] scanLine = new byte[totalBytesPerLine];
         byte nRepeat;
         byte pColor;
         int pIndex = 0;
         byte[] imageBytes = new byte[totalBytesPerLine * ySize];

         for (int iY = 0; iY < ySize; iY++)
         {
            int iX = 0;
            while (iX < totalBytesPerLine)
            {
               nRepeat = (byte)fs.ReadByte();
               if (nRepeat > 192)
               {
                  nRepeat -= 192;
                  pColor = (byte)fs.ReadByte();
                  for (int j = 0; j < nRepeat; j++)
                  {
                     if (iX < scanLine.Length)
                     {
                        scanLine[iX] = pColor;
                        imageBytes[pIndex] = pColor;
                     }
                     iX++;
                     pIndex++;
                  }
               }
               else
               {
                  if (iX < scanLine.Length)
                  {
                     scanLine[iX] = nRepeat;
                     imageBytes[pIndex] = nRepeat;
                  }
                  iX++;
                  pIndex++;
               }
            }
         }

         fs.Close();

         //pixel format standard 4bpp
         PixelFormat pf = PixelFormat.Format8bppIndexed;
         //monochrome
         if (bitsPerPixel == 1 && nPlanes == 1)
         {
            pf = PixelFormat.Format1bppIndexed;
         }

         pcxImage = new Bitmap(xSize, ySize, pf);

         if (!(bitsPerPixel == 1 && nPlanes == 1))
         {
            ColorPalette cPal = pcxImage.Palette;
            int palPos = 0;
            if (version > 4)
            {
               for (int i = 0; i < 256; i++)
               {
                  cPal.Entries[i] = Color.FromArgb(
                            palette[palPos],
                            palette[palPos + 1],
                            palette[palPos + 2]);
                  palPos += 3;
               }
            }
            else
            {
               for (int i = 0; i < 16; i++)
               {
                  cPal.Entries[i] = Color.FromArgb(
                            palette[palPos],
                            palette[palPos + 1],
                            palette[palPos + 2]);
                  palPos += 3;
               }
            }
            pcxImage.Palette = cPal;
         }

         //Create a BitmapData and Lock all pixels to be written
         BitmapData bmpData = pcxImage.LockBits(
             new Rectangle(0, 0, pcxImage.Width,
                                   pcxImage.Height),
                                   ImageLockMode.WriteOnly,
                                   pcxImage.PixelFormat);
         try
         {
            //Copy the data from the byte array into 
            //BitmapData.Scan0
            Marshal.Copy(imageBytes, 0, bmpData.Scan0,
                                imageBytes.Length);
         }
         catch { }

         //Unlock the pixels
         pcxImage.UnlockBits(bmpData);

         return pcxImage;
      }
   }
}