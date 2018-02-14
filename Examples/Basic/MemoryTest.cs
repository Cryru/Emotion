// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Soul.Engine;
using Soul.Engine.Scenography;

#endregion

namespace Examples.Basic
{
    public class MemoryTest : Scene
    {
        public static void Main()
        {
            Core.Setup(new MemoryTest());
        }

        protected override void Setup()
        {
        }
    }
}