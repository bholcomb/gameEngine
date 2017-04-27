ENTITY
{
   type="player";
   inherits="base";

   attributes=
   {
		type={type="string", value="player"};
		airborne={type="bool", value=false};
		viewOri={type="quaternion"}
   };

	behaviors=
	{
		KinematicBehavior=
		{
			mass=10.0;
			stepSize=5.0;
			runStepSize=10.0;
			jumpImpulse=1.0;
			stepHeight=1.0;
			maxSlope=60.0;
		};

		UserInputBehavior=
		{
			left="a";
			right="d";
			forward="w";
			back="s";
			jump="space";
			run="leftShift";
			fly="tab";
			turnLeft="q";
			turnRight="e";
			invertMouse=false;
		};

		CameraBehavior=
		{
			offset={x=0, y=1, z=2};
		};

		ListenerBehavior=
		{
		};

		RenderBehavior=
		{
			modelDefinition="../data/models/characters/zombie/zombie.json";
		}
	};

   reflected=
   {
   };
}
