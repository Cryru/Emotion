#region Using

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;
using Emotion.Utility;

#endregion

namespace Emotion.Primitives
{
	public class Transform3D : Positional
	{
		/// <summary>
		/// The scale of the transform.
		/// </summary>
		public Vector3 Size
		{
			get => new Vector3(_sizeX, _sizeY, _height);
			set
			{
				if (_sizeX == value.X && _sizeY == value.Y && _height == value.Z)
					return;

				_sizeX = value.X;
				_sizeY = value.Y;
				_height = value.Z;
				Resized();
			}
		}

		/// <summary>
		/// The X axis scale of the transform.
		/// </summary>
		[DontSerialize]
		public float SizeX
		{
			get => _sizeX;
			set
			{
				if (_sizeX == value)
					return;
				_sizeX = value;
				Resized();
			}
		}

		/// <summary>
		/// The Y axis scale of the transform.
		/// </summary>
		[DontSerialize]
		public float SizeY
		{
			get => _sizeY;
			set
			{
				if (_sizeY == value)
					return;
				_sizeY = value;
				Resized();
			}
		}

		/// <summary>
		/// The Z axis scale of the transform.
		/// </summary>
		[DontSerialize]
		public float Height
		{
			get => _height;
			set
			{
				if (_height == value)
					return;
				Resized();
			}
		}

		protected float _sizeX;
		protected float _sizeY;
		protected float _height;

		public Vector3 Rotation
		{
			get => _rotationRad;
			set
			{
				if (_rotationRad == value) return;
				_rotationRad = value;
				Rotated();
			}
		}

		[DontSerialize]
		public Vector3 RotationDeg
		{
			get => new Vector3(Maths.RadiansToDegrees(_rotationRad.X), Maths.RadiansToDegrees(_rotationRad.Y), Maths.RadiansToDegrees(_rotationRad.Z));
			set
			{
				_rotationRad = new Vector3(Maths.DegreesToRadians(value.X), Maths.DegreesToRadians(value.Y), Maths.DegreesToRadians(value.Z));
				Rotated();
			}
		}

		/// <summary>
		/// Is invoked when the transform's size is changed.
		/// </summary>
		public event EventHandler OnResize;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Resized()
		{
			_scaleMatrix = Matrix4x4.CreateScale(_sizeX, _sizeY, _height);

			Debug.Assert(!float.IsNaN(_sizeX));
			Debug.Assert(!float.IsNaN(_sizeY));
			Debug.Assert(!float.IsNaN(_height));
			OnResize?.Invoke(this, EventArgs.Empty);
		}

		protected override void Moved()
		{
			_translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
			base.Moved();
		}

		private Vector3 _rotationRad;

		public event EventHandler OnRotate;

		protected virtual void Rotated()
		{
			_rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotationRad.Y, _rotationRad.X, _rotationRad.Z);
			OnRotate?.Invoke(this, EventArgs.Empty);
		}

		protected Matrix4x4 _rotationMatrix;
		protected Matrix4x4 _scaleMatrix;
		protected Matrix4x4 _translationMatrix;

		public Transform3D()
		{
			_sizeX = 100;
			_sizeY = 100;
			_height = 100;
			Resized();
			Moved();
			Rotated();
		}
	}
}