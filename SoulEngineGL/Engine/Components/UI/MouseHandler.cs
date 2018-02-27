// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Components.UI
{
    public class MouseHandler : ComponentBase
    {
        public Action OnClick { get; set; }
        public Action OnHeld { get; set; }
        public Action OnLetGo { get; set; }
        public Action OnEnter { get; set; }
        public Action OnLeave { get; set; }

        internal bool WasIn = false;
        internal bool IsHeld = false;
    }
}