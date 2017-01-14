using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Components
{
    public class Transform : Component
    {
        //The position of the object within the scene.
        #region "Positional"
        /// <summary>
        /// 
        /// </summary>
        float X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Z { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Vector3 PositionFull
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Center
        {
            get
            {
                return new Vector2(X + Width / 2, Y + Height / 2);
            }
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }
        #endregion

        //The size of the box wrapping the object.
        #region "Size"
        /// <summary>
        /// 
        /// </summary>
        float Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        #endregion

        //The rotation of the object.
        #region "Rotation"
        /// <summary>
        /// 
        /// </summary>
        float Rotation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int RotationDegree
        {
            get
            {
                return Soul.Convert.RadiansToDegrees(Rotation);
            }
            set
            {
                Rotation = Soul.Convert.DegreesToRadians(value);
            }
        }
        #endregion

        /// <summary>
        /// The box wrapping the object.
        /// </summary>
        Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }


        #region "Initialization"
        public Transform()
        {
            PositionFull = new Vector3(0, 0, 0);
            Size = new Vector2(100, 100);
        }
        public Transform(Vector3 Position, Vector2 Size)
        {
            PositionFull = Position;
            this.Size = Size;
        }
        public Transform(Vector2 Position, Vector2 Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        public Transform(Vector3 Position)
        {
            PositionFull = Position;
        }
        public Transform(Vector2 Position)
        {
            this.Position = Position;
        }
        #endregion

        #region "Private Helpers"

        #endregion

        #region "Functions"

        #endregion
    }
}
