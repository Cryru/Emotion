#region Using

global using System;
global using System.Diagnostics;
global using System.Numerics;
global using System.Threading;
global using System.Collections;
global using System.Collections.Generic;

global using Emotion.Common;
global using Emotion.Standard.Logging;
global using Emotion.Primitives;
global using Emotion.Testing;
global using Emotion.Game;
global using Debug = Emotion.Testing.AssertWrapper; // System.Diagnostics interface for Debug.Assert
global using static Emotion.Testing.AssertWrapper;

global using Emotion.Graphics;
global using Emotion.Graphics.Objects;

global using MathF = System.MathF;
global using Texture = Emotion.Graphics.Objects.Texture;
global using Color = Emotion.Primitives.Color;
global using Rectangle = Emotion.Primitives.Rectangle;
global using Mesh = Emotion.Graphics.ThreeDee.Mesh;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using ITimer = Emotion.Game.Time.ITimer;

#endregion