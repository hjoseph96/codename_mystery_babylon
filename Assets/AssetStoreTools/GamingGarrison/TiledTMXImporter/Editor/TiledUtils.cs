using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

namespace GamingGarrison
{
    public class TiledUtils
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public static void FindTileDataAndMatrix(uint gid, ImportedTileset[] importedTilesets, int cellWidth, int cellHeight,
            out ImportedTile importedTile, out TSX.Tile tilesetTile, out Matrix4x4 matrix)
        {
            importedTile = null;
            tilesetTile = null;
            matrix = Matrix4x4.identity;

            bool flippedHorizontally = (gid & FLIPPED_HORIZONTALLY_FLAG) != 0;
            bool flippedVertically = (gid & FLIPPED_VERTICALLY_FLAG) != 0;
            bool flippedDiagonally = (gid & FLIPPED_DIAGONALLY_FLAG) != 0;

            gid &= ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

            ImportedTileset tilesetContainingID = null;
            for (int j = importedTilesets.Length - 1; j >= 0; --j)
            {
                int firstGID = importedTilesets[j].firstGID;
                if (firstGID <= gid)
                {
                    tilesetContainingID = importedTilesets[j];
                    break;
                }
            }

            if (tilesetContainingID != null)
            {
                int relativeID = (int)gid - tilesetContainingID.firstGID;

                if (tilesetContainingID.tileset.IsSingleImageTileset())
                {
                    // A single-image-tileset will just order tiles from 0-n
                    if (relativeID >= tilesetContainingID.tiles.Length)
                    {
                        Debug.Log("RelativeID = " + relativeID);
                        return;
                    }
                    importedTile = tilesetContainingID.tiles[relativeID];
                    if (tilesetContainingID.tileset.tiles != null)
                    {
                        for (int t = 0; t < tilesetContainingID.tileset.tiles.Length; t++)
                        {
                            TSX.Tile tile = tilesetContainingID.tileset.tiles[t];
                            if (tile.id == relativeID)
                            {
                                tilesetTile = tile;
                            }
                        }
                    }
                    else
                    {
                        tilesetTile = null;
                    }
                }
                else
                {
                    for (int t = 0; t < tilesetContainingID.tileset.tiles.Length; t++)
                    {
                        TSX.Tile tile = tilesetContainingID.tileset.tiles[t];
                        int id = tile.id;
                        if (id == relativeID)
                        {
                            importedTile = tilesetContainingID.tiles[t];
                            tilesetTile = tile;
                            break;
                        }
                    }
                }

                if (importedTile != null)
                {
                    if (importedTile.tile == null) // Load the tile asset if we haven't already
                    {
                        importedTile.tile = AssetDatabase.LoadAssetAtPath<Tile>(importedTile.path);
                    }
                    if (flippedHorizontally || flippedVertically || flippedDiagonally)
                    {
                        if (flippedDiagonally)
                        {
                            matrix = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 0.0f, 90.0f));
                            matrix = Matrix4x4.Scale(new Vector3(1.0f, -1.0f, 1.0f)) * matrix;
                        }

                        if (flippedHorizontally)
                        {
                            matrix = Matrix4x4.Scale(new Vector3(-1.0f, 1.0f, 1.0f)) * matrix;
                        }
                        if (flippedVertically)
                        {
                            matrix = Matrix4x4.Scale(new Vector3(1.0f, -1.0f, 1.0f)) * matrix;
                        }

                        Rect rect = importedTile.tile.sprite.rect;
                        rect.x = -cellWidth * 0.5f;
                        rect.y = -cellHeight * 0.5f;

                        Vector2[] corners = new Vector2[4] {
                            new Vector2(rect.x, rect.y),
                            new Vector2(rect.x + rect.width, rect.y),
                            new Vector2(rect.x, rect.y + rect.height),
                            new Vector2(rect.x + rect.width, rect.y + rect.height)};

                        for (int i = 0; i < corners.Length; i++)
                        {
                            corners[i] = matrix * corners[i];
                        }
                        Vector2 bottomLeftCorner = corners[0];
                        for (int i = 1; i < corners.Length; i++)
                        {
                            if (corners[i].x < bottomLeftCorner.x)
                            {
                                bottomLeftCorner.x = corners[i].x;
                            }
                            if (corners[i].y < bottomLeftCorner.y)
                            {
                                bottomLeftCorner.y = corners[i].y;
                            }
                        }
                        Vector2 offsetNeededUnits = new Vector2(-0.5f, -0.5f) - new Vector2(bottomLeftCorner.x / (float)cellWidth, bottomLeftCorner.y / (float)cellHeight);
                        matrix = Matrix4x4.Translate(offsetNeededUnits) * matrix;
                    }
                }
            }
        }

        public static bool LoadDataFromPlainTiles(TMX.Tile[] tiles, int width, int height, out uint[] gIDData)
        {
            if (tiles.Length != width * height)
            {
                Debug.LogError("The plain tiles array length isn't equal to the width times height in the TMX layer");
                gIDData = null;
                return false;
            }
            gIDData = new uint[tiles.Length];

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null)
                {
                    Debug.LogError("Null plain tile detected");
                    return false;
                }
                gIDData[i] = tiles[i].gid;
            }

            return true;
        }

        public static bool LoadDataFromCSV(string csv, int width, int height, out uint[] gIDData)
        {
            string[] numbersAsStrings = csv.Split(',');
            if (numbersAsStrings.Length != width * height)
            {
                Debug.LogError("The CSV length isn't equal to the width times height in the TMX layer");
                gIDData = null;
                return false;
            }

            gIDData = new uint[numbersAsStrings.Length];

            uint value;
            for (int i = 0; i < numbersAsStrings.Length; i++)
            {
                bool worked = uint.TryParse(numbersAsStrings[i], out value);
                if (!worked)
                {
                    Debug.LogError("Could not parse GID " + numbersAsStrings[i]);
                    return false;
                }
                gIDData[i] = value;
            }

            return true;
        }

        public static bool LoadDataFromBytes(byte[] data, int width, int height, out uint[] gIDData)
        {
            if (data.Length != (width * height * 4))
            {
                Debug.LogError("The byte data length isn't equal to the width times height in the TMX layer * 4");
                gIDData = null;
                return false;
            }

            gIDData = new uint[data.Length / 4];
            for (int i = 0; i < gIDData.Length; i++)
            {
                int bytePos = i * 4;
                gIDData[i] = (uint)data[bytePos++] | (uint)data[bytePos++] << 8 | (uint)data[bytePos++] << 16 | (uint)data[bytePos++] << 24;
            }

            return true;
        }
        static void CreateSingleImagePaths(string relativeSourcePath, string tmxFolderPath, string spriteStorageDir, out string imageSourcePath, out string imageTargetPath)
        {
            imageSourcePath = relativeSourcePath;
            imageSourcePath = Path.Combine(tmxFolderPath, imageSourcePath);

            string imageName = Path.GetFileName(imageSourcePath);
            imageTargetPath = Path.Combine(spriteStorageDir, imageName);
        }

        public static Sprite ImportPathAsSprite(string tmxFolderPath, string relativeSourcePath, string storageFolder, int pixelsPerUnit)
        {
            string imageSourcePath;
            string imageTargetPath;
            CreateSingleImagePaths(relativeSourcePath, tmxFolderPath, storageFolder, out imageSourcePath, out imageTargetPath);
            TiledTSXImporter.CopyImages (new string[]{ imageSourcePath }, new string[]{ imageTargetPath });
            if (File.Exists (imageTargetPath))
            {
                TextureImporter ti = AssetImporter.GetAtPath(imageTargetPath) as TextureImporter;

                TextureImporterSettings textureSettings = new TextureImporterSettings();
                ti.ReadTextureSettings(textureSettings);

                SpriteMeshType meshType = SpriteMeshType.FullRect;
                SpriteAlignment alignment = SpriteAlignment.TopLeft;
                FilterMode filterMode = FilterMode.Point;

                if (textureSettings.spritePixelsPerUnit != pixelsPerUnit
                    || textureSettings.spriteMeshType != meshType
                    || textureSettings.spriteAlignment != (int)alignment
                    || textureSettings.filterMode != filterMode)
                {
                    textureSettings.spritePixelsPerUnit = pixelsPerUnit;
                    textureSettings.spriteMeshType = meshType;
                    textureSettings.spriteAlignment = (int)alignment;
                    textureSettings.filterMode = filterMode;
                    ti.SetTextureSettings(textureSettings);

                    EditorUtility.SetDirty(ti);
                    ti.SaveAndReimport();
                }
                Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite> (imageTargetPath);
                Debug.Assert (newSprite != null);
                return newSprite;
            }
            else
            {
                return null;
            }
        }

        // https://discourse.mapeditor.org/t/why-do-isometric-objects-get-miss-positioned-when-imported-into-visual-studio-using-xtiled/707
        public static Vector2 TiledIsometricPixelToScreenSpace(Vector2 isometricSpacePos, Vector2 tilePixelWidthAndHeight)
        {
            // Using height in both places on purpose here...
            Vector2 tileSpace = new Vector2(isometricSpacePos.x / tilePixelWidthAndHeight.y, isometricSpacePos.y / tilePixelWidthAndHeight.y);
            return TiledIsometricTileToScreenSpace(tileSpace, tilePixelWidthAndHeight);
        }

        public static Vector2 TiledIsometricTileToScreenSpace(Vector2 isometricSpacePos, Vector2 tilePixelWidthAndHeight)
        {
            Vector2 tilePixelWidthHeightHalf = tilePixelWidthAndHeight * 0.5f;
            Vector2 screenPos = new Vector2(
                (isometricSpacePos.x - isometricSpacePos.y),
                -(isometricSpacePos.x + isometricSpacePos.y));
            screenPos = Vector2.Scale(screenPos, tilePixelWidthHeightHalf);
            return screenPos;
        }
    }
}
