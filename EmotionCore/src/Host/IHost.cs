// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Primitives;
using Emotion.System;
using OpenTK.Input;

#endregion

namespace Emotion.Host
{
    public interface IHost
    {
        Vector2 Size { get; set; }
        Vector2 RenderSize { get; }
        bool Focused { get; }

        void ApplySettings(Settings settings);
        void SetHooks(Action<float> update, Action<float> draw);

        void Run();
        void SwapBuffers();

        // Events.
        event EventHandler<MouseButtonEventArgs> MouseDown;
        event EventHandler<MouseButtonEventArgs> MouseUp;
        event EventHandler<MouseMoveEventArgs> MouseMove;
        event EventHandler<EventArgs> FocusedChanged;

        // Cleanup.
        void Close();
        void Dispose();
    }
}