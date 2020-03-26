#region Using

using System;

#endregion

namespace Emotion.Plugins.Steamworks.Helpers
{
    public class NativeMethodAttribute : Attribute
    {
        public string Name { get; private set; }

        public NativeMethodAttribute(string name)
        {
            Name = name;
        }
    }
}