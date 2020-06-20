#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Platform;
using Emotion.Primitives;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Common
{
    public sealed class Configurator
    {
        /// <summary>
        /// Set the logging provider. If none set the default one will be created at setup.
        /// The default logger logs to the console if in debug mode, and to a file.
        /// </summary>
        public LoggingProvider Logger { get; set; }

        /// <summary>
        /// Whether the Engine is in debug mode. Off by default.
        /// There are other factors such as compilation targets which determine whether it is "truly" debug mode,
        /// but most of the "in engine" debugging functionalities are runtime and determined using this flag,
        /// because the nuget package is compiled in Release mode. The debug mode compilation is for debugging
        /// the engine, while this switch is for debugging the game running on it.
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// When enabled an OpenGL debug context (platform dependent) will be created and a debug callback will be attached,
        /// printing messages in the logs.
        /// </summary>
        public bool GlDebugMode { get; set; }

        #region Rendering

        /// <summary>
        /// The resolution to render at. The host size should never be below this number.
        /// </summary>
        public Vector2 RenderSize { get; set; } = new Vector2(640, 360);

        /// <summary>
        /// Whether to run the renderer in compatibility mode.
        /// In this mode newer features such as "direct state access" are disabled.
        /// Is automatically set if certain conditions are detected.
        /// </summary>
        public bool RendererCompatMode { get; set; }

        /// <summary>
        /// If enabled the DrawBuffer will only scale between 1-1.99 and the integer scaling will be performed
        /// automatically when drawing to the screen buffer. This means your IntScale will always be 1 - but it
        /// also reduces the resolution you can work with outside of integer scaling (as in non-pixel art, UI etc.)
        /// This is off by default, but it is a huge performance boost - at the cost
        /// of very little in games which do not use scaling a lot.
        /// If scale black bars is enabled this will cause it to scale integerly only.
        /// </summary>
        public bool IntScaleDrawBuffer { get; set; }

        /// <summary>
        /// Whether to render with pillarboxed/letterboxed if the render size doesn't match the host size.
        /// This is off by the default and the camera adjusts the scale.
        /// </summary>
        public bool ScaleBlackBars { get; set; }

        /// <summary>
        /// Whether to render to a draw buffer before blitting to the screen buffer.
        /// Disabling this is a performance gain.
        /// </summary>
        public bool UseIntermediaryBuffer { get; set; }

        /// <summary>
        /// Whether textures should default to "smooth" (bilinear) sampling on creation.
        /// This is off by default and textures are created with a nearest filter.
        /// </summary>
        public bool TextureDefaultSmooth { get; set; }

        /// <summary>
        /// The color to clear the default and all other framebuffers to.
        /// This is transparent by default and is set only once - at the start.
        /// </summary>
        public Color ClearColor { get; set; } = new Color(0, 0, 0, 0);

        #endregion

        #region Loop

        /// <summary>
        /// The desired tps. 60 by default.
        /// You don't want the true time between ticks as that will cause an uneven delta time and hiccups.
        /// This setting applies only if using the default loop.
        /// </summary>
        public byte DesiredStep { get; set; } = 60;

        /// <summary>
        /// Whether to scale the step up if the game is running fast enough. This causes more frequent updates with smaller
        /// DeltaTime increments.
        /// If VSync is on this is not possible.
        /// This setting applies only if using the default loop.
        /// </summary>
        public bool ScaleStepUp { get; set; } = true;

        /// <summary>
        /// Whether to scale the loop speed between double the desired step and 4 times as slow as the desired step.
        /// This is on by default, but so is VSync so the loop will only scale downward.
        /// If this is disabled ScaleStepUp is ignored.
        /// This setting applies only if using the default loop.
        /// </summary>
        public bool VariableLoopSpeed { get; set; } = true;

        /// <summary>
        /// If true frames will be rendered only when at least one update occurs in a tick.
        /// This will essentially cap the FPS to the DesiredStep (and the variance associated, if any).
        /// In some cases you might want this, as VSync (such as VSync forced by the driver) can cause
        /// the loop to be throttled by a wait on what is essentially an unchanged frame. Usually a problem on weaker machines.
        /// This setting applies only if using the default loop.
        /// Off by default.
        /// </summary>
        public bool DrawOnUpdate { get; set; }

        /// <summary>
        /// The function to run as a loop.
        /// The first argument passed is the "RunTick" method which you should call every tick, the second is the "RunFrame" method
        /// which performs rendering.
        /// If no factory is set the default one will be used. Other loop settings might not apply if not using the default loop.
        /// Platforms are free to override your loop settings.
        /// </summary>
        public Action<Action, Action> LoopFactory { get; set; }

        #endregion

        #region Platform

        /// <summary>
        /// Set the engine's platform. Usually it is auto detected - but on platforms not part
        /// of the base-Emotion package this might be necessary.
        /// </summary>
        public PlatformBase PlatformOverride { get; set; }

        /// <summary>
        /// Some kind of title or label displayed on the host.
        /// On desktop platforms this is the window name.
        /// </summary>
        public string HostTitle { get; set; } = "Untitled";

        /// <summary>
        /// The starting size of the host. On desktop platforms the host is the window.
        /// On some platforms this is ignored as the host is either always fullscreen or unresizable.
        /// </summary>
        public Vector2 HostSize { get; set; } = new Vector2(640, 360);

        /// <summary>
        /// The display mode to start the host in.
        /// Not all values are valid for all platforms - invalid ones will fallback to the platform default.
        /// </summary>
        public DisplayMode InitialDisplayMode { get; set; } = DisplayMode.Windowed;

        #endregion

        #region Audio

        /// <summary>
        /// Volume modulation is not linear but exponential. This is the base.
        /// </summary>
        public float AudioCurve { get; set; } = 2f;

        /// <summary>
        /// The master volume. This is the percentage of the layer's volume to play at.
        /// The exponential transform is not applied to it.
        /// </summary>
        public float MasterVolume { get; set; } = 1f;

        #endregion

        #region Plugins

        /// <summary>
        /// List of plugins to load.
        /// </summary>
        public List<IPlugin> Plugins { get; private set; } = new List<IPlugin>();

        /// <summary>
        /// Add a plugin.
        /// </summary>
        /// <param name="plugin">The plugin to add.</param>
        /// <returns>This configurator, for chaining purposes.</returns>
        public Configurator AddPlugin(IPlugin plugin)
        {
            Plugins.Add(plugin);
            return this;
        }

        #endregion
    }
}