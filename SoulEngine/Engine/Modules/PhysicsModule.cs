// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using Raya.Primitives;
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
        public static float Scale = 5;

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
        public static Soul.Physics.Common.Vector2 Gravity = new Soul.Physics.Common.Vector2(0, 9.8f);

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        static PhysicsModule()
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
                CurrentWorld.Step(Core.FrameTime / 1000f);
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
        public static Vector2f PhysicsToPixel(Physics.Common.Vector2 vec)
        {
            return new Vector2f(vec.X, vec.Y) * Scale;
        }

        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static Vector2 PhysicsToPixel(float x, float y)
        {
            return new Vector2((int) (x * Scale), (int) (y * Scale));
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
        public static Physics.Common.Vector2 PixelToPhysics(Vector2f vec)
        {
            return new Physics.Common.Vector2(vec.X, vec.Y) * ScaleReverse;
        }

        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static Physics.Common.Vector2 PixelToPhysics(Vector2 vec)
        {
            return new Physics.Common.Vector2(vec.X, vec.Y) * ScaleReverse;
        }
        #endregion
    }
}