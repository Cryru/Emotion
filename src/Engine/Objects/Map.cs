using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TiledSharp;
using System.IO;
using System.Linq;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    // Copyright TiledSharp - https://github.com/marshallward/TiledSharp        //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An object for rendering Tiled maps, using TiledSharp.
    /// </summary>
    public class Map : ObjectBase
    {
        #region "Declarations"
        #region "Map Data"
        /// <summary>
        /// The TiledSharp object that holds the map.
        /// </summary>
        private TmxMap map;
        /// <summary>
        /// The list of load tileset textures.
        /// </summary>
        private List<Texture> tileSets = new List<Texture>();

        private List<TileObject> tileObjects = new List<TileObject>();
        #endregion
        #region "ReadOnly"
        /// <summary>
        /// The TiledSharp map object.
        /// </summary>
        public TmxMap TiledMap
        {
            get
            {
                return map;
            }
        }
        /// <summary>
        /// The number of loaded tileset textures.
        /// </summary>
        public int LoadedTileSets
        {
            get
            {
                return tileSets.Count;
            }
        }
        #endregion
        #region "Internal"
        /// <summary>
        /// The path to the map files.
        /// </summary>
        private string mapsContentPath = "Content/SNcon/";
        /// <summary>
        /// The map layers as composed to textures.
        /// </summary>
        private List<Texture> mapLayers = new List<Texture>();
        #endregion
        #endregion

        /// <summary>
        /// Initializes a new map object, loading the .tmx from the specified path.
        /// </summary>
        /// <param name="mapPath">The map file, pathed from Content/SNCon/</param>
        /// <param name="tilesetContentPath">The path to the tileset images, pathed from Content/SCon/</param>
        public Map(string mapPath, string tilesetContentPath = "Tilesets/", DrawMode ComposeMode = DrawMode.Default)
        {
            //Check if the map file exists.
            if(File.Exists(mapsContentPath + mapPath + ".tmx") == false)
            {
                return;
            }
     
            //Create a stream
            Stream mapData = File.Open(mapsContentPath + mapPath + ".tmx", FileMode.Open);

            //Load the map file.
            map = new TmxMap(mapData);

            //Close the stream.
            mapData.Close();

            //Load tilesets.
            for (int i = 0; i < map.Tilesets.Count; i++)
            {
                tileSets.Add(new Texture(tilesetContentPath + map.Tilesets[i].Name.ToString()));
            }

            //Compose the map.
            ComposeMap(ComposeMode);

            //Add objects.
            ProcessObjects();

            //Set the image to first layer.
            Image = mapLayers[0];
        }

        /// <summary>
        /// Draws all layers and tiles into layer images.
        /// </summary>
        /// <param name="ComposeMode"></param>
        private void ComposeMap(DrawMode ComposeMode)
        {
            //Check if no map is loaded.
            if (map == null) return;

            //Save the viewport.
            Viewport tempPort = Core.graphics.GraphicsDevice.Viewport;

            //Iterate through each layer, composing them and saving them.
            for (int layer = 0; layer < map.Layers.Count; layer++)
            {
                //Define a render target to draw the layers to.
                RenderTarget2D tempTarget = new RenderTarget2D(Core.graphics.GraphicsDevice, map.Width * map.TileWidth, map.Height * map.TileHeight);
                //Set the graphics to draw to the render target.
                Core.graphics.GraphicsDevice.SetRenderTarget(tempTarget);

                //Clear the render target.
                Core.graphics.GraphicsDevice.Clear(Color.Transparent);

                //Check if visible.
                if (map.Layers[layer].Visible == false)
                {
                    continue;
                }

                //Start drawing.
                Core.DrawOnNone(ComposeMode);

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
                    Core.ink.Draw(tileSets[tilesetID].Image, new Rectangle(
                        (int)(Location.X + offsetX),
                        (int)(Location.Y + offsetY ),
                        (int)(tWidth),
                        (int)(tHeight)),
                        tilesetRec, Color.White);
                }

                //End drawing.
                Core.ink.End();

                //Save the layer to an image.
                mapLayers.Add(new Texture(tempTarget));
            }


            //Return to the default render target.
            Core.graphics.GraphicsDevice.SetRenderTarget(null);

            //Restore the viewport.
            Core.graphics.GraphicsDevice.Viewport = tempPort;
        }

        /// <summary>
        /// Extracts all tmx objects and converts them to tile objects.
        /// </summary>
        private void ProcessObjects()
        {
            for (int i = 0; i < TiledMap.ObjectGroups.Count; i++)
            {
                for (int o = 0; o < TiledMap.ObjectGroups[i].Objects.Count; o++)
                {
                    var b = TiledMap.ObjectGroups[i].Objects[o];
                    tileObjects.Add(new TileObject(b.Name, b.Properties, b.Type, (int) b.X, (int) b.Y, 
                        new Vector2(map.TileWidth, map.TileHeight), map.Width));
                }
            }
        }

        /// <summary>
        /// Draws the specified layer of the map.
        /// </summary>
        public void Draw(int DrawLayer)
        {
            //If no map loaded return;
            if (map == null) return;

            //Load the specified layer as an image.
            Image = mapLayers[DrawLayer];

            //Draw the image through the objectbase draw method.
            base.Draw();

            //Return the image to the first layer.
            Image = mapLayers[0];
        }

        //Draw all layers.
        public override void Draw()
        {
            DrawLayers(0, map.Layers.Count - 1);
        }

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
        /// <summary>
        /// Draw all layers from the specified one to the other specified one.
        /// </summary>
        public void DrawLayers(int LayerToStartAt, int LayerToStopAt)
        {
            for (int i = Math.Max(Math.Min(map.Layers.Count, LayerToStartAt), 0); i <= Math.Min(mapLayers.Count - 1, LayerToStopAt); i++)
            {
                Draw(i);
            }
        }
        #endregion
        #region "Coordinate Locators"
        /// <summary>
        /// Returns a pixel location of a tile coordinate.
        /// </summary>
        public Vector2 TileCoordinateToWorldLocation(int TileCoordinate)
        {
            //Check if no map.
            if (map == null || TileCoordinate < 0) return Vector2.Zero;

            if (TileCoordinate >= map.Layers[0].Tiles.Count) return Vector2.Zero;

            //Get the X and Y of the tile.
            float XTile = X + map.Layers[0].Tiles[TileCoordinate].X * GetWarpedTileSize().X;
            float YTile = Y + map.Layers[0].Tiles[TileCoordinate].Y * GetWarpedTileSize().Y;
            //The location of the object, plus the tile's actual size warped through the scale.

            return new Vector2(XTile, YTile);
        }

        /// <summary>
        /// Converts a pixel location of a tile coordinate.
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
        #endregion
        #region "Tile Convertors"
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
                if(tileObjects[i].Location == TileCoordinate)
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

        /// <summary>
        /// Returns the size of tiles as warped through the current size of the map object.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWarpedTileSize()
        {
            //Check if invalid map.
            if(map == null) return Vector2.Zero;

            //Calculate warp scale.
            float XScale =  Width / (map.Width * map.TileWidth);
            float YScale =  Height / (map.Height * map.TileHeight);

            //Get the X and Y of the tile.
            float WarpedWidth = map.TileWidth * XScale;
            float WarpedHeight = map.TileHeight * YScale;

            return new Vector2(WarpedWidth, WarpedHeight);
        }
    }

    /// <summary>
    /// An object that holds tile data.
    /// </summary>
    public class TileData
    {
        #region "Privates ;)"
        /// <summary>
        /// The map object this tile belongs to.
        /// </summary>
        private Map originMap;
        /// <summary>
        /// The X coordinate of the tile in map space.
        /// </summary>
        private int X = -1;
        /// <summary>
        /// The Y coordinate of the tile, in map space.
        /// </summary>
        private int Y = -1;
        /// <summary>
        /// The X coordinate of the tile in world space.
        /// </summary>
        private int XWorld = -1;
        /// <summary>
        /// The Y coordinate of the tile in world space.
        /// </summary>
        private int YWorld = -1;
        /// <summary>
        /// The tileset this tile belongs to.
        /// </summary>
        private int tileSet = -1;
        /// <summary>
        /// The ID of the tile image within the tileset.
        /// </summary>
        private int tileImage = -1;
        /// <summary>
        /// The layer the tile is on.
        /// </summary>
        private int layer = -1;
        /// <summary>
        /// A list of all objects on this tile.
        /// </summary>
        private List<TileObject> tileObjects;
        #endregion
        #region "Accessors"
        public Map OriginMap
        {
            get
            {
                return originMap;
            }
        }
        public int TileSet
        {
            get
            {
                return tileSet;
            }
        }
        public int TileImage
        {
            get
            {
                return tileImage;
            }
        }
        public int Layer
        {
            get
            {
                return layer;
            }
        }
        public Vector2 Location
        {
            get
            {
                return new Vector2(X, Y);
            }
        }
        public Vector2 WorldLocation
        {
            get
            {
                return new Vector2(XWorld, YWorld);
            }
        }
        public List<TileObject> TileObjects
        {
            get
            {
                return tileObjects;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a tiledata object.
        /// </summary>
        /// <param name="_originMap">The map the tile is from.</param>
        /// <param name="_Location">The location in tile space.</param>
        /// <param name="_WLocation">The location in world space.</param>
        /// <param name="_tileSet">The tileset this tile is from.</param>
        /// <param name="_tileImage">The image id within the tileset.</param>
        /// <param name="_layer">The layer the tile is on.</param>
        /// <param name="_tileObjects">A list of objects on this tile.</param>
        public TileData(Map _originMap, Vector2 _Location, Vector2 _WLocation, int _tileSet, int _tileImage, int _layer, List<TileObject> _tileObjects)
        {
            originMap = _originMap;
            X = (int) _Location.X;
            Y = (int) _Location.Y;
            XWorld = (int)_WLocation.X;
            YWorld = (int)_WLocation.Y;
            tileSet = _tileSet;
            tileImage = _tileImage;
            layer = _layer;
            tileObjects = _tileObjects;
        }

        /// <summary>
        /// Prints all data properties as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[TileData]\nTileLoc:" + Location.ToString() +
                "\nWLoc:" + WorldLocation.ToString() +
                "\nLayer:" + layer +
                "\nImage: " + tileImage +
                " on tileSet: " + tileSet +
                "\n{Objects:}\n" + string.Join("\n", tileObjects); 
        }
    }

    /// <summary>
    /// A tile data object.
    /// </summary>
    public class TileObject
    {
        #region "Declarations"
        public string Name;
        public string Type;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public int Location;
        #endregion

        /// <summary>
        /// Initializes a tile object.
        /// </summary>
        /// <param name="Name">The name of the object.</param>
        /// <param name="Properties">The object's properties.</param>
        /// <param name="X">The X location, will be used to calculate tile coordinate.</param>
        /// <param name="Y">The Y location, will be used to calculate tile coordinate.</param>
        public TileObject(string Name, Dictionary<string, string> Properties, string Type, int X, int Y, Vector2 TileSize, int mapWidth)
        {
            //Assign properties.
            this.Name = Name;
            this.Properties = Properties;
            this.Type = Type;

            //Calculate location.
            X /= (int) TileSize.X;
            Y /= (int) TileSize.Y;

            Location = Y * mapWidth + X % mapWidth;
        }
    }
}