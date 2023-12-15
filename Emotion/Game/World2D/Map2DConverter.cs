#region Using

using System.Reflection;
using Emotion.Game.Tiled;
using Emotion.Game.World;
using Emotion.IO;
using Emotion.Standard.TMX;
using Emotion.Standard.TMX.Layer;
using Emotion.Standard.TMX.Object;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public static class Map2DConverter
    {
        public static Map2D? ConvertTmxToMap2D(Type mapType, TileMap map)
        {
            if (!typeof(Map2D).IsAssignableFrom(mapType)) return null;
            if (map.TiledMap == null) return null;

            var newMap = (Map2D?) Activator.CreateInstance(mapType, true);
            if (newMap == null) return null;
            newMap.MapSize = map.WorldSize;
            newMap.MapName = map.FileName ?? "Converted TMX";
            newMap.FileName = map.FileName == null ? "converted.xml" : map.FileName.Replace(".tmx", "_new.xml");
            newMap.PersistentObjects = new List<BaseGameObject>(); // Serialization constructor wouldn't have made it.

            // Convert tile data.
            var tileData = new Map2DTileMapData
            {
                TileSize = map.TileSize,
                SizeInTiles = map.SizeInTiles,
                BackgroundColor = map.TiledMap.BackgroundColor
            };

            for (var i = 0; i < map.TiledMap.Tilesets.Count; i++)
            {
                TmxTileset? tileset = map.TiledMap.Tilesets[i];
                if (tileset == null) continue;

                // Convert tileset name to full asset path.
                string tileSetFolder = AssetLoader.GetDirectoryName(map.FileName ?? "");
                string? tilesetFile = tileset.Source;
                if (string.IsNullOrEmpty(tilesetFile)) continue;
                tilesetFile = AssetLoader.NameToEngineName(tilesetFile);
                if (tilesetFile[0] == '/') tilesetFile = tilesetFile[1..];

                string assetPath = AssetLoader.GetNonRelativePath(tileSetFolder, tilesetFile);
                tileData.Tilesets.Add(new Map2DTileset
                {
                    AssetFile = assetPath,
                    FirstTileId = tileset.FirstGid,
                    Spacing = tileset.Spacing,
                    Margin = tileset.Margin,
                });
            }

            for (var i = 0; i < map.TiledMap.TileLayers.Count; i++)
            {
                TmxLayer? layer = map.TiledMap.TileLayers[i];

                var layerTiles = new uint[layer.Tiles.Count];
                for (var j = 0; j < layer.Tiles.Count; j++)
                {
                    TmxLayerTile? tile = layer.Tiles[j];
                    var uintRepresentation = (uint) tile.Gid;

                    if (tile.HorizontalFlip)
                        uintRepresentation |= Map2DTileMapLayer.FLIPPED_HORIZONTALLY_FLAG;

                    if (tile.VerticalFlip)
                        uintRepresentation |= Map2DTileMapLayer.FLIPPED_VERTICALLY_FLAG;

                    if (tile.HorizontalFlip)
                        uintRepresentation |= Map2DTileMapLayer.FLIPPED_DIAGONALLY_FLAG;

                    layerTiles[j] = uintRepresentation;
                }

                var tileMapLayer = new Map2DTileMapLayer(layer.Name, layerTiles);
                tileMapLayer.Visible = layer.Visible;
                tileData.Layers.Add(tileMapLayer);
            }

            newMap.TileData = tileData;

            // Construct all objects.
            // Expected conversion signature in Map2D instance is
            // GameObject2D (string objectType, string objName, Vector2 objectPos, Map2DTileset tileset, Rectangle uv, TmxProperties properties)
            MethodInfo? constructionMethod = newMap.GetType().GetMethod("CreateObjectFromTmx");
            if (map.TiledMap.ObjectLayers.Count > 0 && constructionMethod != null)
                // For each layer with objects.
                for (var i = 0; i < map.TiledMap.ObjectLayers.Count; i++)
                {
                    TmxObjectLayer? objectLayer = map.TiledMap.ObjectLayers[i];

                    // For each object.
                    for (var j = 0; j < map.TiledMap.ObjectLayers[i].Objects.Count; j++)
                    {
                        TmxObject? objDef = objectLayer.Objects[j];

                        Map2DTileset? tilesetAsset = null;
                        Rectangle? uv = null;
                        if (objDef.Gid != null)
                        {
                            // Get the UV from the TiledMap as it supports variable tile sizes, and
                            // objects there are done through tilesets.
                            uv = map.GetUvFromTileImageId(objDef.Gid.Value, out int tsId);
                            if (tsId >= 0 && tsId < tileData.Tilesets.Count) tilesetAsset = tileData.Tilesets[tsId];
                        }

                        var obj = (GameObject2D?) constructionMethod.Invoke(newMap, new object[]
                        {
                            objDef.Type, objDef.Name, new Vector2(objDef.X, objDef.Y).Ceiling(), tilesetAsset!, uv!, objDef.Properties, objDef, objectLayer
                        });

                        if (obj != null)
                        {
                            obj.ObjectFlags |= ObjectFlags.Persistent;
                            newMap.AddObject(obj);
                        }
                    }
                }

            // Remove unused tilesets
            var tileColumns = (int) tileData.SizeInTiles.X;
            var tileRows = (int) tileData.SizeInTiles.Y;
            //int lastUsedIdx = 0; // We can only unused at the end as others will modify the tid.
            for (var i = tileData.Tilesets.Count - 1; i >= 0; i--)
            {
                var used = false;
                for (var j = 0; j < tileData.Layers.Count; j++)
                {
                    Map2DTileMapLayer layer = tileData.Layers[j];

                    for (var c = 0; c < tileRows * tileColumns; c++)
                    {
                        layer.GetTileData(c, out uint tId, out bool _, out bool _, out bool _);
                        int tsId = tileData.GetTilesetIdFromTid(tId, out int _);

                        if (tsId != i) continue;
                        used = true;
                        break;
                    }

                    if (used) break;
                }

                if (!used) tileData.Tilesets.RemoveAt(i);
            }

            return newMap;
        }
    }
}