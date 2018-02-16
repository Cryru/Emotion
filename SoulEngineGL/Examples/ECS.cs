using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.Scenography;

namespace Soul.Examples
{
    public class ECS : Scene
    {
        public static void Main()
        {
            Core.Setup(new ECS());
        }

        protected override void Setup()
        {
            // Add one system now.
            AddSystem(new TestSystem());

            Entity noLinkEntity = new Entity("noLink");
            AddEntity(noLinkEntity);

            Entity singleLink = new Entity("singleLink");
            singleLink.AttachComponent<TestComponent>();
            AddEntity(singleLink);

            // Add another now.
            AddSystem(new NoRequirementSystem());

            Entity otherComponent = new Entity("otherComponent");
            otherComponent.AttachComponent<OtherTestComponent>();
            AddEntity(otherComponent);

            AddSystem(new HighPrioritySystem());
        }

        protected override void Update()
        {
            
        }
    }

    public class TestComponent : ComponentBase
    {

    }

    public class OtherTestComponent : ComponentBase
    {

    }

    public class TestSystem : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(TestComponent) };
        }

        protected internal override void Setup()
        {
            Order = 1;
        }

        internal override void Update(Entity link)
        {
            
        }
    }

    public class NoRequirementSystem : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return null;
        }

        protected internal override void Setup()
        {
            Order = 0;
        }

        internal override void Update(Entity link)
        {
           
        }
    }

    public class HighPrioritySystem : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(OtherTestComponent) };
        }

        protected internal override void Setup()
        {
            Order = 999;
        }

        internal override void Update(Entity link)
        {

        }
    }
}
