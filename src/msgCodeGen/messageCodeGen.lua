--code generator for taking a message format (as a table) and generating the event code and net message serialization/deserialization for given events
--handles OpenTK types as well as atomic types
--also handles creating serialization functions

--message code parts
createEvent=false
createNetwork=false
Definitions=nil
input=""
outputDir="./"

templates={
	file=[[
/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

USING

using Util;
using Events;


namespace NAMESPACE
{
CLASS_DEF
}

	]],

	class=[[
	public class EVENTNAME : Event
	{
		static EventName theName;
ATTRIBUTES

		public EVENTNAME(): base() { myName=theName; }
		public EVENTNAME(PARAM_DEF) : this(PARAM_VAL, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EVENTNAME(PARAM_DEF, double timeStamp) : this(PARAM_VAL, timeStamp, 0.0) { }
		public EVENTNAME(PARAM_DEF, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
PARAM_SET
		}

		static EVENTNAME()
		{
			theName = new EventName("EVENTSTRINGNAME");
			REGISTER_ENTITY_ATTRIBUTE_CHANGE
		}

ATTRIBUTE_ACCESSOR

DISPATCH_CHANGE

ADDITIONAL_CODE

NETWORK

	}
	]],

	dispatchChange=
	[[
	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		EVENTNAME evt=new EVENTNAME(e.id, (PARAM)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	]],

	network=[[
	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

ATTRIBUTE_SIZES

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

ATTRIBUTE_WRITERS
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

ATTRIBUTE_READERS
		}

	#endregion
	]],

	using="using LIB;",
	attributeNoNew="		ATTRIBUTE_TYPE myATTRIBUTE_NAME;",
	attributeNew="		ATTRIBUTE_TYPE myATTRIBUTE_NAME=new ATTRIBUTE_TYPE();",
	paramDef="PARAM_TYPE PARAM_NAME",
	paramVal="PARAM_NAME",
	paramSet="			myPARAM_NAME=PARAM_VALUE;",

	attributeAccessor=[[

		public ATTRIBUTE_TYPE ATTRIBUTE_ACCESS
		{
			get { return myATTRIBUTE_NAME;}
		}
	]],
}

dataTypes={}
dataTypes["String"]={
				    size=[[
			size+=System.Text.Encoding.Unicode.GetByteCount(VAR) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(VAR);]],
				    reader="			VAR=reader.ReadString();",
					writer="			writer.Write(VAR);"}
dataTypes["bool"]={
				    size="			size+=sizeof(bool);",
					reader="			VAR=reader.ReadBoolean();",
					writer="			writer.Write(VAR);"}
dataTypes["byte"]={
					size="			size+=sizeof(byte);",
					reader="			VAR=reader.ReadByte();",
					writer="			writer.Write(VAR);"}
dataTypes["sbyte"]={
					size="			size+=sizeof(sbyte);",
					reader="			VAR=reader.ReadSByte();",
					writer="			writer.Write(VAR);"}
dataTypes["char"]={
					size="			size+=sizeof(char);",
					reader="			VAR=reader.ReadChar();",
					writer="			writer.Write(VAR)'"}
dataTypes["decimal"]={
					size="			size+=sizeof(decimal);",
					reader=			"VAR=reader.ReadDecimal();",
					writer=			"writer.Write(VAR);"}
dataTypes["double"]={
					size="			size+=sizeof(double);",
					reader="			VAR=reader.ReadDouble();",
					writer="			writer.Write(VAR);"}
dataTypes["float"]={
					size="			size+=sizeof(float);",
					reader="			VAR=reader.ReadSingle();",
					writer="			writer.Write(VAR);"}
dataTypes["Int16"]={
					size="			size+=sizeof(Int16);",
					reader="			VAR=reader.ReadInt16();",
					writer="			writer.Write(VAR);"}
dataTypes["Int32"]={
					size="			size+=sizeof(Int32);",
					reader="			VAR=reader.ReadInt32();",
					writer="			writer.Write(VAR);"}
dataTypes["Int64"]={
					size="			size+=sizeof(Int64);",
					reader="			VAR=reader.ReadInt64();",
					writer="			writer.Write(VAR);"}
dataTypes["UInt16"]={
					size="			size+=sizeof(UInt16);",
					reader="			VAR=reader.ReadUInt16();",
					writer="			writer.Write(VAR);"}
dataTypes["UInt32"]={
					size="			size+=sizeof(UInt32);",
					reader="			VAR=reader.ReadUInt32();",
					writer="			writer.Write(VAR);"}
dataTypes["UInt64"]={
					size="			size+=sizeof(UInt64);",
					reader="			VAR=reader.ReadUInt64();",
					writer="			writer.Write(VAR);"}


dataTypes["Vector2"]={
	size="			size+=sizeof(float)*2;",
	reader=[[
		VAR.X=reader.ReadSingle();
		VAR.Y=reader.ReadSingle();
	]],
	writer=[[
		writer.Write(VAR.X);
		writer.Write(VAR.Y);
	]], 
	listSize="sizeof(float)*2"
}
dataTypes["Vector3"]={
	size="			size+=sizeof(float)*3;",
	reader=[[
		VAR.X=reader.ReadSingle();
		VAR.Y=reader.ReadSingle();
		VAR.Z=reader.ReadSingle();
	]],
	writer=[[
		writer.Write(VAR.X);
		writer.Write(VAR.Y);
		writer.Write(VAR.Z);
	]], 
	listSize="sizeof(float)*3"
}
dataTypes["Vector4"]={
	size="			size+=sizeof(float)*4;",
	reader=[[
		VAR.X=reader.ReadSingle();
		VAR.Y=reader.ReadSingle();
		VAR.Z=reader.ReadSingle();
		VAR.W=reader.ReadSingle();
	]],
	writer=[[
		writer.Write(VAR.X);
		writer.Write(VAR.Y);
		writer.Write(VAR.Z);
		writer.Write(VAR.W);
	]], 
	listSize="sizeof(float)*4"
}
dataTypes["Quaternion"]={
	size="			size+=sizeof(float)*4;",
	reader=[[
		VAR.X=reader.ReadSingle();
		VAR.Y=reader.ReadSingle();
		VAR.Z=reader.ReadSingle();
		VAR.W=reader.ReadSingle();
	]],
	writer=[[
		writer.Write(VAR.X);
		writer.Write(VAR.Y);
		writer.Write(VAR.Z);
		writer.Write(VAR.W);
	]], 
	listSize="sizeof(float)*4"
}
dataTypes["NodeLocation"]={
	new=true;
	size="			size+=sizeof(UInt32)*3;",
	reader=[[
		VAR.nx=reader.ReadUInt32();
		VAR.ny=reader.ReadUInt32();
		VAR.nz=reader.ReadUInt32();
	]],
	writer=[[
		writer.Write(VAR.nx);
		writer.Write(VAR.ny);
		writer.Write(VAR.nz);
	]], 
	listSize="sizeof(UInt32)*3"
}
dataTypes["KeyModifier"]={
	new=true;
	size="			size+=sizeof(Int32);";
	reader="			VAR.modifiers=reader.ReadInt32();";
	writer="			writer.Write(VAR.modifiers);";
}

function needsNew(v)
	local dt=dataTypes[v.type]
	if(dt~=nil) then
		return dt.new==true
	end

	if(v.type:find("List")~=nil) then
		return true;
	end

	print("Unknown type: "..v.type)
end

function generateSize(v)
	--check if it is simple/known data type
	if(dataTypes[v.type]~=nil) then
		local ret=dataTypes[v.type].size
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "VAR", "my"..upperName)
		return ret
	end

	--check if it was a list of one of the basic types
	if(v.type:find("List")~=nil) then
		local ret="			size+=4; //for the count of the items in the list\n"
		local start, stop, listType=v.type:find('<(.+)>')
		assert(dataTypes[listType]~=nil)
		if(dataTypes[listType].listSize~=nil) then
			ret=ret.."			size+=myATTRIBUTE_NAME.Count * "..dataTypes[listType].listSize..";\n"
		else
			ret=ret.."			size+=myATTRIBUTE_NAME.Count * sizeof("..listType..");\n"
		end

		--do the name substitution
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "VAR", "my"..upperName)
		ret=string.gsub(ret, "ATTRIBUTE_NAME", upperName)

		return ret
	end

	print("Unknown type: "..v.type)
