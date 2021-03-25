using UnityEngine;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor 
{
	public class TileDataAttribute : PropertyAttribute
    {
        // Add tileset name as alternative to place the script in the tilemap gameObject

        public int tileSize;
        public bool displayProperties;

        public TileDataAttribute(int tileSize = 64, bool advanceEditor = false)
        {
            this.tileSize = tileSize;
            this.displayProperties = advanceEditor;
        }
    }
}
