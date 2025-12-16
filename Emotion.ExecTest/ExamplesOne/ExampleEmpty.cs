#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Scenography;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        yield break;
    }

    public override void RenderScene(Renderer c)
    {
        base.RenderScene(c);

        c.SetUseViewMatrix(false);
        //var sdfShader = Engine.AssetLoader.Get<NewShaderAsset>("Sdf.glsl");
        //if (!_noSdf) c.SetShader(sdfShader);
        //c.RenderSprite(new Vector3(200, 200, 0), _renderSize, Color.Blue);
        //c.RenderSprite(new Vector3(200, 200, 0), _renderSize, _t, new Rectangle(0, 0, _renderSize));
        //c.RenderSprite(new Vector3(200, 500, 0), _t.Size, _t);
        //c.SetShader(null);

        //var layoutEngine = new TextLayouter();
        //string text = $"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum. It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose (injected humour and the like).";
        //layoutEngine.RunLayout(text, 100, FontAsset.GetDefaultBuiltIn(), (int) c.CurrentTarget.Size.X, GlyphHeightMeasurement.FullHeight);
        ////c.RenderSprite(new Vector3(0, 0, 0), layoutEngine.Calculated_TotalSize.ToVec2(), Color.Yellow * 0.5f);
        //layoutEngine.RenderLastLayout(c, new Vector3(10, 0, 0), Color.White);
    }
}
