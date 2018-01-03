// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using OpenTK;

#endregion

namespace Soul.Engine.ECS.Components
{
    public class Transform : ComponentBase
    {
        #region Properties

        /// <summary>
        /// The location of the transform.
        /// </summary>
        public Vector2 Location
        {
            get { return _location; }
            set
            {
                _hasUpdated = true;
                _location = value;
            }
        }

        private Vector2 _location = new Vector2(0, 0);

        /// <summary>
        /// The size of the transform.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _hasUpdated = true;
                _size = value;
            }
        }

        private Vector2 _size = new Vector2(50, 50);


        /// <summary>
        /// The transform's rotation in degrees.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _hasUpdated = true;
                _rotation = value;
            }
        }

        private float _rotation;

        #endregion

        #region Accessors

        public float X
        {
            get { return Location.X; }
            set
            {
                _hasUpdated = true;
                _location.X = value;
            }
        }

        public float Y
        {
            get { return Location.Y; }
            set
            {
                _hasUpdated = true;
                _location.Y = value;
            }
        }

        public float Width
        {
            get { return Size.X; }
            set
            {
                _hasUpdated = true;
                _size.X = value;
            }
        }

        public float Height
        {
            get { return Size.Y; }
            set
            {
                _hasUpdated = true;
                _size.Y = value;
            }
        }

        /// <summary>
        /// The center of the transform.
        /// </summary>
        public Vector2 Center
        {
            get { return new Vector2(X + Width / 2, Y + Height / 2); }
            set
            {
                // Transform the location.
                value.X -= Width / 2;
                value.Y -= Height / 2;

                // Set the position.
                Location = value;
            }
        }

        #endregion

        #region Trackers

        /// <summary>
        /// Whether the transform has updated since the last time this was called.
        /// </summary>
        public bool HasUpdated
        {
            get
            {
                // If it hasn't been updated return false.
                if (!_hasUpdated) return false;
                // If it has then set the flag to false and return true.
                _hasUpdated = false;
                return true;
            }
        }

        private bool _hasUpdated = true;

        #endregion
    }
}