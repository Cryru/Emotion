// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Modules
{
    public static class AssetLoader
    {
        #region Default Assets

        /// <summary>
        /// A blank texture.
        /// </summary>
        public static Texture2D BlankTexture;

        /// <summary>
        /// The default font - Arial.
        /// </summary>
        public static SpriteFont DefaultFont;

        /// <summary>
        /// A texture to be displayed when one couldn't be loaded.
        /// </summary>
        private static Texture2D _missingTexture;

        #endregion

        internal static void Setup()
        {
            // todo: secure mode

            // Load default assets.
            try
            {
                _missingTexture = Core.Context.Content.Load<Texture2D>("Defaults/missing");
                DefaultFont = Core.Context.Content.Load<SpriteFont>("Defaults/font");
                BlankTexture = new Texture2D(Core.Context.GraphicsDevice, 1, 1);
                Color[] data = {Color.White};
                BlankTexture.SetData(data);
                BlankTexture.Name = "Engine/blank";
            }
            catch (Exception)
            {
                ErrorHandling.Raise(DiagnosticMessageType.Assetloader, "Could not load default assets.");
            }
        }

        #region Loading Functions

        public static T LoadAsset<T>(string path)
        {
            return Core.Context.Content.Load<T>(path);
        }

        public static string LoadScript(string path)
        {
            // todo
            return "";
        }

        #endregion
    }
}