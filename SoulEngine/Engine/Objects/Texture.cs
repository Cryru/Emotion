using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raya.Graphics.Primitives;
using Soul.Engine.ECS;

namespace Soul.Engine.Objects
{
    public class Texture : Actor
    {
        public Rectangle Source;
        public Rectangle Destination;

        private Raya.Graphics.Texture _nativeTexture;

        public override void Initialize()
        {

        }

        public override void Update()
        {
            
        }
    }
}
