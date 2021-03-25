using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GamingGarrison
{
    public class TiledTXImporter : MonoBehaviour
    {
        public static ImportedTemplate LoadTXFile(string path, string tilesetDir, int cellWidth, int cellHeight, int pixelsPerUnit)
        {
            TXTypes.Template template = ImportUtils.ReadXMLIntoObject<TXTypes.Template>(path);

            ImportedTileset tileset;
            if (template.tileset != null)
            {
                string baseFolder = Path.GetDirectoryName(path);
                tileset = TiledTSXImporter.ImportFromTilesetReference(template.tileset, baseFolder, tilesetDir, cellWidth, cellHeight, pixelsPerUnit);
            }
            else
            {
                tileset = null;
            }

            return new ImportedTemplate(template, tileset);
        }
    }
}
