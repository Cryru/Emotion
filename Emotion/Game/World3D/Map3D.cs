#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
    public override void Render(RenderComposer c)
    {
        if (!Initialized) return;

        Rectangle clipArea = c.Camera.GetCameraFrustum();

        var renderObjectsList = new List<BaseGameObject>();
        GetObjects(renderObjectsList, 0, clipArea);
        for (var i = 0; i < renderObjectsList.Count; i++)
        {
            BaseGameObject obj = renderObjectsList[i];
            obj.Render(c);
        }
    }
}