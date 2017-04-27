using System;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;

namespace Util
{

   public class DllLoader
   {
      IntPtr myDllPtr { get; set; }

      public DllLoader(String dllName)
      {
         loadDll(dllName);
      }

      /// <summary>
      /// Loads a dll into process memory.
      /// </summary>
      /// <param name="lpFileName">Filename to load.</param>
      /// <returns>Pointer to the loaded library.</returns>
      /// <remarks>
      /// This method is used to load a dll into memory, before calling any of it's DllImported methods.
      /// 
      /// This is done to allow loading an x86 version of a dll for an x86 process, or an x64 version of it
      /// for an x64 process.
      /// </remarks>
      [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
      public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]String lpFileName);

      /// <summary>
      /// Frees a previously loaded dll, from process memory.
      /// </summary>
      /// <param name="hModule">Pointer to the previously loaded library (This pointer comes from a call to LoadLibrary).</param>
      /// <returns>Returns true if the library was successfully freed.</returns>
      [DllImport("kernel32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool FreeLibrary(IntPtr hModule);

      /// <summary>
      /// This method is used to load the dll into memory, before calling any of it's DllImported methods.
      /// 
      /// This is done to allow loading an x86 or x64 version of the dll depending on the process
      /// </summary>
      private void loadDll(String dllName)
      {
         if (myDllPtr == IntPtr.Zero)
         {
            // Retrieve the folder of the OculusWrap.dll.
            string executingAssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string subfolder;

            if (Environment.Is64BitProcess)
               subfolder = "x64";
            else
               subfolder = "x32";

            string filename = Path.Combine(executingAssemblyFolder, subfolder, dllName);

            // Check that the dll file exists.
            bool exists = File.Exists(filename);
            if (!exists)
               throw new DllNotFoundException("Unable to load the file \"" + filename + "\", the file wasn't found.");

            myDllPtr = LoadLibrary(filename);
            if (myDllPtr == IntPtr.Zero)
            {
               int win32Error = Marshal.GetLastWin32Error();
               throw new Win32Exception(win32Error, "Unable to load the file \"" + filename + "\", LoadLibrary reported error code: " + win32Error + ".");
            }
         }
      }

      /// <summary>
      /// Frees previously loaded dll, from process memory.
      /// </summary>
      private void UnloadDll()
      {
         if (myDllPtr != IntPtr.Zero)
         {
            bool success = FreeLibrary(myDllPtr);
            if (success)
               myDllPtr = IntPtr.Zero;
         }
      }
   };
}