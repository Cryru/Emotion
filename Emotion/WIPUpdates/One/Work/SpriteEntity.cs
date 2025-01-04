using Emotion.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.Work;

public class SpriteEntity
{
    public string Name = "Unnamed Entity";
    public SerializableAssetHandle<TextureAsset> Textures;

    public string SourceFile;

    public void Render(RenderComposer c)
    {

    }
}
