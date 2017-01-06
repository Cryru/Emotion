using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The base for engine objects.
    /// </summary>
    public class ObjectBase
    {
        #region "Variables"
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
        Vector3 Location
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

        /// <summary>
        /// The box wrapping the object.
        /// </summary>
        Rectangle Bounds
        {
            get
            {
                return new Rectangle((int) X, (int) Y, (int) Width, (int) Height);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

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

        //Display properties of the object.
        #region "Display"
        /// <summary>
        /// 
        /// </summary>
        Internal.ActiveTexture Texture { get; set; }

        /// <summary>
        /// 
        /// </summary>
        float Opacity = 1f;

        /// <summary>
        /// 
        /// </summary>
        Color Tint = Color.White;
        #endregion

        //Events of the object.
        #region "Triggers"
        /// <summary>
        /// 
        /// </summary>
        Internal.Trigger onUpdate = new Internal.Trigger();
        /// <summary>
        /// 
        /// </summary>
        Internal.Trigger onDraw = new Internal.Trigger();
        #endregion

        //Ways for the object to link with parental objects.
        #region "Hooks"
        /// <summary>
        /// 
        /// </summary>
        Main.Scene Scene { get; set; }
        #endregion

        //Other
        #region "Others"
        /// <summary>
        /// Tags used to store information within the object.
        /// </summary>
        Dictionary<string, string> Tags = new Dictionary<string, string>();
        #endregion
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Draw()
        {

        }
    }
}
