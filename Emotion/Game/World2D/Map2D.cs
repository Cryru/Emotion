#region Using

using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Editor;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.Game.World2D;

public class Map2D : BaseMap
{
    /// <summary>
    /// Contains tile information, if the map has a tile map portion.
    /// </summary>
    [DontShowInEditor] public Map2DTileMapData? TileData;

    public Map2D(Vector2 size, string mapName = "Unnamed Map") : base(size, mapName)
    {
    }

    // Serialization constructor
    protected Map2D() : base()
    {
    }

    #region Init

    protected override async Task InitAsyncInternal()
    {
        // Load tile data. During this time object loading is running async.
        if (TileData != null) await TileData.LoadTileDataAsync();
    }

    #endregion

    public static Comparison<BaseGameObject> ObjectComparison = ObjectSort; // Prevent delegate allocation

    protected static int ObjectSort(BaseGameObject x, BaseGameObject y)
    {
        return MathF.Sign(x.Position.Z - y.Position.Z);
    }

    public override void Render(RenderComposer c)
    {
        if (!Initialized) return;

        Rectangle clipArea = c.Camera.GetCameraFrustum();
        TileData?.RenderTileMap(c, clipArea);

        var renderObjectsList = new List<BaseGameObject>();
        GetObjects(renderObjectsList, 0, clipArea);
        renderObjectsList.Sort(ObjectComparison);
        for (var i = 0; i < renderObjectsList.Count; i++)
        {
            BaseGameObject obj = renderObjectsList[i];
            obj.Render(c);
        }
    }
}