// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using TiledSharp;
// ReSharper disable PossibleInvalidOperationException

#endregion

namespace Soul.Engine.Systems
{
    public class TiledRenderer : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(RenderData), typeof(TiledMap)};
        }

        protected internal override void Setup()
        {
            // This needs to run before rendering.
            Order = -1;
        }

        internal override void Update(Entity link)
        {
        }

        internal override void Draw(Entity link)
        {
            // Get components.
            RenderData renderData = link.GetComponent<RenderData>();
            TiledMap tiledMap = link.GetComponent<TiledMap>();

            // Check if the map is loaded, and all needed data is present.
            if (tiledMap.Map != null || string.IsNullOrEmpty(tiledMap.MapPath) || tiledMap.Tileset == null) return;

            // Read the file.
            tiledMap.Map = new TmxMap(tiledMap.MapPath);
            TmxMap map = tiledMap.Map;
            int width = tiledMap.Map.Width * tiledMap.Map.TileWidth;
            int height = tiledMap.Map.Height * tiledMap.Map.TileHeight;

            // Start drawing.
            if (tiledMap.CachedRender == null || tiledMap.CachedRender.Width != width ||
                tiledMap.CachedRender.Height != height)
                tiledMap.CachedRender = new RenderTarget2D(Core.Context.GraphicsDevice, width, height);

            Core.Context.Ink.SetRenderTarget(ref tiledMap.CachedRender);
            Core.Context.Ink.Start();
            foreach (TmxLayer layer in tiledMap.Map.Layers)
            {
                if (!layer.Visible) continue;

                for (int i = 0; i < layer.Tiles.Count; i++)
                {
                    // Get the tile ID.
                    int gId = layer.Tiles[i].Gid;

                    // Get the tile location in pixel space.
                    float offsetX = i % map.Width * map.TileWidth;
                    float offsetY = (float) Math.Floor(i / (double) map.Width) * map.TileHeight;

                    // Check for empty tile.
                    if (gId == 0) break;

                    // Get the tileset for this layer. This isn't in the layer method for reasons.
                    int tilesetId = 0;
                    int gID_offset = gId;
                    int tilesetLoopLast = 0;
                    for (int t = 0; t < map.Tilesets.Count; t++)
                    {
                        // Check if the current tile is beyond the first tileset.
                        if (gId > map.Tilesets[t].FirstGid)
                            tilesetId = t;
                        else
                            break;

                        tilesetLoopLast = t;
                    }

                    if (tilesetLoopLast > 0) gID_offset -= map.Tilesets[tilesetLoopLast].FirstGid - 1;

                    // Define the sizes of the tiles.
                    int tWidth = map.Tilesets[tilesetId].TileWidth;
                    int tHeight = map.Tilesets[tilesetId].TileHeight;

                    // Get the number of columns in the tileset.
                    int tilesetColumns = (int) map.Tilesets[tilesetId].Columns;

                    // Alias the size of the tile set.
                    int tilesetWidth = (int) map.Tilesets[tilesetId].Image.Width;
                    int tilesetHeight = (int) map.Tilesets[tilesetId].Image.Height;

                    // Find the current tile within the tileset, according to the id we calculated.
                    int tileFrame = gID_offset - 1;
                    int column = tileFrame % tilesetColumns;
                    int row = (int) (tileFrame / (double) tilesetColumns);

                    // The rectangle that holds the location of the tile.
                    Rectangle tilesetRec = new Rectangle(tWidth * column, tHeight * row, tWidth, tHeight);

                    //Add margins
                    tilesetRec.X += map.Tilesets[tilesetId].Margin;
                    tilesetRec.Y += map.Tilesets[tilesetId].Margin;
                    //Add spacing
                    tilesetRec.X += map.Tilesets[tilesetId].Spacing * column;
                    tilesetRec.Y += map.Tilesets[tilesetId].Spacing * row;


                    //Location of the map plus the offset of the current tile multiplied by the scale.
                    Core.Context.Ink.Draw(tiledMap.Tileset,
                        new Rectangle(
                            (int) offsetX,
                            (int) offsetY,
                            tWidth,
                            tHeight),
                        tilesetRec, Color.White);
                }
            }

            Core.Context.Ink.End();
            Core.Context.Ink.UnsetRenderTarget();

            renderData.Texture = tiledMap.CachedRender;
        }
    }
}