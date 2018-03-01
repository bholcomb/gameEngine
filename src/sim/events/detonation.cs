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
	public class DetonationEvent : Event
	{
		static EventName theName;

		UInt64 myFiringEntity;
		Vector3 myLocation;
		UInt64 myReceiver;
		String myEnergyType;
		float myEnergyAmount;
		String myWeaponType;

		public DetonationEvent(): base() { myName=theName; }
		public DetonationEvent(UInt64 firingEntity, Vector3 location, UInt64 receiver, String energyType, float energyAmount, String weaponType) : this(firingEntity, location, receiver, energyType, energyAmount, weaponType, TimeSource.defaultClock.currentTime(), 0.0) { }
		public DetonationEvent(UInt64 firingEntity, Vector3 location, UInt64 receiver, String energyType, float energyAmount, String weaponType, double timeStamp) : this(firingEntity, location, receiver, energyType, energyAmount, weaponType, timeStamp, 0.0) { }
		public DetonationEvent(UInt64 firingEntity, Vector3 location, UInt64 receiver, String energyType, float energyAmount, String weaponType, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myFiringEntity=firingEntity;
			myLocation=location;
			myReceiver=receiver;
			myEnergyType=energyType;
			myEnergyAmount=energyAmount;
			myWeaponType=weaponType;
		}

		static DetonationEvent()
		{
			theName = new EventName("entity.detonation");
			
		}


		public UInt64 firingEntity
		{
			get { return myFiringEntity;}
		}
	
		public Vector3 location
		{
			get { return myLocation;}
		}
	
		public UInt64 receiver
		{
			get { return myReceiver;}
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
			size+=sizeof(UInt64);
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
	
			writer.Write(myReceiver);
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
	
			myReceiver=reader.ReadUInt64();
			myEnergyType=reader.ReadString();
			myEnergyAmount=reader.ReadSingle();
			myWeaponType=reader.ReadString();
		}

	#endregion
	

	}
	
}

	