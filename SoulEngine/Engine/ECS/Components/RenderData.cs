using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breath.Graphics;

namespace Soul.Engine.ECS.Components
{
    public class RenderData : ComponentBase
    {
        /// <summary>
        /// The Breath drawable object.
        /// </summary>
        internal Polygon DrawableObject;

        /// <summary>
        /// The color of the drawable.
        /// </summary>
        public Color DrawColor = Color.White;

    }
}
