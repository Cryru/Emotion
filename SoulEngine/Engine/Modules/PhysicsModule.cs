// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using Raya.Graphics.Primitives;
using Soul.Physics;
using Soul.Physics.Dynamics;

#endregion

namespace Soul.Engine.Modules
{
    public static class PhysicsModule
    {
        #region Properties

        /// <summary>
        /// The physics world for the current scene.
        /// </summary>
        public static World CurrentWorld
        {
            get
            {
                if (SceneManager.CurrentScene == null)
                {
                    return null;
                }

                return GetWorldForScene(SceneManager.CurrentScene);
            }
        }

        private static Dictionary<Scene, World> _physicsWorlds;

        /// <summary>
        /// The scale at which to simulate physics.
        /// </summary>
        public static float Scale = 2;

        /// <summary>
        /// The scale at which to simulate physics in reverse.
        /// </summary>
        public static float ScaleReverse
        {
            get { return 1 / Scale; }
        }

        /// <summary>
        /// The physics gravity.
        /// </summary>
        public static Soul.Physics.Common.Vector2 Gravity = new Soul.Physics.Common.Vector2(0, 1);

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        public static void Start()
        {
            // Define a list of worlds.
            _physicsWorlds = new Dictionary<Scene, World>();
        }

        /// <summary>
        /// Update the module.
        /// </summary>
        public static void Update()
        {
            if (SceneManager.CurrentScene != null && SceneManager.CurrentScene.HasPhysics)
            {
                CurrentWorld.Step(1000 / Core.FrameTime);
            }
        }

        /// <summary>
        /// Returns the physics world for the provided scene.
        /// </summary>
        /// <param name="scene">The scene's world to return.</param>
        /// <returns>The physics world belonging to the provided scene.</returns>
        public static World GetWorldForScene(Scene scene)
        {
            // Check if a world for the scene has been defined already, in which case define it. Return the scene's physics world.
            if (!_physicsWorlds.ContainsKey(scene))
            {
                _physicsWorlds.Add(scene, new World(Gravity));
                Debugger.DebugMessage(Enums.DebugMessageSource.PhysicsModule,
                    "Created new world for scene: " + scene);
            }
                

            return _physicsWorlds[scene];
        }

        #region Helper Functions

        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static float PhysicsToPixel(float num)
        {
            return num * Scale;
        }

        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static Vector2f PhysicsToPixel(Vector2f vec)
        {
            return vec * Scale;
        }

        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static float PixelToPhysics(float num)
        {
            return num * ScaleReverse;
        }

        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static Vector2f PixelToPhysics(Vector2f vec)
        {
            return vec * ScaleReverse;
        }

        #endregion
    }
}