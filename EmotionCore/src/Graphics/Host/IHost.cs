// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Host
{
    internal interface IHost
    {
        Vector2 Size { get; set; }
        void ApplySettings(Settings settings);
        void SetHooks(Action<float> update, Action<float> draw);
        void Run();
        void SwapBuffers();
    }
}