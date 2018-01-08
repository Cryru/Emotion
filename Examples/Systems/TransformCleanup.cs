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
    public class TransformCleanup : SystemBase
    {
        protected override void Setup()
        {
            
        }

        protected override void Update(Entity link)
        {
            Transform transform = link.GetComponent<Transform>();

            if (transform.Y > 9999)
            {
                Parent.RemoveEntity(link.Name);
            }
        }

        protected override Type[] GetRequirements()
        {
            return new[] { typeof(Transform) };
        }
    }
}
