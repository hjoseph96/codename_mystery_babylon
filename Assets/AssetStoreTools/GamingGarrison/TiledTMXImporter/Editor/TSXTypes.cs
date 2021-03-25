using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;

namespace GamingGarrison
{
    /// <summary>
    /// http://doc.mapeditor.org/en/stable/reference/tmx-map-format/
    /// </summary>
    namespace TSX
    {
        [XmlRoot(ElementName = "tileset")]
        public class Tileset
        {
            [XmlAttribute]
            public float version;

            [XmlAttribute]
            public string tiledversion;

            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public int tilewidth;

            [XmlAttribute]
            public int tileheight;

            [XmlAttribute]
            public int spacing;

            [XmlAttribute]
            public int margin;

            /// <summary>
            /// The XML can give a tilecount of 0, so to get a predicted tilecount in those cases, use GetTileCount()
            /// </summary>
            [XmlAttribute]
            public int tilecount;

            [XmlAttribute]
            public int columns;

            [XmlElement]
            public Grid grid;

            [XmlElement]
            public TerrainTypes terraintypes;

            [XmlElement(ElementName = "tile")]
            public Tile[] tiles;

            [XmlElement]
            public TSX.Image image;

            public Tileset(TMX.TilesetReference embeddedTileset)
            {
                this.name = embeddedTileset.name;
                this.tilewidth = embeddedTileset.tilewidth;
                this.tileheight = embeddedTileset.tileheight;
                this.tilecount = embeddedTileset.tilecount;
                this.columns = embeddedTileset.columns;
                this.grid = embeddedTileset.grid;
                this.tiles = embeddedTileset.tiles;
                this.image = embeddedTileset.image;
            }

            public Tileset()
            {

            }

            public bool IsSingleImageTileset()
            {
                return tiles == null || image != null;
            }
            public int GetTileCountGivenDimensions(int width, int height)
            {
                // This calculation comes from isolating n in "imageWidth = 2*margin + tileWidth * n + spacing * (n-1)"
                int tilesAcross = (width + spacing - margin * 2) / (spacing + tilewidth);
                int tilesDown = (height + spacing - margin * 2) / (spacing + tileheight);
                return tilesAcross * tilesDown;
            }
            public int GetTileCount()
            {
                if (tilecount > 0)
                {
                    return tilecount;
                }
                else
                {
                    // If tilecount isn't given in the XML, then we just have to assume a tile count...
                    return GetTileCountGivenDimensions(image.width, image.height);
                }
            }
        }

        public class Grid
        {
            [XmlAttribute]
            public string orientation;

            [XmlAttribute]
            public int width;

            [XmlAttribute]
            public int height;
        }

        public class TerrainTypes
        {
            [XmlElement(ElementName = "terrain")]
            public Terrain[] terrain;
        }

        public class Terrain
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public int tile;

            [XmlElement]
            public TMX.Properties properties;
        }

        // I've put this annotation on the types that have the same name as TMXTypes,
        // as .net 4.6 would complain.  Namespacing or using different type names would break the parsing.
        [XmlType(AnonymousType = true)]
        public class Tile
        {
            [XmlAttribute]
            public int id;

            /// <summary>
            /// Comma-separated indexes in the terrain types array in the order top-left, top-right, bottom-left, bottom-right.
            /// Leaving out a value means that corner has no terrain. (optional)
            /// As far as I'm aware, Tiled outputs the raw tile ids, not terrain ids, so shouldn't be needed in the runtime
            /// </summary>
            [XmlAttribute]
            public string terrain;

            [XmlAttribute]
            public float probability;

            [XmlElement]
            public Image image;

            [XmlElement]
            public ObjectGroup objectgroup;

            [XmlElement]
            public Animation animation;

            [XmlElement]
            public TMX.Properties properties;

            public bool HasCollisionData()
            {
                return objectgroup != null && objectgroup.objects != null && objectgroup.objects.Length > 0;
            }
        }

        public class TileOffset
        {
            [XmlAttribute]
            public int x;

            [XmlAttribute]
            public int y;
        }

        public class Image
        {
            [XmlAttribute]
            public int width;

            [XmlAttribute]
            public int height;

            [XmlAttribute]
            public string source;

            /// <summary>
            /// Defines a specific colour that is treated as transparent (example value: "#FF00FF" for magenta).
            /// Up until Tiled 0.12, this value is written out without a # but this is planned to change
            /// </summary>
            [XmlAttribute]
            public string trans;
        }

        [XmlType(AnonymousType = true)]
        public class ObjectGroup
        {
            [XmlAttribute]
            public int id;

            [XmlAttribute]
            public string draworder = "topdown";

            [XmlAttribute]
            public string color;

            [XmlAttribute]
            public float opacity = 1.0f;

            [XmlElement(ElementName = "object")]
            public TMX.Object[] objects;

            [XmlElement]
            public TMX.Properties properties;
        }

        public class Animation
        {
            [XmlElement(ElementName = "frame")]
            public Frame[] frames;
        }

        public class Frame
        {
            /// <summary>
            /// The local ID of a tile within the parent tileset
            /// </summary>
            [XmlAttribute]
            public int tileid;

            /// <summary>
            /// How long (in milliseconds) this frame should be displayed before advancing to the next frame
            /// </summary>
            [XmlAttribute]
            public int duration;
        }

        public class WangSets
        {
            [XmlElement(ElementName = "wangset")]
            public WangSet[] wangsets;
        }

        public class WangSet
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public int tile;

            [XmlElement]
            public TMX.Properties properties;

            [XmlElement(ElementName = "wangcornercolor")]
            public WangCornerColor[] wangcornercolors;

            [XmlElement(ElementName = "wangedgecolor")]
            public WangEdgeColor[] wangedgecolors;

            [XmlElement(ElementName = "wangtile")]
            public WangTile[] wangtiles;
        }

        public class WangCornerColor
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string color;

            [XmlAttribute]
            public int tile;

            [XmlAttribute]
            public int probability;
        }

        public class WangEdgeColor
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string color;

            [XmlAttribute]
            public int tile;

            [XmlAttribute]
            public int probability;
        }

        public class WangTile
        {
            [XmlAttribute]
            public int tileid;

            [XmlAttribute]
            public string wangid;

            [XmlAttribute]
            public bool hflip = false;

            [XmlAttribute]
            public bool vflip = false;

            [XmlAttribute]
            public bool dflip = false;
        }
    }
}
