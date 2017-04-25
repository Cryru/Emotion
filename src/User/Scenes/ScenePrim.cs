using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Events;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public class ScenePrim : Scene
    {
        #region "Declarations"

        #endregion
        public override void Start()
        {
            GameObject Player = new GameObject();
            Player.Width = 32;
            Player.Height = 32;
            //Player.CenterObject();
            Player.AddComponent(new ActiveTexture(AssetManager.BlankTexture));

            Player.Layer = Enums.ObjectLayer.World;

            //TargetLocation = Player.Position;

            AddObject("Player", Player);
        }

        private Vector2 TargetLocation;
        private Vector2 StartLocation;
        private float LocationProgress;
        private bool MovementInput
        {
            get
            {
                if (StartLocation == TargetLocation) return true;
                if (LocationProgress > 0.6)
                    return true;
                return false;
            }
        }

        private Vector2 PlayerActualPosition
        {
            get
            {
                if (LocationProgress > 0.5) return TargetLocation; else return StartLocation;
            }
        }

        public override void Update()
        {
            //Process player input.
            ProcessInput();
            //Process movement.
            ProcessMovement();

            // GetObject("Player").X -= (int) (0.7 * Context.Core.frameTime);
            //Debugging.DebugScene.debugText = GetObject("Player").Position.ToString() + GetObject("Player").X % 16 + GetObject("Player").Y % 16;

        }

        private void ProcessInput()
        {
            if (!MovementInput) return;

            int XAxis = 0;
            int YAxis = 0;

            //X Axis
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                XAxis -= 1;
            }
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                XAxis += 1;
            }

            //Y Axis
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                YAxis -= 1;
            }
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                YAxis += 1;
            }

            //Calculate target tile from axis.
            Vector2 Player = GetObject("Player").Position;
            StartLocation = StartLocation == TargetLocation ? PlayerActualPosition : TargetLocation;
            TargetLocation = new Vector2(PlayerActualPosition.X + (16 * XAxis), PlayerActualPosition.Y + (16 * YAxis));
            
            LocationProgress = 0f;
        }

        private void ProcessMovement()
        {
            Vector2 Player = GetObject("Player").Position;

            if(Player != TargetLocation)
            {
                int newX = (int)MathHelper.SmoothStep(StartLocation.X, TargetLocation.X, LocationProgress);
                int newY = (int)MathHelper.SmoothStep(StartLocation.Y, TargetLocation.Y, LocationProgress);

                Console.WriteLine(newX - GetObject("Player").X);
                Console.WriteLine(newY - GetObject("Player").Y);

                GetObject("Player").X = newX;
                GetObject("Player").Y = newY;
                LocationProgress += 0.3f;
                LocationProgress = Math.Min(1, LocationProgress);
            }
        }
    }
}
