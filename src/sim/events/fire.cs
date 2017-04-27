/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using OpenTK;


using Util;
using Events;


namespace Sim
{
	public class FireEvent : Event
	{
		static EventName theName;

		UInt64 myFiringEntity;
		Vector3 myLocation;
		Vector3 myDirection;
		String myEnergyType;
		float myEnergyAmount;
		String myWeaponType;

		public FireEvent(): base() { myName=theName; }
		public FireEvent(UInt64 firingEntity, Vector3 location, Vector3 direction, String energyType, float energyAmount, String weaponType) : this(firingEntity, location, direction, energyType, energyAmount, weaponType, TimeSource.defaultClock.currentTime(), 0.0) { }
		public FireEvent(UInt64 firingEntity, Vector3 location, Vector3 direction, String energyType, float energyAmount, String weaponType, double timeStamp) : this(firingEntity, location, direction, energyType, energyAmount, weaponType, timeStamp, 0.0) { }
		public FireEvent(UInt64 firingEntity, Vector3 location, Vector3 direction, String energyType, float energyAmount, String weaponType, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myFiringEntity=firingEntity;
			myLocation=location;
			myDirection=direction;
			myEnergyType=energyType;
			myEnergyAmount=energyAmount;
			myWeaponType=weaponType;
		}

		static FireEvent()
		{
			theName = new EventName("entity.fire");
			
		}


		public UInt64 firingEntity
		{
			get { return myFiringEntity;}
		}
	
		public Vector3 location
		{
			get { return myLocation;}
		}
	
		public Vector3 direction
		{
			get { return myDirection;}
		}
	
		public String energyType
		{
			get { return myEnergyType;}
		}
	
		public float energyAmount
		{
			get { return myEnergyAmount;}
		}
	
		public String weaponType
		{
			get { return myWeaponType;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(float)*3;
			size+=sizeof(float)*3;
			size+=System.Text.Encoding.Unicode.GetByteCount(myEnergyType) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myEnergyType);
			size+=sizeof(float);
			size+=System.Text.Encoding.Unicode.GetByteCount(myWeaponType) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myWeaponType);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myFiringEntity);
		writer.Write(myLocation.X);
		writer.Write(myLocation.Y);
		writer.Write(myLocation.Z);
	
		writer.Write(myDirection.X);
		writer.Write(myDirection.Y);
		writer.Write(myDirection.Z);
	
			writer.Write(myEnergyType);
			writer.Write(myEnergyAmount);
			writer.Write(myWeaponType);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myFiringEntity=reader.ReadUInt64();
		myLocation.X=reader.ReadSingle();
		myLocation.Y=reader.ReadSingle();
		myLocation.Z=reader.ReadSingle();
	
		myDirection.X=reader.ReadSingle();
		myDirection.Y=reader.ReadSingle();
		myDirection.Z=reader.ReadSingle();
	
			myEnergyType=reader.ReadString();
			myEnergyAmount=reader.ReadSingle();
			myWeaponType=reader.ReadString();
		}

	#endregion
	

	}
	
}

	