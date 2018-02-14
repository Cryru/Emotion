// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Systems;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using System.Linq;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.ECS.Systems
{
    internal class ComponentUpdateCleanup : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            // All components.
            return null;
        }

        protected internal override void Setup()
        {
            // Set priority to highest so it is run last.
            Priority = 9;
        }

        protected override void Update(Entity entity)
        {
            int componentCount = entity.GetComponentCount();

            for (int i = 0; i < componentCount; i++)
            {
                entity.GetComponent(i).HasUpdated = false;
            }
        }

        #endregion
    }
}