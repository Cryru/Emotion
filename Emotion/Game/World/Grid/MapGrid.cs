﻿using System.Threading.Tasks;

#nullable enable

namespace Emotion.Game.World.Grid;

public interface IMapGrid
{
    public Task LoadAsync(BaseMap map);

    public void Render(RenderComposer c);
}

public class MapGrid<T> : Grid<T>, IMapGrid
{
    public virtual Task LoadAsync(BaseMap map)
    {
        return Task.CompletedTask;
    }

    public virtual void DebugRender(RenderComposer c)
    {
        //for (int x = 0; x < GridSizeInTiles.X; x++)
        //{
        //    for (int y = 0; y < GridSizeInTiles.Y; y++)
        //    {
        //        Vector2 gridTile = new Vector2(x, y);


        //    }
        //}
    }

    public virtual void Render(RenderComposer c)
    {

    }

    public Vector2 WorldToGrid(Vector3 wPos)
    {
        return Vector2.Zero;
    }

    public Vector3 GridToWorld(Vector2 gPos)
    {
        return Vector3.Zero;
    }
}
