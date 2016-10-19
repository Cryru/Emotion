using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TiledSharp;
using System.IO;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, The TiledSharp Project                  //
    // https://github.com/marshallward/TiledSharp                               //
    //                                                                          //
    // An object that using the TiledSharp library allows the                   //
    // easy rendering and control of .tmx maps from the Tiled application.      //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Map
    {
        #region "Declarations"
        //Map Data.
        private TmxMap map; //The map as loaded by TiledSharp.
        private List<Texture> tileSets = new List<Texture>(); //The list of loaded textures to be used as tilesets.

        //Read-Only Accessors.
        public TmxMap TiledMap
        {
            get
            {
                return map;
            }
        }
        public int LoadedTileSets
        {
            get
            {
                return tileSets.Count;
            }
        }
        public float Width
        {
            get
            {
                return map.Width * map.TileWidth * tileScale;
            }
        }
        public float Height
        {
            get
            {
                return map.Height * map.TileHeight * tileScale;
            }
        }

        //Settings
        public string tilesetsContentPath = "Tilesets/"; //This path's root is the Content folder.
        public string mapsContentPath = "Content/SNcon/Maps/"; //This path's root is the .exe.
        public float tileScale = 1; //The scale at which the tiles should be rendered.
        public Vector2 Location = new Vector2(0, 0); //The origin location to draw the map from.
        #endregion

        //Initializer
        public Map(string mapName, float tileScale = 1)
        {
            //Check if empty.
            if(mapName == "")
            {
                return;
            }

            //Check if the map file exists.
#if !ANDROID
            if(File.Exists(mapsContentPath + mapName + ".tmx") == false)
            {
                return;
            }
#endif
     
            //Create a stream
#if WINDOWS
            Stream mapData = File.Open(mapsContentPath + mapName + ".tmx", FileMode.Open);
#endif
#if ANDROID
            Stream mapData = Core.androidHost.Assets.Open("SNCon/Maps/" + mapName + ".tmx");
#endif
#if __UNIFIED__ //Not actually tested. TODO
             Stream mapData = File.Open(mapsContentPath + mapName + ".tmx", FileMode.Open);
#endif
            //Load the map file.
            map = new TmxMap(mapData);
            //Load tilesets.
            for (int i = 0; i < map.Tilesets.Count; i++)
            {
                tileSets.Add(new Texture(tilesetsContentPath + map.Tilesets[i].Name.ToString()));
            }
            //Set the tile's scale.
            this.tileScale = tileScale;
        }

        //Draws the specified layers of the map.
        public void Draw(int StartLayer = 0, int EndingLayer = -1, Rectangle Limit = new Rectangle())
        {
            if (map == null) return;

            //Check if invalid ending layer.
            if(EndingLayer < 0 || EndingLayer > map.Layers.Count - 1)
            {
                EndingLayer = map.Layers.Count - 1;
            }
            //Check if invalid starting layer.
            if(StartLayer < 0 || StartLayer > map.Layers.Count - 1)
            {
                StartLayer = 0;
            }

            //Iterate through each layer.
            for (int layer = StartLayer; layer <= EndingLayer; layer++)
            {
                //Check if visible.
                if(map.Layers[layer].Visible == false)
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

                    //If out of view skip.
                    Rectangle tileBound = new Rectangle((int) offsetX, (int) offsetY, map.TileWidth, map.TileHeight);
                    if(Limit.Intersects(tileBound) == false)
                    {
                        if(Limit != new Rectangle())
                        {
                            continue;
                        }
                    }

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

                    int tWidth = map.Tilesets[tilesetID].TileWidth;
                    int tHeight = map.Tilesets[tilesetID].TileHeight;

                    int tilesetColumns = (int) map.Tilesets[tilesetID].Columns;

                    int tilesetWidth = (int)map.Tilesets[tilesetID].Image.Width;
                    int tilesetHeight = (int)map.Tilesets[tilesetID].Image.Height;

                    //Find the current tile.
                    int tileFrame = gID_offset - 1;
                    int column = tileFrame % tilesetColumns;
                    int row = (int)(tileFrame / (double)tilesetColumns);

                    Rectangle tilesetRec = new Rectangle(tWidth * column, tHeight * row, tWidth, tHeight);
                    //Add margins
                    tilesetRec.X += map.Tilesets[tilesetID].Margin;
                    tilesetRec.Y += map.Tilesets[tilesetID].Margin;
                    //Add spacing
                    tilesetRec.X += map.Tilesets[tilesetID].Spacing * column;
                    tilesetRec.Y += map.Tilesets[tilesetID].Spacing * row;


                    //Location of the map plus the offset of the current tile multiplied by the scale.
                    Core.ink.Draw(tileSets[tilesetID].Image, new Rectangle(
                        (int) (Location.X + offsetX * tileScale), 
                        (int) (Location.Y + offsetY * tileScale), 
                        (int) (tWidth * tileScale), 
                        (int) (tHeight * tileScale)), 
                        tilesetRec, Color.White); 
                }
            }
        }

        //Draw helpers.
        public void DrawUpTo(int LayerToStopAt)
        {
            Draw(0, LayerToStopAt);
        }
        public void DrawFrom(int LayerToStartAt)
        {
            Draw(LayerToStartAt);
        }

        //Converts a tile coordinate to a coordinate in pixel world space.
        public Vector2 TileCoordinateToWorldLocation(int TileCoordinate)
        {

            float offsetX = (TileCoordinate % map.Width) * map.TileWidth; //Calculate the X space.
            float offsetY = (float)Math.Floor(TileCoordinate / (double)map.Width) * map.TileHeight; //Calculate the Y space.

            return new Vector2(offsetX, offsetY); 
        }
        public Vector2 TileCoordinateToWorldLocation(Vector2 TileCoordinate)
        {
            return TileCoordinateToWorldLocation(TileLocationAsInt(TileCoordinate));
        }

        //Converts a coordinate in pixel world space to a tile coordinate.
        public int WorldLocationToTileCoordinate(Vector2 WorldCoordinate)
        {
            Rectangle selector = new Rectangle(WorldCoordinate.ToPoint(), new Point(1, 1));
            for (int i = 0; i < map.Layers[0].Tiles.Count; i++)
            {
                Vector2 TileWorldLocation = TileCoordinateToWorldLocation(i);

                if (selector.Intersects(new Rectangle(TileWorldLocation.ToPoint(), new Point(map.TileWidth, map.TileHeight))))
                {
                    return i;
                }

            }

            ////If not return an invalid value.
            return -1;
        }
        public Vector2 WorldLocationToTileCoordinate(Vector2 WorldCoordinate, bool ReturnVectorPos = true)
        {
            return TileLocationAsVector2(WorldLocationToTileCoordinate(WorldCoordinate));
        }

        //Returns a two dimensional vector from a one dimensional tile coordinate.
        public Vector2 TileLocationAsVector2(int TileCoordinate)
        {
            //Check for invalid values.
            if(TileCoordinate == -1)
            {
                return new Vector2(-1, -1);
            }

            int X = 1;
            int Y = 1;

            //Go through all tiles.
            for (int i = 0; i < TileCoordinate; i++)
            {
                //If the current tile we are counting is the last one on the line, increment the line.
                if(X == map.Width)
                {
                    Y++;
                    X = 1;
                }
                else
                {
                    X++;
                }
            }

            return new Vector2(X, Y);
        }

        //Returns an int location of a tile from a two dimensional vector.
        public int TileLocationAsInt(Vector2 TileCoordinate)
        {
            //Check for invalid values.
            if(TileCoordinate.X < 0 || TileCoordinate.Y < 0)
            {
                return -1;
            }
            if((int)(TileCoordinate.X - 1 + (TileCoordinate.Y - 1) * map.Width) > map.Layers[0].Tiles.Count)
            {
                return -1;
            }
            //Get the tile id by merging the X and Y into a single dimension.
            return (int)(TileCoordinate.X - 1 + (TileCoordinate.Y - 1) * map.Width);
        }

        //Returns the data for the selected tile coordinate.
        public TileData GetTileDataFromCoordinate(int TileCoordinate, int Layer)
        {

            //Check if layer is out of bounds.
            if (Layer > map.Layers.Count - 1 || TileCoordinate > map.Layers[Layer].Tiles.Count || TileCoordinate < 0)
            {
                return new TileData(this, new Vector2(), new Vector2(), -1, -1, -1);
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

            return new TileData(this, Location, WorldLocation, tilesetID, imageID, Layer);
        }
        public TileData GetTileDataFromCoordinate(Vector2 TileCoordinate, int Layer)
        {
            return GetTileDataFromCoordinate(TileLocationAsInt(TileCoordinate), Layer);
        }

        //Note: The tile X,Y locations are present within the map.layers[l].Tiles[t].X and Y properties while their int locations
        //are their index in the Tiles list. This information was uncovered after the writing of these functions, but if something breaks
        //the conversion functions and such should be switched out for these findings.
    }

    //An object to hold tile data.
    class TileData
    {
        private Map originMap; //The map this tile belongs to.
        private int X = -1; //The X coordinate of the tile, in map space.
        private int Y = -1; //The Y coordinate of the tile, in map space.
        private int XWorld = -1; //The X coordinate of the tile in world space.
        private int YWorld = -1; //The Y coordinate of the tile in world space.
        private int tileSet = -1; //The tileset this tile uses, from the map's tilesets.
        private int tileImage = -1; //The image of the tile, not as Tiled reports it, but as relative to the tileset.
        private int layer = -1; //The layer on which the tile is located.

        //Read-Only Accessors.
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

        //Constructor.
        public TileData(Map _originMap, Vector2 _Location, Vector2 _WLocation, int _tileSet, int _tileImage, int _layer)
        {
            originMap = _originMap;
            X = (int) _Location.X;
            Y = (int) _Location.Y;
            XWorld = (int)_WLocation.X;
            YWorld = (int)_WLocation.Y;
            tileSet = _tileSet;
            tileImage = _tileImage;
            layer = _layer;
        }

        public override string ToString()
        {
            return "[TileData]\nTileLoc:" + Location.ToString() + "\r\nWLoc:" + WorldLocation.ToString() + "\r\nLayer:" + layer + "\r\nImage: " + tileImage + " on tileSet: " + tileSet;
        }
    }
}