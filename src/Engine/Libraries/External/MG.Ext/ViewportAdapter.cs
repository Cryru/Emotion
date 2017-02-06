using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    public abstract class ViewportAdapter
    {
        public abstract Matrix GetScaleMatrix();







        public virtual void Reset() { }
    }
}