end

function generateWriter(v)
	--check if it is simple/known data type
	if(dataTypes[v.type]~=nil) then
		local ret= dataTypes[v.type].writer

		--do the name substitution
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "VAR", "my"..upperName)

		return ret
	end

	--check if it was a list of one of the basic types
	if(v.type:find("List")~=nil) then
		local ret="			writer.Write(myATTRIBUTE_NAME.Count); //for the count of the items in the list\n"
		local start, stop, listType=v.type:find("<(.+)>")
		assert(dataTypes[listType]~=nil)
		ret=ret.."			for(int i=0; i<myATTRIBUTE_NAME.Count; i++)\n"
		ret=ret.."			{\n"

		local writeLine=dataTypes[listType].writer
		writeLine=writeLine:gsub("VAR", "myATTRIBUTE_NAME[i]")
		ret=ret.."				"..writeLine.."\n"
		ret=ret.."			}\n"

		--do the name substitution
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "ATTRIBUTE_NAME", upperName)

		return ret
	end

	print("Unknown type: "..v.type)
end

function generateReader(v)
	--check if it is simple/known data type
	if(dataTypes[v.type]~=nil) then
		local ret= dataTypes[v.type].reader

		--do the name substitution
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "VAR", "my"..upperName)

		return ret
	end

	--check if it was a list of one of the basic types
	if(v.type:find("List")~=nil) then
		local ret="			int myATTRIBUTE_NAME_count=reader.ReadInt32(); //for the count of the items in the list\n"
		local start, stop, listType=v.type:find("<(.+)>")
		assert(dataTypes[listType]~=nil)
		ret=ret.."			for(int i=0; i<myATTRIBUTE_NAME_count; i++)\n"
		ret=ret.."			{\n"
		ret=ret.."				"..listType.." a"..listType.."=new "..listType.."();\n"

		local readLine=dataTypes[listType].reader
		readLine=readLine:gsub("VAR", "a"..listType)
		ret=ret.."				"..readLine.."\n"
		ret=ret.."				myATTRIBUTE_NAME.Add(a"..listType..");\n"
		ret=ret.."			}"

		--do the name substitution
		local upperName= v.name:gsub("^%l", string.upper)
		ret=string.gsub(ret, "ATTRIBUTE_NAME", upperName)

		return ret
	end

	print("Unknown type: "..v.type)
