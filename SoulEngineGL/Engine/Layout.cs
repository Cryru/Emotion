using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.ECS;

namespace Soul.Engine
{
    public static class Layout
    {
        /// <summary>
        /// Centers an entity on the screen.
        /// </summary>
        /// <param name="entity">The entity to center.</param>
        public static void CenterEntity(Entity entity)
        {
            entity.X = Settings.Width / 2 - entity.Width / 2;
            entity.Y = Settings.Height / 2 - entity.Height / 2;
        }

        /// <summary>
        /// Makes the entity take up the whole screen.
        /// </summary>
        /// <param name="entity"></param>
        public static void FullscreenEntity(Entity entity)
        {
            entity.X = 0;
            entity.Y = 0;
            entity.Width = Settings.Width;
            entity.Height = Settings.Height;
        }
    }
}
