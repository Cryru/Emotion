﻿using Emotion.Game.WorldTwoDee;
using Emotion.Graphics;
using Emotion.Graphics.Assets;
using Emotion.IO;
using Emotion.WIPUpdates.One.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.BadIce;

public class Iceblock : MapObjectSprite
{
    private TextureAsset _texture;

    //public override void LoadAssets(AssetLoader assetLoader)
    //{
    //    _texture = assetLoader.ONE_Get<TextureAsset>("Test/bad_ice/iceblock.png");

    //    base.LoadAssets(assetLoader);
    //}

    public override void Render(RenderComposer c)
    {
        c.RenderSprite(Vector3.Zero, _texture);
    }
}
