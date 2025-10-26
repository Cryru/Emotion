#region Using

global using System;
global using System.Diagnostics;
global using System.Numerics;
global using System.Threading;
global using System.Collections;
global using System.Collections.Generic;

global using Emotion.Core;
global using Emotion.Core.Systems.Input;
global using Emotion.Core.Systems.Logging;
global using Emotion.Standard;
global using Emotion.Standard.DataStructures;
global using Emotion.Standard.Extensions;
global using Emotion.Standard.Serialization;
global using Emotion.Game.World;
global using Emotion.Game.Systems.GameData;
global using Emotion.Game.Systems.UI;
global using Emotion.Game.Systems.UI2;
global using Emotion.Primitives;
global using Emotion.Testing;
global using Debug = Emotion.Testing.AssertWrapper; // System.Diagnostics interface for Debug.Assert
global using static Emotion.Testing.AssertWrapper;
global using ITimer = Emotion.Core.Utility.Time.ITimer;

global using Emotion.Graphics;
global using Emotion.Graphics.Objects;

global using MathF = System.MathF;
global using Color = Emotion.Primitives.Color;
global using Rectangle = Emotion.Primitives.Rectangle;

global using MeshReference = Emotion.Core.Systems.IO.AssetOrObjectReference<Emotion.Core.Systems.IO.MeshAsset, Emotion.Game.World.ThreeDee.MeshEntity>;
global using SpriteReference = Emotion.Core.Systems.IO.AssetOrObjectReference<Emotion.Core.Systems.IO.SpriteEntityAsset, Emotion.Game.World.TwoDee.SpriteEntity>;
global using TextureReference = Emotion.Core.Systems.IO.AssetOrObjectReference<Emotion.Graphics.Assets.TextureAsset, Emotion.Graphics.Objects.Texture>;
global using CubeMapTextureReference = Emotion.Core.Systems.IO.AssetOrObjectReference<Emotion.Graphics.Assets.TextureCubemapAsset, Emotion.Graphics.Objects.TextureCubemap>;
global using ShaderReference = Emotion.Core.Systems.IO.AssetOrObjectReference<Emotion.Graphics.Shader.NewShaderAsset, Emotion.Graphics.Shading.ShaderProgram>;

#endregion