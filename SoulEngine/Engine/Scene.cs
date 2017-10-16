// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using System.Linq;
using Raya.Primitives;
using Soul.Engine.ECS;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine
{
    /// <summary>
    /// A game scene.
    /// </summary>
    public abstract class Scene : Actor
    {
        /// <summary>
        /// Whether the scene has physics.
        /// </summary>
        public bool HasPhysics = false;

        /// <summary>
        /// Returns an attached game object.
        /// </summary>
        /// <param name="name">The object's name.</param>
        /// <returns>An attached game object.</returns>
        public GameObject GetChild(string name)
        {
            return GetChild<GameObject>(name);
        }

        /// <summary>
        /// Returns the object the mouse is currently over, or null.
        /// </summary>
        /// <returns>The object the mouse is over, or null.</returns>
        public GameObject GetMousedObject()
        {
            // Get mouse location.
            Vector2 mouseLocation = Input.MousePosition;

            return _children.Select(child => child.Value).OfType<GameObject>().FirstOrDefault(currentObject => currentObject.Bounds.IntersectsWith(mouseLocation));
        }
    }
}