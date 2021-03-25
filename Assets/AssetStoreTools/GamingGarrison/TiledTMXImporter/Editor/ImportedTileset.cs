using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class ImportedTileset
    {
        public ImportedTile[] tiles;
        public int firstGID;
        public TSX.Tileset tileset;

        public ImportedTileset(ImportedTile[] tilesIn, int firstGIDIn, TSX.Tileset tilesetIn)
        {
            tiles = tilesIn;
            firstGID = firstGIDIn;
            tileset = tilesetIn;
        }
    }
}
