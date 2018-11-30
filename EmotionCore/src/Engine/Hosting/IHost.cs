// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Primitives;
using OpenTK.Input;

#endregion

namespace Emotion.Engine.Hosting
{
    /// <summary>
    /// A platform host for the engine. It must provide a GL context, surface, input, and the standard update-draw loop.
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// The size of the host display area.
        /// </summary>
        Vector2 Size { get; set; }

        /// <summary>
        /// Whether the host is focused.
        /// </summary>
        bool Focused { get; }

        /// <summary>
        /// Applies host settings to the host.
        /// </summary>
        /// <param name="settings"></param>
        void ApplySettings(Settings settings);

        /// <summary>
        /// Setup function hooks for host functionality.
        /// </summary>
        /// <param name="onUpdate">Update loop cycle.</param>
        /// <param name="onDraw">Draw loop cycle.</param>
        /// <param name="onResize">Resize event.</param>
        /// <param name="onClose">Host close event.</param>
        void SetHooks(Action<float> onUpdate, Action<float> onDraw, Action onResize, Action onClose);

        /// <summary>
        /// Start running the host loop.
        /// </summary>
        void Run();

        /// <summary>
        /// Swap OpenGL buffers on the host.
        /// </summary>
        void SwapBuffers();

        // Events.
        event EventHandler<MouseButtonEventArgs> MouseDown;
        event EventHandler<MouseButtonEventArgs> MouseUp;
        event EventHandler<MouseMoveEventArgs> MouseMove;
        event EventHandler<EventArgs> FocusedChanged;

        /// <summary>
        /// Close the host.
        /// </summary>
        void Close();

        /// <summary>
        /// Perform host cleanup.
        /// </summary>
        void Dispose();
    }
}