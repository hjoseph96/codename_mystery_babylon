using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GamingGarrison
{
    internal enum StaggerAxis
    {
        X = 0,
        Y = 1
    }
    internal class TileLayerImporter
    {
        internal static bool s_needsHexOddToEvenConversion;
        internal static StaggerAxis s_hexStaggerAxis;
        internal static bool s_needsGridRotationToMatchUnityIsometric;

        internal static bool ImportTileLayer(TMX.Layer layer, GameObject parent, int layerID, bool infinite, out GameObject newLayer)
        {
            newLayer = null;
            if (infinite != (layer.data.chunks != null))
            {
                Debug.LogError("Our map infinite setting is " + infinite + " but our chunks value is " + layer.data.chunks);
                return false;
            }
            newLayer = new GameObject(layer.name, typeof(Tilemap), typeof(TilemapRenderer));
            newLayer.transform.SetParent(parent.transform, false);
            TiledTMXImporter.SetupLayerOffset(newLayer, layer.offsetx, layer.offsety);
            Tilemap layerTilemap = newLayer.GetComponent<Tilemap>();
            Grid tileGrid = TiledTMXImporter.s_gridGO.GetComponent<Grid>();
            if (tileGrid.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                layerTilemap.tileAnchor = new Vector3(0.0f, 0.0f); // Seems to play better with how Unity renders the hex sprites
            }
            else if (tileGrid.cellLayout == GridLayout.CellLayout.Isometric || tileGrid.cellLayout == GridLayout.CellLayout.IsometricZAsY)
            {
                layerTilemap.tileAnchor = new Vector3(0.0f, -1.0f); // Isometric tile anchors are at the bottom of the tile
            }
            else
            {
                layerTilemap.tileAnchor = new Vector2(0.5f, 0.5f);
            }
            if (layer.opacity < 1.0f)
            {
                layerTilemap.color = new Color(1.0f, 1.0f, 1.0f, layer.opacity);
            }

            int gridXOffset = 0;
            int gridYOffset = 0;
            Vector3 offsetPosition = newLayer.transform.position;
            if (tileGrid.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                HandleHexagonOffsetting(tileGrid.cellSize, out gridXOffset, out gridYOffset, out offsetPosition);
            }
            newLayer.transform.position = offsetPosition;
            if (layer.data.chunks != null)
            {
                for (int c = 0; c < layer.data.chunks.Length; c++)
                {
                    TMX.Chunk chunk = layer.data.chunks[c];
                    bool success = AddChunkToTilemap(layerTilemap, layer.data.encoding, layer.data.compression, chunk.tiles, chunk.text,
                        chunk.x + gridXOffset, chunk.y + gridYOffset, chunk.width, chunk.height);
                    if (!success)
                    {
                        return false;
                    }
                }
            }
            else
            {
                bool success = AddChunkToTilemap(layerTilemap, layer.data.encoding, layer.data.compression, layer.data.tiles, layer.data.text,
                     gridXOffset, gridYOffset, layer.width, layer.height);
                if (!success)
                {
                    return false;
                }
            }

            TilemapRenderer renderer = newLayer.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = layerID;
            renderer.sortOrder = TiledTMXImporter.s_sortOrder;
            
            return true;
        }
        static void HandleHexagonOffsetting(Vector3 cellSize, out int gridXOffset, out int gridYOffset, out Vector3 offsetPosition)
        {
            if (s_needsHexOddToEvenConversion) // When odd
            {
                if (s_hexStaggerAxis == StaggerAxis.X)
                {
                    gridXOffset = 1;
                    gridYOffset = 0;
                    offsetPosition = new Vector3(cellSize.x * -0.25f, cellSize.y * 0.0f);
                }
                else // StaggerAxis Y
                {
                    gridXOffset = 0;
                    gridYOffset = 1;
                    offsetPosition = new Vector3(cellSize.x * 0.5f, cellSize.y * 1.0f);
                }
            }
            else // when even
            {
                if (s_hexStaggerAxis == StaggerAxis.X)
                {
                    gridXOffset = 1;
                    gridYOffset = 0;
                    offsetPosition = new Vector3(cellSize.x * -0.25f, cellSize.y * -0.5f);
                }
                else // StaggerAxis Y
                {
                    gridXOffset = 0;
                    gridYOffset = 0;
                    offsetPosition = new Vector3(cellSize.x * 0.5f, cellSize.y * 0.25f);
                }
            }
        }

        static bool FillTilemapFromData(Tilemap tilemap, int startX, int startY, int width, int height, uint[] data)
        {
            bool anyTilesWithCollision = false;
            for (int i = 0; i < data.Length; i++)
            {
                uint value = data[i];

                ImportedTile importedTile;
                TSX.Tile tilesetTile;
                Matrix4x4 matrix;
                TiledUtils.FindTileDataAndMatrix(value, TiledTMXImporter.s_importedTilesets, TiledTMXImporter.s_cellWidth, TiledTMXImporter.s_cellHeight, out importedTile, out tilesetTile, out matrix);

                if (importedTile != null && importedTile.tile != null)
                {
                    int x = startX + (i % width);
                    int y = -(startY + ((i / width) + 1));

                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (s_needsGridRotationToMatchUnityIsometric)
                    {
                        // Rotate 2D grid coordinates 90 degrees clockwise
                        pos.x = y;
                        pos.y = -x;
                    }
                    if (s_hexStaggerAxis == StaggerAxis.X)
                    {
                        // Rotate 2D grid coordinates 90 degrees clockwise but also flip on the x axis (relative to yxz cell swizzle)
                        pos.x = y;
                        pos.y = x;
                    }

                    tilemap.SetTile(pos, importedTile.tile);
                    tilemap.SetTransformMatrix(pos, matrix);

                    if (importedTile.tile.colliderType != Tile.ColliderType.None)
                    {
                        anyTilesWithCollision = true;
                    }
                }
                else if (value > 0)
                {
                    Debug.LogError("Could not find tile " + value + " in tilemap " + tilemap.name);
                    if (ImportUtils.s_validationMode)
                    {
                        return false;
                    }
                }
            }

            if (anyTilesWithCollision)
            {
                if (tilemap.gameObject.GetComponent<TilemapCollider2D>() == null)
                {
                    if (TiledTMXImporter.s_useCompositeColliders)
                    {
                        TilemapCollider2D collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
                        tilemap.gameObject.AddComponent<CompositeCollider2D>();
                        tilemap.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        collider.usedByComposite = true;
                    }
                    else
                    {
                        tilemap.gameObject.AddComponent<TilemapCollider2D>();
                    }
                }
            }

            return true;
        }

        static bool AddChunkToTilemap(Tilemap layerTilemap, string encoding, string compression, TMX.Tile[] plainTiles, string dataText,
            int x, int y, int width, int height)
        {
            uint[] gIDData = null;
            if (encoding == null)
            {
                TiledUtils.LoadDataFromPlainTiles(plainTiles, width, height, out gIDData);
            }
            else if (encoding.Equals("csv"))
            {
                if (!TiledUtils.LoadDataFromCSV(dataText, width, height, out gIDData))
                {
                    Debug.LogError("Layer data for layer " + layerTilemap.gameObject.name + " could not be csv decoded");
                    return false;
                }
            }
            else if (encoding.Equals("base64"))
            {
                byte[] decoded = Convert.FromBase64String(dataText);
                if (decoded == null)
                {
                    Debug.LogError("Layer data for layer " + layerTilemap.gameObject.name + " could not be base64 decoded");
                    return false;
                }
                if (compression != null)
                {
                    if (compression.Equals("zlib"))
                    {
                        decoded = ImportUtils.DecompressZLib(decoded);
                    }
                    else if (compression.Equals("gzip"))
                    {
                        decoded = ImportUtils.DecompressGZip(decoded);
                    }
                }
                if (!TiledUtils.LoadDataFromBytes(decoded, width, height, out gIDData))
                {
                    Debug.LogError("Layer data for layer " + layerTilemap.gameObject.name + " could not created from loaded byte data");
                    return false;
                }
            }

            if (gIDData == null)
            {
                Debug.LogError("Layer data for layer " + layerTilemap.gameObject.name + " could not be decoded");
                return false;
            }

            bool worked = FillTilemapFromData(layerTilemap, x, y, width, height, gIDData);
            return worked;
        }
    }
}
