using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;

namespace Examples.Systems
{
    public class TestSystem : SystemBase
    {
        protected override void Setup()
        {
            
        }

        protected override void Update(Entity link)
        {
            
        }

        protected override Type[] GetRequirements()
        {
            return new[] { typeof(RenderData), typeof(Transform) };
        }
    }
}
