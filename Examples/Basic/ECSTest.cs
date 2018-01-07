using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examples.Systems;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

namespace Examples.Basic
{
    public class ECSTest : Scene
    {
        public static void Main()
        {
            Soul.Engine.Core.Setup(new ECSTest());
        }

        protected override void Setup()
        {
            AddSystem(new TestSystem());

            Entity a = new Entity("test1");
            a.AttachComponent<Transform>();
            a.AttachComponent<RenderData>();
            a.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            AddEntity(a);

            Entity b = new Entity("test2");
            AddEntity(b);

            Scripting.Expose("obj", a);

            AddSystem(new TestSystem());
        }

    }
}
