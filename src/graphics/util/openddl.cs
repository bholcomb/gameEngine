using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

using Util;

namespace OpenDDL
{
	public class Structure
	{
		public enum DataType { BOOL = 0x01, INT8 = 0x02, INT16 = 0x04, INT32 = 0x08, INT64 = 0x10, UINT8 = 0x20, UINT16 = 0x40, UINT32 = 0x80, UINT64 = 0x100, HALF = 0x200, SINGLE = 0x400, DOUBLE = 0x800, STRING = 0x1000, REF = 0x2000, STRUCT = 0x4000, ARRAY = 0x8000 };
		public DataType dataType;
		public int arrayLength = 0;
		public string identifier = "";
		public string name = "";
		public Dictionary<string, ValueType> properties = new Dictionary<string, ValueType>();

		public Object[] values;
	}

	public class Parser
	{
		Dictionary<String, Structure> globalNames = new Dictionary<string, Structure>();
		Dictionary<String, Structure> localNames = new Dictionary<string, Structure>();

		public Parser()
		{

		}

		public static List<Structure> loadFile(String path)
		{
			if (System.IO.File.Exists(path))
			{
				System.IO.StreamReader file = new System.IO.StreamReader(path);
				String data = file.ReadToEnd();
				file.Close();
				Parser p = new Parser();
				return p.parse(data);
			}

			Warn.print("Cannot find OpenDDL file {0}", path);
			return null;
		}

		List<Structure> parse(String data)
		{
			List<Structure> structs = new List<Structure>();

			Char[] source = data.ToCharArray();
			int i = 0;
			while (i < source.Length)
			{
			}

			return structs;
		}
	}
}