using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	/*
   layout(std140) uniform camera {
      mat4 view;
      mat4 projection;
      mat4 viewProj;
      mat4 ortho;
      vec4 viewVector;
      vec4 eyeLocation;
      float left, right, top, bottom; //aligned 4N
      float zNear, zFar, aspect, fov; //aligned 4N
      int frame;
      float dt;
      float pad1, pad2
   };
    */
	[StructLayout(LayoutKind.Sequential)]
	public struct CameraUniformData
	{
		public Matrix4 ViewMatrix;       //aligned 4N
		public Matrix4 ProjectionMatrix; //aligned 4N
		public Matrix4 ViewProjMatrix;   //aligned 4N
		public Matrix4 OrthoMatrix;      //aligned 4N
		public Vector4 viewVector;       //aligned 4N       
		public Vector4 CameraPosition;   //aligned 4N
		public float left, right, top, bottom; //aligned 4N
		public float zNear, zFar, aspect, fov; //aligned 4N
		public int frame;                //start of a new entry
		public float dt;                 //fits in the second component of that new entry
		public float pad1, pad2;         //padding
	};

	public class Camera
	{
		Viewport myViewport;
		float myFovx;
		float mySinFov;
		float myCosFov;
		float myAspectRatio;
		float myZnear;
		float myZfar;
		public Vector3 myEye;
		public Quaternion myOrientation;

		Matrix4 myViewMatrix;
		Matrix4 myProjMatrix;
		Matrix4 myClipMatrix;

		Plane[] myFrustum = new Plane[6];

		public Vector3 myXAxis;
		public Vector3 myYAxis;
		public Vector3 myZAxis;
		public Vector3 myViewDir;

		public static float theDefaultFovX = 60.0f;
		public static float theDefaultZFar = 1000.0f;
		public static float theDefaultZNear = 0.1f;
		public static Vector3 theDefaultWorldXAxis = new Vector3(1.0f, 0.0f, 0.0f);
		public static Vector3 theDefaultWorldYAxis = new Vector3(0.0f, 1.0f, 0.0f);
		public static Vector3 theDefaultWorldZAxis = new Vector3(0.0f, 0.0f, 1.0f);

		UniformBufferObject myCameraUniformBuffer;
		public CameraUniformData myCameraUniformData;

		bool myIsDirty = true;

		Camera myDebugCamera;

		public Camera(Viewport v) : this(v, theDefaultFovX, theDefaultZNear, theDefaultZFar) { }
		public Camera(Viewport v, float fov, float n, float f, bool debug = false)
		{
			myViewport = v;
			myViewport.notifier += handle_viewportChanged;

			fieldOfView = fov;
			myZnear = n;
			myZfar = f;
			myAspectRatio = (float)myViewport.width / (float)myViewport.height;

			myOrientation = Quaternion.Identity;
			myEye = new Vector3(0.0f, 0.0f, 0.0f);
			myXAxis = new Vector3(1.0f, 0.0f, 0.0f);
			myYAxis = new Vector3(0.0f, 1.0f, 0.0f);
			myZAxis = new Vector3(0.0f, 0.0f, 1.0f);
			myViewDir = new Vector3(0.0f, 0.0f, 1.0f);
			updateViewMatrix();
			setPerspective(fov, (float)v.width / (float)v.height, myZnear, myZfar);

			myCameraUniformBuffer = new UniformBufferObject(BufferUsageHint.DynamicDraw);

			if(debug == false)
				myDebugCamera = new Camera(v, fov, n, f * 2, true); //need to see further than the regular camera
		}

		public int uniformBufferId()
		{
			if (myIsDebugging == true)
				return myDebugCamera.myCameraUniformBuffer.id;

			return myCameraUniformBuffer.id;
		}

		public void bind()
		{
			myViewport.apply();
			if (myIsDebugging == true)
			{
				DebugRenderer.addFrustum(myClipMatrix, Color4.White, false, 0.0);
				Renderer.device.bindUniformBuffer(myDebugCamera.myCameraUniformBuffer.id, 0); //camera is always ubo 0
			}
			else
			{
				Renderer.device.bindUniformBuffer(myCameraUniformBuffer.id, 0); //camera is always ubo 0
			}
		}

		public void unbind()
		{
			myCameraUniformBuffer.unbind();
		}

		public void updateCameraUniformBuffer()
		{
			if (myIsDebugging == true)
				myDebugCamera.updateCameraUniformBuffer();

			if (myIsDirty == true)
			{
				myCameraUniformData.ViewMatrix = viewMatrix();
				myCameraUniformData.ProjectionMatrix = projMatrix();
				myCameraUniformData.ViewProjMatrix = viewProjection();
				myCameraUniformData.OrthoMatrix = orthoMatrix();
				myCameraUniformData.viewVector = new Vector4(viewVector);
				myCameraUniformData.CameraPosition = new Vector4(position);
				myCameraUniformData.top = myZfar * (float)Math.Tan(MathHelper.DegreesToRadians(myFovx) / 2.0f);
				myCameraUniformData.bottom = -myCameraUniformData.top;
				myCameraUniformData.left = myCameraUniformData.bottom * myAspectRatio;
				myCameraUniformData.right = myCameraUniformData.top * myAspectRatio;
				myCameraUniformData.zNear = myZnear;
				myCameraUniformData.zFar = myZfar;
				myCameraUniformData.aspect = myAspectRatio;
				myCameraUniformData.fov = myFovx;
				myCameraUniformData.frame = 0; // Renderer.context.frame;
				myCameraUniformData.dt = 0.0f; // Renderer.context.dt;

				myCameraUniformBuffer.setData(myCameraUniformData);
				myIsDirty = false;
			}
		}

		public Plane[] frustum
		{
			get { return myFrustum; }
		}

		public void handle_viewportChanged(int x, int y, int w, int h)
		{
			setPerspective(fieldOfView, (float)w / (float)h, myZnear, myZfar);
			myAspectRatio = (float)myViewport.width / (float)myViewport.height;
		}

		public void lookAt(Vector3 target)
		{
			lookAt(myEye, target, myYAxis);
		}

		public void lookAt(Vector3 eye, Vector3 target, Vector3 up)
		{
			myEye = eye;

			myZAxis = eye - target;
			myZAxis.Normalize();

			myViewDir = -myZAxis;

			myXAxis = Vector3.Cross(up, myViewDir);
			myXAxis.Normalize();

			myYAxis = Vector3.Cross(myViewDir, myXAxis);
			myYAxis.Normalize();

			myViewMatrix.M11 = myXAxis.X;
			myViewMatrix.M21 = myXAxis.Y;
			myViewMatrix.M31 = myXAxis.Z;
			myViewMatrix.M41 = -Vector3.Dot(myXAxis, eye);

			myViewMatrix.M12 = myYAxis.X;
			myViewMatrix.M22 = myYAxis.Y;
			myViewMatrix.M32 = myYAxis.Z;
			myViewMatrix.M42 = -Vector3.Dot(myYAxis, eye);

			myViewMatrix.M13 = myZAxis.X;
			myViewMatrix.M23 = myZAxis.Y;
			myViewMatrix.M33 = myZAxis.Z;
			myViewMatrix.M43 = -Vector3.Dot(myViewDir, eye);

			myOrientation.fromMatrix(myViewMatrix);

			updateViewMatrix();
		}

		public void move(float dx, float dy, float dz)
		{
			if (myIsDebugging == true)
			{
				myDebugCamera.move(dx, dy, dz);
				return;
			}

			myIsDirty = true;
			Vector3 eye = position;

			eye += myXAxis * dx;
			eye += myYAxis * dy;
			eye += myZAxis * -dz;

			position = eye;

			updateViewMatrix();
		}

		public void move(Vector3 direction, Vector3 amount)
		{
			move(direction.X * amount.X, direction.Y * amount.Y, direction.Z * amount.Z);
		}

		public void setProjection(Matrix4 projection)
		{
			myProjMatrix = projection;


			updateFrustum();
		}

		public void setPerspective(float fovx, float aspect, float znear, float zfar)
		{
			myProjMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovx), aspect, znear, zfar);

			fieldOfView = fovx;
			myZnear = znear;
			myZfar = zfar;

			updateFrustum();
		}

		public void rotate(float headingDegrees, float pitchDegrees, float rollDegrees)
		{
			Quaternion rotX, rotY, rotZ;
			rotX = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(pitchDegrees));
			rotY = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(headingDegrees));
			rotZ = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(rollDegrees));

			Quaternion rot;
			rot = rotX * rotY * rotZ;

			myOrientation *= rot;
			updateViewMatrix();
		}

		public void rotate(Quaternion q)
		{
			myOrientation *= q;
			updateViewMatrix();
		}

		public void setOrientation(Vector3 hpr)
		{
			setOrientation(hpr.X, hpr.Y, hpr.Z);
		}

		public void setOrientation(float headingDegrees, float pitchDegrees, float rollDegrees)
		{
			Quaternion rot = new Quaternion();

			rot = rot.fromHeadingPitchRoll(headingDegrees, pitchDegrees, rollDegrees);
			myOrientation = rot;

			updateViewMatrix();
		}

		public void setOrientation(Quaternion ori)
		{
			if (myIsDebugging == true)
			{
				myDebugCamera.setOrientation(ori);
				return;
			}

			Matrix4 m = new Matrix4();
			ori.setMatrix(ref m);
			myOrientation = ori;

			updateViewMatrix();
		}

		public void updateViewMatrix()
		{
			myOrientation.setMatrix(ref myViewMatrix);

			myXAxis.X = myViewMatrix.M11;
			myXAxis.Y = myViewMatrix.M21;
			myXAxis.Z = myViewMatrix.M31;

			myYAxis.X = myViewMatrix.M12;
			myYAxis.Y = myViewMatrix.M22;
			myYAxis.Z = myViewMatrix.M32;

			myZAxis.X = myViewMatrix.M13;
			myZAxis.Y = myViewMatrix.M23;
			myZAxis.Z = myViewMatrix.M33;

			myXAxis.Normalize();
			myYAxis.Normalize();
			myZAxis.Normalize();

			myViewDir = -myZAxis;

			myViewMatrix.M41 = -Vector3.Dot(myXAxis, myEye);
			myViewMatrix.M42 = -Vector3.Dot(myYAxis, myEye);
			myViewMatrix.M43 = -Vector3.Dot(myZAxis, myEye);
			myViewMatrix.M44 = 1.0f;

			updateFrustum();
		}

		public void updateFrustum()
		{
			Matrix4 oldClipMatrix = myClipMatrix;
			myClipMatrix = myViewMatrix * myProjMatrix;
			myClipMatrix.extractFrustumPlanes(ref myFrustum);

			if (myClipMatrix != oldClipMatrix)
				myIsDirty = true;
		}

		public Viewport viewport()
		{
			return myViewport;
		}

		public Matrix4 viewMatrix()
		{
			return myViewMatrix;
		}

		public Matrix4 projMatrix()
		{
			return myProjMatrix;
		}

		public Matrix4 viewProjection()
		{
			return myViewMatrix * myProjMatrix;
		}

		public Matrix4 orthoMatrix()
		{
			//orthographic matrix with 0,0 in bottom left corner
			return Matrix4.CreateOrthographicOffCenter(0, myViewport.width, 0, myViewport.height, -100, 100);
		}

		public Vector3 viewVector
		{
			get { return myViewDir; }
		}

		public Vector3 position
		{
			get { return myEye; }
			set { myEye = value; updateViewMatrix(); }
		}

		public Vector3 up
		{
			get { return myYAxis; }
		}

		public Vector3 right
		{
			get { return myXAxis; }
		}

		public float near
		{
			get { return myZnear; }
			set { myZnear = value; updateFrustum(); }
		}

		public float far
		{
			get { return myZfar; }
			set { myZfar = value; updateFrustum(); }
		}

		public float fieldOfView
		{
			get { return myFovx; }
			set
			{
				myFovx = value;
				mySinFov = (float)Math.Sin(myFovx);
				myCosFov = (float)Math.Cos(myFovx);
			}
		}

		public float aspectRatio
		{
			get { return myAspectRatio; }
		}

		public Ray getPickRay(int x, int y)
		{
			Vector4 pos = new Vector4();
			int[] viewPort = new int[4];
			GL.GetInteger(GetPName.Viewport, viewPort);


			// Map x and y from window coordinates, map to range -1 to 1 
			pos.X = (x * 2.0f) / viewPort[2] - 1;
			pos.Y = -1 + ((y * 2.0f) / viewPort[3]);
			pos.Z = 0; //because it is on the near plane
			pos.W = 1.0f; //homogeneous coordinate

			//unproject the normalized view coordinate
			Matrix4 unproject = Matrix4.Invert(myClipMatrix);
			Vector4 worldSpace = Vector4.Transform(pos, unproject);
			Vector3 pos_out = new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z) / worldSpace.W;

			//create the ray
			Ray ret = new Ray(myEye, pos_out - myEye);
			return ret;
		}

		public bool containsSphere(Vector3 sphereCenter, float sphereRadius)
		{
			for (int p = 0; p < 6; p++)
			{
				Plane plane = frustum[p];
				float d = plane.A * sphereCenter.X + plane.B * sphereCenter.Y + plane.C * sphereCenter.Z + plane.D;
				if (d <= -sphereRadius)
					return false;
			}

			return true;
		}

		bool myIsDebugging;
		public void toggleDebugging()
		{
			myIsDebugging = !myIsDebugging;

			if(myIsDebugging == true)
			{
            myDebugCamera.position = position;
            myDebugCamera.setOrientation(myOrientation);
			}
		}

		public void debugFrustum()
		{
			DebugRenderer.addFrustum(myClipMatrix, Color4.White, false, 60.0);
		}
	}
}
