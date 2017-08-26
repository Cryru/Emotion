// SoulEngine - https://github.com/Cryru/SoulEngine

using System;
using Soul.Engine.Objects;

namespace Soul.Engine.Scenes
{
    public class Loading : Scene
    {
        public override void Initialize()
        {
            AddChild("loadingScreen", new GameObject());
            GetChild<GameObject>("loadingScreen");
        }

        public override void Update()
        {

        }
    }
}