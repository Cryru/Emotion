using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using SoulEngine.Objects.Components.Helpers;
using SoulEngine.Modules;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Used to render maps created by the Tiled program. 
    /// Uses TiledSharp - Public Repository: https://github.com/marshallward/TiledSharp
    /// </summary>
    public class TiledMap : DrawComponent
    {
        #region "Declarations"
        #region "Read-Only"
        /// <summary>
        /// The TiledSharp map object.
        /// </summary>
        public TmxMap Map
        {
            get
            {
                return map;
            }
        }
        /// <summary>
        /// The number of loaded tileset textures.
        /// </summary>
        public int LoadedTilesets
        {
            get
            {
                return tileSets.Count;
            }
        }
        #endregion
        #region "Private"
        /// <summary>
        /// The TiledSharp object that holds the map.
        /// </summary>
        private TmxMap map;
        /// <summary>
        /// The list of load tileset textures.
        /// </summary>
        private List<Texture2D> tileSets = new List<Texture2D>();
        /// <summary>
        /// The Tiled object converted to an internal TileObject type.
        /// </summary>
        private List<TileObject> tileObjects = new List<TileObject>();
        /// <summary>
        /// The map layers as composed to textures.
        /// </summary>
        private List<Texture2D> mapLayers = new List<Texture2D>();
        /// <summary>
        /// The render target that composes the map.
        /// </summary>
        private RenderTarget2D mapComposer;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// Initializes a new map object, loading the .tmx from the specified path.
        /// </summary>
        /// <param name="mapPath">The map file's location. The root is the Content folder.</param>
        /// <param name="tilesetContentPath">The path to the folder where the tileset images are. The root is the Content folder.</param>
        public TiledMap(string MapPath, string TilesetsContentPath = "Tilesets/")
        {
            Reload(MapPath, TilesetsContentPath);
        }
        #endregion

        /// <summary>
        /// Reloads the map object, loading the .tmx from the specified path.
        /// </summary>
        /// <param name="mapPath">The map file's location. The root is the Content folder.</param>
        /// <param name="tilesetContentPath">The path to the folder where the tileset images are. The root is the Content folder.</param>
        public void Reload(string MapPath, string TilesetsContentPath = "Tilesets/")
        {
            //Load the map data.
            string MapData = AssetManager.CustomFile(MapPath + ".tmx");

            //Check if the map file exists.
            if (MapData == "")
            {
                Texture = null;
                map = null;
                return;
            }

            //Create a stream because that's what TiledSharp wants.
            Stream mapDataStream = Functions.StreamFromString(MapData);

            //Load the map file.
            map = new TmxMap(mapDataStream);

            //Close the stream.
            mapDataStream.Close();

            //Load tilesets.
            for (int i = 0; i < map.Tilesets.Count; i++)
            {
                tileSets.Add(AssetManager.Texture(TilesetsContentPath + map.Tilesets[i].Name.ToString()));
            }

            //Process the objects in the map.
            ProcessObjects();
        }

        public override void Compose()
        {
            //Check if a map is loaded.
            if (map == null) return;

            //Clear last frame's layers.
            mapLayers.Clear();

            //Iterate through each layer, composing them and saving them.
            for (int layer = 0; layer < map.Layers.Count; layer++)
            {

                //Define a render target to draw the layers to.
                Context.ink.StartRenderTarget(ref mapComposer, map.Width * map.TileWidth, map.Height * map.TileHeight);

                //Clear the render target.
                Context.Graphics.Clear(Color.Transparent);

                //Check if visible.
                if (map.Layers[layer].Visible == false)
                {
                    continue;
                }

                //Iterate through each tile on the layer.
                for (int i = 0; i < map.Layers[layer].Tiles.Count; i++)
                {
                    //Get the tile ID.
                    int gID = map.Layers[layer].Tiles[i].Gid;

                    //Get the tile location in pixel space.
                    float offsetX = (i % map.Width) * map.TileWidth;
                    float offsetY = (float)Math.Floor(i / (double)map.Width) * map.TileHeight;

                    //If empty tile then skip.
                    if (gID == 0)
                    {
                        continue;
                    }

                    //Get the tileset for this layer. This isn't in the layer method for reasons.
                    int tilesetID = 0;
                    int gID_offset = gID;
                    int tilesetLoopLast = 0;
                    for (int t = 0; t < map.Tilesets.Count; t++)
                    {
                        //Check if the current tile is beyond the first tileset.
                        if (gID > map.Tilesets[t].FirstGid)
                        {
                            tilesetID = t;
                        }
                        else
                        {
                            break;
                        }

                        tilesetLoopLast = t;
                    }
                    if (tilesetLoopLast > 0)
                    {
                        gID_offset -= map.Tilesets[tilesetLoopLast].FirstGid - 1;
                    }

                    //Define the sizes of the tiles.
                    int tWidth = map.Tilesets[tilesetID].TileWidth;
                    int tHeight = map.Tilesets[tilesetID].TileHeight;

                    //Get the number of columns in the tileset.
                    int tilesetColumns = (int)map.Tilesets[tilesetID].Columns;

                    //Define the size of the tile set.
                    int tilesetWidth = (int)map.Tilesets[tilesetID].Image.Width;
                    int tilesetHeight = (int)map.Tilesets[tilesetID].Image.Height;

                    //Find the current tile within the tileset, according to the id we calculated.
                    int tileFrame = gID_offset - 1;
                    int column = tileFrame % tilesetColumns;
                    int row = (int)(tileFrame / (double)tilesetColumns);

                    //The rectangle that holds the location of the tile.
                    Rectangle tilesetRec = new Rectangle(tWidth * column, tHeight * row, tWidth, tHeight);

                    //Add margins
                    tilesetRec.X += map.Tilesets[tilesetID].Margin;
                    tilesetRec.Y += map.Tilesets[tilesetID].Margin;
                    //Add spacing
                    tilesetRec.X += map.Tilesets[tilesetID].Spacing * column;
                    tilesetRec.Y += map.Tilesets[tilesetID].Spacing * row;


                    //Location of the map plus the offset of the current tile multiplied by the scale.
                    Context.ink.Draw(tileSets[tilesetID], new Rectangle(
                        (int)(offsetX),
                        (int)(offsetY),
                        (int)(tWidth),
                        (int)(tHeight)),
                        tilesetRec, Color.White);
                }

                //End drawing.
                Context.ink.End();

                //Save the layer to an image.
                mapLayers.Add(mapComposer as Texture2D);
            }

            //Stop drawing on the render target.
            Context.ink.EndRenderTarget();
        }

        #region "Draw and Extensions"
        /// <summary>
        /// Draws the specified layer of the map.
        /// </summary>
        public void Draw(int DrawLayer)
        {
            //If no map loaded return;
            if (map == null) return;

            //Check if empty texture, sometimes it happens.
            if (mapLayers.Count == 0) return;

            //Draw the texture.
            Draw(mapLayers[DrawLayer]);
        }

        /// <summary>
        /// Draw all layers.
        /// </summary>
        public override void Draw()
        {
            if (map == null) return;

            DrawLayers(0, map.Layers.Count - 1);
        }

        /// <summary>
        /// Draw all layers between the specified range.
        /// </summary>
        public void DrawLayers(int LayerToStartAt, int LayerToStopAt)
        {
            for (int i = Math.Max(Math.Min(map.Layers.Count, LayerToStartAt), 0); i <= Math.Min(mapLayers.Count - 1, LayerToStopAt); i++)
            {
                Draw(i);
            }
        }
        #endregion

        #region "Internal Functions"
        /// <summary>
        /// Extracts all tmx objects and converts them to tile objects.
        /// </summary>
        private void ProcessObjects()
        {
            for (int i = 0; i < map.ObjectGroups.Count; i++)
            {
                for (int o = 0; o < map.ObjectGroups[i].Objects.Count; o++)
                {
                    var b = map.ObjectGroups[i].Objects[o];
                    tileObjects.Add(new TileObject(b.Name, b.Properties, b.Type, (int)b.X, (int)b.Y,
                        new Vector2(map.TileWidth, map.TileHeight), map.Width));
                }
            }
        }
        #endregion

        #region "Drawing Helpers"
        /// <summary>
        /// Draw all layers up to the specified one.
        /// </summary>
        public void DrawUpTo(int LayerToStopAt)
        {
            for (int i = 0; i < Math.Min(mapLayers.Count, LayerToStopAt + 1); i++)
            {
                Draw(i);
            }
        }
        /// <summary>
        /// Draw all layers from the specified one to the final one.
        /// </summary>
        /// <param name="LayerToStartAt"></param>
        public void DrawFrom(int LayerToStartAt)
        {
            for (int i = LayerToStartAt; i < mapLayers.Count; i++)
            {
                Draw(i);
            }
        }
        #endregion

        #region "Coordinate Locators"
        /// <summary>
        /// Returns the pixel location of a tile coordinate.
        /// </summary>
        public Vector2 TileCoordinateToWorldLocation(int TileCoordinate)
        {
            //Check if no map.
            if (map == null || TileCoordinate < 0) return Vector2.Zero;

            if (TileCoordinate >= map.Layers[0].Tiles.Count) return Vector2.Zero;

            //Get the X and Y of the tile.
            float XTile = attachedObject.X + map.Layers[0].Tiles[TileCoordinate].X * GetWarpedTileSize().X;
            float YTile = attachedObject.Y + map.Layers[0].Tiles[TileCoordinate].Y * GetWarpedTileSize().Y;
            //The location of the object, plus the tile's actual size warped through the scale.

            return new Vector2(XTile, YTile);
        }

        /// <summary>
        /// Converts a pixel location to a tile coordinate.
        /// </summary>
        public int WorldLocationToTileCoordinate(Vector2 WorldCoordinate)
        {
            //Check if no map.
            if (map == null) return -1;

            //Assign a selector.
            Rectangle selector = new Rectangle(WorldCoordinate.ToPoint(), new Point(1, 1));

            //Run through all tiles until the selector is hit by a tile.
            for (int i = 0; i < map.Layers[0].Tiles.Count; i++)
            {
                Vector2 TileWorldLocation = TileCoordinateToWorldLocation(i);

                if (selector.Intersects(new Rectangle(TileWorldLocation.ToPoint(), GetWarpedTileSize().ToPoint())))
                {
                    return i;
                }

            }

            //If not return an invalid value.
            return -1;
        }

        /// <summary>
        /// Returns the size of tiles as warped through the current size of the map object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWarpedTileSize()
        {
            //Check if invalid map.
            if (map == null && mapLayers.Count == 0) return Vector2.Zero;

            //Calculate warp scale.
            float XScale = attachedObject.Width / (map.Width * map.TileWidth);
            float YScale = attachedObject.Height / (map.Height * map.TileHeight);

            //Get the X and Y of the tile.
            float WarpedWidth = map.TileWidth * XScale;
            float WarpedHeight = map.TileHeight * YScale;

            return new Vector2(WarpedWidth, WarpedHeight);
        }
        #endregion

        #region "Tile Converters"
        /// <summary>
        /// Returns the single dimension coordinate of a tile from its two dimensional coordinate.
        /// </summary>
        public Vector2 TileLocationAsVector2(int TileCoordinate)
        {
            //Check if no map.
            if (map == null || TileCoordinate < 0) return Vector2.Zero;

            //Check if out of range.
            if (TileCoordinate >= map.Layers[0].Tiles.Count) return Vector2.Zero;

            return new Vector2(map.Layers[0].Tiles[TileCoordinate].X, map.Layers[0].Tiles[TileCoordinate].Y);
        }

        /// <summary>
        /// Returns the two dimensional coordinate of a tile from its single dimensional coordinate.
        /// </summary>
        public int TileLocationAsInt(Vector2 TileCoordinate)
        {
            //Check if invalid map.
            if (map == null) return -1;

            //Find the index of the item that has the same X and Y values as the ones we are looking for.
            return map.Layers[0].Tiles.IndexOf(map.Layers[0].Tiles.ToList().Find(x => x.X == TileCoordinate.X && x.Y == TileCoordinate.Y));
        }
        #endregion

        #region "Data Getters"
        /// <summary>
        /// Returns the data for the selected tile coordinate.
        /// </summary>
        /// <param name="TileCoordinate">The one dimensional coordinate of the tile.</param>
        /// <param name="Layer">The layer of the tile.</param>
        public TileData GetTileDataFromCoordinate(int TileCoordinate, int Layer)
        {

            //Check if layer is out of bounds.
            if (Layer > map.Layers.Count - 1 || TileCoordinate > map.Layers[Layer].Tiles.Count || TileCoordinate < 0)
            {
                return new TileData(this, new Vector2(), new Vector2(), -1, -1, -1, new List<TileObject>());
            }

            //Get the GID of the tile.
            int gID;

            gID = map.Layers[Layer].Tiles[TileCoordinate].Gid;

            //Get the tileset for this layer.
            int tilesetID = 0;
            int imageID = gID;
            for (int t = 0; t < map.Tilesets.Count; t++)
            {
                //Check if the current tile is beyond the first tileset.
                if (gID > map.Tilesets[t].FirstGid)
                {
                    tilesetID = t;

                    if (t > 0)
                    {
                        imageID -= map.Tilesets[t].FirstGid - t;
                    }
                }
                else
                {
                    break;
                }
            }

            //Get the location as a vector 2.
            Vector2 Location = TileLocationAsVector2(TileCoordinate);
            Vector2 WorldLocation = TileCoordinateToWorldLocation(TileCoordinate);

            //Get the tile objects on the tile.
            List<TileObject> objList = new List<TileObject>();
            for (int i = 0; i < tileObjects.Count; i++)
            {
                if (tileObjects[i].Location == TileCoordinate)
                {
                    objList.Add(tileObjects[i]);
                }
            }

            return new TileData(this, Location, WorldLocation, tilesetID, imageID, Layer, objList);
        }
        /// <summary>
        /// Returns the data for the selected tile coordinate.
        /// </summary>
        /// <param name="TileCoordinate">The two dimensional coordinate of the tile.</param>
        /// <param name="Layer">The layer of the tile.</param>
        public TileData GetTileDataFromCoordinate(Vector2 TileCoordinate, int Layer)
        {
            return GetTileDataFromCoordinate(TileLocationAsInt(TileCoordinate), Layer);
        }
        #endregion
    }
}
