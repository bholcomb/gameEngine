using System;
using System.IO;
using System.Collections.Generic;

namespace Util
{
   public abstract class PrintSink
   {
      public PrintSink(){}
      public abstract void print(String txt);
   }

   public class StdOutSink : PrintSink
   {
      public StdOutSink() { }
      public override void print(String txt)
      {
         System.Console.WriteLine(txt);
      }
   }

#if false
   public class EventPrintSink : PrintSink
   {
      public EventPrintSink() { }
      public override void print(String txt)
      {
         PrintEvent msg = new PrintEvent(txt);
         Kernel.eventManager.queueEvent(msg);
      }
   }
#endif

   public class FilePrintSink : PrintSink, IDisposable
   {
      TextWriter myFileStream=null;
      public FilePrintSink(String filename)
      {
         myFileStream = new StreamWriter(filename);
      }

      public override void print(String txt)
      {
         myFileStream.WriteLine(txt);
         myFileStream.Flush();
      }

      public void Dispose()
      {
         myFileStream.Close();
      }
   }

   public static class Printer
   {
      static VerboseLevel theVerboseLevel = VerboseLevel.Info;
      static Dictionary<String, PrintSink> thePrintSinks = new Dictionary<String, PrintSink>();

      public enum VerboseLevel { None, Error, Warn, Info, Debug };

      static Printer()
      {
         thePrintSinks.Add("stdOut", new StdOutSink());
         //thePrintSinks.Add("eventSink", new EventPrintSink());
      }

      public static void print(String txt)
      {
         foreach(PrintSink p in thePrintSinks.Values)
         {
            p.print(txt);
         }
      }

      public static VerboseLevel verboseLevel
      {
         get { return theVerboseLevel; }
         set { theVerboseLevel = value; }
      }

      public static void addPrintSink(String name, PrintSink sink)
      {
         thePrintSinks.Add(name, sink);
      }

      public static void removePrintSink(String name)
      {
         thePrintSinks.Remove(name);
      }
   }

   public static class Error
   {
      public static void print(String txt, params Object[] objs)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Error)
         {
            txt = "Error: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Red;
            Printer.print(String.Format(txt, objs));
         }
      }

      public static void print(String txt)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Error)
         {
            txt = "Error: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Red;
            Printer.print(txt);
         }
      }
   }

   public static class Debug
   {
      public static void print(String txt, params Object[] objs)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Debug)
         {
            txt = "Debug: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Blue;
            Printer.print(String.Format(txt, objs));
         }
      }

      public static void print(String txt)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Debug)
         {
            txt = "Debug: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Blue;
            Printer.print(txt);
         }
      }
   }

   public static class Info
   {
      public static void print(String txt, params Object[] objs)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Info)
         {
            txt = "Info: " + txt;
            System.Console.ForegroundColor = ConsoleColor.White;
            Printer.print(String.Format(txt, objs));
         };
      }

      public static void print(String txt)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Info)
         {
            txt = "Info: " + txt;
            System.Console.ForegroundColor = ConsoleColor.White;
            Printer.print(txt);
         }
      }
   }

   public static class Warn
   {
      public static void print(String txt, params Object[] objs)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Warn)
         {
            txt = "Warn: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            Printer.print(String.Format(txt, objs));
         }
      }

      public static void print(String txt)
      {
         if (Printer.verboseLevel >= Printer.VerboseLevel.Warn)
         {
            txt = "Warn: " + txt;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            Printer.print(txt);
         }
      }
   }
}