end

function EVENT(def)
	--open the file
	local filename=def.fileName..".cs"
	filename=outputDir..filename
	print("Writing file: "..filename)
	local f = assert(io.open(filename, "w"))

	temp=templates.file
	temp=string.gsub(temp, "NAMESPACE", def.namespace)
	
	classDef=templates.class

	classDef=string.gsub(classDef, "EVENTNAME", def.className)
	classDef=string.gsub(classDef, "EVENTSTRINGNAME", def.eventName)
	

	--generate code for any additional libraries
	local usings=""
	if(def.libs~=nil) then
		for k,v in pairs(def.libs) do
			local lib=templates.using
			lib=string.gsub(lib, "LIB", v)
			usings=usings..lib.."\n"
		end
	end
	temp=string.gsub(temp, "USING", usings)

	--generate code for the attributes definitions
	local attribs=""
	if(def.attributes~=nil) then
		for k,v in pairs(def.attributes) do
			local att=""
			if(needsNew(v)==true) then 
				att=templates.attributeNew 
			else 
				att=templates.attributeNoNew 
			end
			att=string.gsub(att, "ATTRIBUTE_TYPE", v.type)
			local upperName= v.name:gsub("^%l", string.upper)
			att=string.gsub(att, "ATTRIBUTE_NAME", upperName)
			attribs=attribs.."\n"..att
		end
	end
	classDef=string.gsub(classDef, "ATTRIBUTES", attribs)

	--if this is an entity attribute change message, then add registration code
	if(def.attributeChange==true) then
		classDef=string.gsub(classDef, "REGISTER_ENTITY_ATTRIBUTE_CHANGE", "Entity.registerDispatcher(theName.myName, dispatchAttributeChange);")
		local disp=templates.dispatchChange
		disp=string.gsub(disp, "EVENTNAME", def.className)
		disp=string.gsub(disp, "PARAM", def.attributes[2].type)
		classDef=string.gsub(classDef, "DISPATCH_CHANGE", disp)
	else
		classDef=string.gsub(classDef, "REGISTER_ENTITY_ATTRIBUTE_CHANGE", "")
		classDef=string.gsub(classDef, "DISPATCH_CHANGE", "")
	end

	--generate code for the attributes within the constructor definitions
	local paramDefs=""
	if(def.attributes~=nil) then
		for k,v in pairs(def.attributes) do
			local pdef=templates.paramDef
			pdef=string.gsub(pdef, "PARAM_TYPE", v.type)
			pdef=string.gsub(pdef, "PARAM_NAME", v.name)
			paramDefs= paramDefs..pdef
			if(k~=#def.attributes) then
				paramDefs=paramDefs..", "
			end
		end
	end
	classDef=string.gsub(classDef, "PARAM_DEF", paramDefs)

	--generate code for the attributes within the constructor definitions
	local paramVals=""
	if(def.attributes~=nil) then
		for k,v in pairs(def.attributes) do
			local pval=templates.paramVal
			pval=string.gsub(pval, "PARAM_NAME", v.name)
			paramVals= paramVals..pval
			if(k~=#def.attributes) then
				paramVals=paramVals..", "
			end
		end
	end
	classDef=string.gsub(classDef, "PARAM_VAL", paramVals)

	--generate code for all the setting of parameters within the constructor
	local paramSets=""
	if(def.attributes~=nil) then
		for k,v in pairs(def.attributes) do
			local pset=templates.paramSet
			local upperName= v.name:gsub("^%l", string.upper)
			pset=string.gsub(pset, "PARAM_NAME", upperName)
			pset=string.gsub(pset, "PARAM_VALUE", v.name)
			paramSets= paramSets..pset
			if(k~=#def.attributes) then
				paramSets=paramSets.."\n"
			end
		end
	end
	classDef=string.gsub(classDef, "PARAM_SET", paramSets)

	--generate code for all the accessors
	local attribAccessors=""
	if(def.attributes~=nil) then
		for k,v in pairs(def.attributes) do
			local att=templates.attributeAccessor
			att=string.gsub(att, "ATTRIBUTE_TYPE", v.type)
			att=string.gsub(att, "ATTRIBUTE_ACCESS", v.name)
			local upperName= v.name:gsub("^%l", string.upper)
			att=string.gsub(att, "ATTRIBUTE_NAME", upperName)
			attribAccessors=attribAccessors..att
		end
	end
	classDef=string.gsub(classDef, "ATTRIBUTE_ACCESSOR", attribAccessors)

	--add any additional code if in the definition file
	if(def.additionalCode~=nil and def.additionalCode~="") then
		classDef=string.gsub(classDef, "ADDITIONAL_CODE", def.additionalCode)
	else
		classDef=string.gsub(classDef, "ADDITIONAL_CODE", "")
	end

	--add the serialization code if its enabled
	if(def.serialize==true) then
		classDef=string.gsub(classDef, "NETWORK", templates.network)
		--generate code for the message size
		local attSizes=""
		if(def.attributes~=nil) then
			for k,v in pairs(def.attributes) do
				local sval=generateSize(v)
				if(sval==nil) then print(v.name) end
				attSizes= attSizes..sval
				if(k~=#def.attributes) then
					attSizes=attSizes.."\n"
				end
			end
		end
		classDef=string.gsub(classDef, "ATTRIBUTE_SIZES", attSizes)

		--generate code for the message serialization
		local attWriter=""
		if(def.attributes~=nil) then
			for k,v in pairs(def.attributes) do
				local wval=generateWriter(v)
				attWriter= attWriter..wval
				if(k~=#def.attributes) then
					attWriter=attWriter.."\n"
				end
			end
		end
		classDef=string.gsub(classDef, "ATTRIBUTE_WRITERS", attWriter)

		--generate code for the message deserialization
		local attReader=""
		if(def.attributes~=nil) then
			for k,v in pairs(def.attributes) do
				local rval=generateReader(v)
				attReader= attReader..rval
				if(k~=#def.attributes) then
					attReader=attReader.."\n"
				end
			end
		end
		classDef=string.gsub(classDef, "ATTRIBUTE_READERS", attReader)
	else
		classDef=string.gsub(classDef, "NETWORK", "")
	end

	--put the generated code in the file definition
	temp=string.gsub(temp, "CLASS_DEF", classDef)

	--write the code
	f:write(temp)

	--close the file
	f:close()
end

for  i=1,#arg do
	if(arg[i]=='-i') then
		input=arg[i+1]
		print("generating code for: "..input)
	end
	if(arg[i]=='-o') then
		outputDir=arg[i+1]
      if(outputDir:sub(-1)~="/") then
         outputDir=outputDir.."/"
      end
		print("output directory: "..outputDir)
	end
end

dofile(input)
