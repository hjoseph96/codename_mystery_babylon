using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamingGarrison
{
    public class ImportedTile
    {
        public UnityEngine.Tilemaps.Tile tile;
        public string path;

        public ImportedTile(UnityEngine.Tilemaps.Tile tileIn, string pathIn)
        {
            tile = tileIn;
            path = pathIn;
        }
    }
}
