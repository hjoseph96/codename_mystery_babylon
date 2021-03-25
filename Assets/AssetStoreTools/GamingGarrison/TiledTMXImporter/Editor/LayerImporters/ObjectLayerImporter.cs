using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GamingGarrison
{
    internal class ObjectLayerImporter
    {
        internal static Dictionary<string, ImportedTemplate> s_importedTemplates;

        internal static bool ImportObjectGroup(TMX.ObjectGroup objectGroup, ref int layerID, GameObject parent, out GameObject newObjectLayer)
        {
            newObjectLayer = null;
            if (objectGroup != null && objectGroup.objects != null && objectGroup.objects.Length > 0)
            {
                newObjectLayer = new GameObject(objectGroup.name);
                newObjectLayer.transform.SetParent(parent.transform, false);
                TiledTMXImporter.SetupLayerOffset(newObjectLayer, objectGroup.offsetx, objectGroup.offsety);

                for (int i = 0; i < objectGroup.objects.Length; i++)
                {
                    TMX.Object mapObject = objectGroup.objects[i];

                    bool success = ImportMapObject(mapObject, TiledTMXImporter.s_importedTilesets, newObjectLayer, TiledTMXImporter.s_map.tilewidth, TiledTMXImporter.s_map.tileheight, layerID);
                    if (!success)
                    {
                        return false;
                    }
                }
                Color objectColour = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                if (objectGroup.color != null)
                {
                    ColorUtility.TryParseHtmlString(objectGroup.color, out objectColour);
                }
                if (objectGroup.opacity != 1.0f)
                {
                    objectColour.a = objectGroup.opacity;
                }
                SpriteRenderer[] renderers = newObjectLayer.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer r in renderers)
                {
                    r.color = objectColour;
                }
            }
            return true;
        }

        static bool ImportMapObject(TMX.Object mapObject, ImportedTileset[] inImportedTilesets, GameObject newObjectLayer, int mapTileWidth, int mapTileHeight, int sortingLayer)
        {
            ImportedTileset replacementTileset = null;

            if (mapObject.template != null)
            {
                // Fill out empty object fields with data from the template.
                // The template could have a tile from it's own tileset reference, so in that case, use the template tileset instead, and the template object GID
                TMX.Object combinedMapObject = ApplyTemplate(mapObject, out replacementTileset);

                if (combinedMapObject == null)
                {
                    Debug.LogError("Could not load template for map object " + mapObject);
                    return false;
                }
                else
                {
                    mapObject = combinedMapObject;
                }
            }
            mapObject.InitialiseUnsetValues(); // We need special code to set defaults here, because setting them before merging with a template would give incorrect results.

            // Use the template's tileset (and the gid that's been set by ApplyTemplate)
            if (replacementTileset != null)
            {
                inImportedTilesets = new ImportedTileset[] { replacementTileset };
            }

            ImportedTile importedTile;
            TSX.Tile tilesetTile; // Unused
            Matrix4x4 matrix;
            TiledUtils.FindTileDataAndMatrix(mapObject.gid.Value, inImportedTilesets, TiledTMXImporter.s_cellWidth, TiledTMXImporter.s_cellHeight, out importedTile, out tilesetTile, out matrix);

            Vector2 pixelsToUnits = new Vector2(1.0f / (float)mapTileWidth, -1.0f / (float)mapTileHeight);

            GameObject newObject = new GameObject(mapObject.name);
            newObject.transform.SetParent(newObjectLayer.transform, false);

            // So we gain the tile rotation/flipping
            newObject.transform.FromMatrix(matrix);

            Vector2 corner = Vector2.Scale(new Vector2(mapObject.x.Value, mapObject.y.Value), pixelsToUnits);
            Vector2 pivotScaler = pixelsToUnits;
            if (TileLayerImporter.s_needsGridRotationToMatchUnityIsometric)
            {
                corner = TiledUtils.TiledIsometricPixelToScreenSpace(corner, new Vector2(mapTileWidth, mapTileHeight));
                pivotScaler = Vector2.zero; // Pivot offsets are unneeded for objects on isometric grids
            }

            if (importedTile != null)
            {
                Tile unityTile = importedTile.tile;
                Vector2 pivotProportion = new Vector2(unityTile.sprite.pivot.x / unityTile.sprite.rect.width, unityTile.sprite.pivot.y / unityTile.sprite.rect.height);
                Vector3 pivotWorldPosition = corner + Vector2.Scale(new Vector2(mapObject.width.Value * pivotProportion.x, mapObject.height.Value * -pivotProportion.y), pivotScaler);
                newObject.transform.localPosition += pivotWorldPosition;

                SpriteRenderer renderer = newObject.AddComponent<SpriteRenderer>();
                renderer.sprite = unityTile.sprite;
                renderer.sortingOrder = sortingLayer;
                if (unityTile.colliderType == Tile.ColliderType.Sprite)
                {
                    newObject.AddComponent<PolygonCollider2D>();
                }
                else if (unityTile.colliderType == Tile.ColliderType.Grid)
                {
                    newObject.AddComponent<BoxCollider2D>();
                }
                Vector2 scale = new Vector2(mapObject.width.Value / unityTile.sprite.rect.width, mapObject.height.Value / unityTile.sprite.rect.height);
                newObject.transform.localScale = Vector3.Scale(newObject.transform.localScale, new Vector3(scale.x, scale.y, 1.0f));
            }
            else
            {
                Vector3 pivotWorldPosition = corner + Vector2.Scale(new Vector2(mapObject.width.Value * 0.5f, mapObject.height.Value * 0.5f), pivotScaler);
                newObject.transform.localPosition += pivotWorldPosition;
                // If no tile used, must be a non-tile object of some sort (collision or text)
                if (mapObject.ellipse != null)
                {
                    EllipseCollider2D collider = newObject.AddComponent<EllipseCollider2D>();
                    collider.RadiusX = (mapObject.width.Value * 0.5f) / mapTileWidth;
                    collider.RadiusY = (mapObject.height.Value * 0.5f) / mapTileHeight;
                }
                else if (mapObject.polygon != null)
                {
                    PolygonCollider2D collider = newObject.AddComponent<PolygonCollider2D>();
                    string points = mapObject.polygon.points;
                    collider.points = ImportUtils.PointsFromString(points, pixelsToUnits);
                }
                else if (mapObject.polyline != null)
                {
                    EdgeCollider2D collider = newObject.AddComponent<EdgeCollider2D>();
                    string points = mapObject.polyline.points;
                    collider.points = ImportUtils.PointsFromString(points, pixelsToUnits);
                }
                else if (mapObject.text != null)
                {
                    TextMesh textMesh = newObject.AddComponent<TextMesh>();
                    textMesh.text = mapObject.text.text;
                    textMesh.anchor = TextAnchor.MiddleCenter;

                    Color color = Color.white;
                    if (mapObject.text.color != null)
                    {
                        ColorUtility.TryParseHtmlString(mapObject.text.color, out color);
                    }
                    textMesh.color = color;

                    // Saving an OS font as an asset in unity (through script) is seemingly impossible right now in a platform independent way
                    // So we'll skip fonts for now

                    textMesh.fontSize = (int)mapObject.text.pixelsize; // Guess a good resolution for the font
                    float targetWorldTextHeight = (float)mapObject.text.pixelsize / (float)mapTileHeight;
                    textMesh.characterSize = targetWorldTextHeight * 10.0f / textMesh.fontSize;

                    Renderer renderer = textMesh.gameObject.GetComponent<MeshRenderer>();
                    renderer.sortingOrder = sortingLayer;
                    renderer.sortingLayerID = SortingLayer.GetLayerValueFromName("Default");
                }
                else
                {
                    // Regular box collision
                    BoxCollider2D collider = newObject.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(mapObject.width.Value / mapTileWidth, mapObject.height.Value / mapTileHeight);
                }
            }

            if (mapObject.rotation != 0.0f)
            {
                newObject.transform.RotateAround(corner, new Vector3(0.0f, 0.0f, 1.0f), -mapObject.rotation.Value);
            }

            if (mapObject.visible == false)
            {
                if (TiledTMXImporter.s_setHiddenLayersToInactive)
                {
                    newObject.SetActive(false);
                }
                else
                {
                    Renderer renderer = newObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                }
            }

            TiledTMXImporter.HandleCustomProperties(newObject, mapObject.properties);
            return true;
        }

        static TMX.Object ApplyTemplate(TMX.Object mapObject, out ImportedTileset replacementTileset)
        {
            // Template path seems to be relative to the TMX
            string templatePath = Path.GetFullPath(Path.Combine(TiledTMXImporter.s_tmxParentFolder, mapObject.template));

            ImportedTemplate importedTemplate;
            if (s_importedTemplates.ContainsKey(templatePath))
            {
                importedTemplate = s_importedTemplates[templatePath];
            }
            else
            {
                importedTemplate = TiledTXImporter.LoadTXFile(templatePath, TiledTMXImporter.s_tilesetDir, TiledTMXImporter.s_cellWidth, TiledTMXImporter.s_cellHeight, TiledTMXImporter.s_pixelsPerUnit);
                s_importedTemplates.Add(templatePath, importedTemplate);
            }

            if (!mapObject.gid.HasValue)
            {
                replacementTileset = importedTemplate.m_importedTileset;
            }
            else
            {
                replacementTileset = null; // If the instance has a gid, assume it uses the normal map tilesets
            }
            return mapObject.GetVersionWithTemplateApplied(importedTemplate.m_template.templateObject);
        }
    }
}
