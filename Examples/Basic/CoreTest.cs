using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine.Scenography;

namespace Examples.Basic
{
    public class CoreTest : Scene
    {
        public static void Main()
        {
            Soul.Engine.Core.Setup(new CoreTest());
        }

        protected override void Setup()
        {
            
        }
    }
}
