using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;
using System;

namespace GamingGarrison
{    
    public class TiledTMXImporter
    {
        internal static GameObject s_gridGO;
        internal static int s_cellWidth;
        internal static int s_cellHeight;
        internal static TMX.Map s_map;
        internal static string s_imageLayerSpriteDir;
        internal static int s_pixelsPerUnit;
        internal static string s_tmxParentFolder;
        internal static ImportedTileset[] s_importedTilesets;
        internal static ITilemapImportOperation[] s_importOperations;
        internal static string s_tilesetDir;
        internal static TilemapRenderer.SortOrder s_sortOrder;
        private static int s_orderInLayer;
        internal static bool s_setHiddenLayersToInactive;
        internal static bool s_forceGridColliders;
        internal static bool s_useCompositeColliders;
        internal static bool s_allowPrefabEdits;
        internal static string s_targetPrefabToSaveAs;

        /* Entry point into this class */
        public static bool ImportTMXFile(string path, string inTilesetDir, Grid targetGrid, string inImageLayerSpriteDir, bool setHiddenLayersToInactive, bool forceGridColliders, bool useCompositeColliders, bool allowPrefabEdits)
        {
            s_tmxParentFolder = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            s_imageLayerSpriteDir = inImageLayerSpriteDir;
            s_tilesetDir = inTilesetDir;
            s_orderInLayer = 0;
            s_setHiddenLayersToInactive = setHiddenLayersToInactive;
            s_forceGridColliders = forceGridColliders;
            s_useCompositeColliders = useCompositeColliders;
            s_allowPrefabEdits = allowPrefabEdits;
            s_targetPrefabToSaveAs = null;

            s_map = ImportUtils.ReadXMLIntoObject<TMX.Map>(path);
            if (s_map == null)
            {
                return false;
            }
            if (s_map.backgroundcolor != null)
            {
                Color backgroundColor;
                if (ColorUtility.TryParseHtmlString(s_map.backgroundcolor, out backgroundColor))
                {
                    Camera.main.backgroundColor = backgroundColor;
                }
            }
            if (s_map.tilesets != null)
            {
                // First we need to load (or import) all the tilesets referenced by the TMX file...
                s_cellWidth = s_map.tilewidth;
                s_cellHeight = s_map.tileheight;
                s_pixelsPerUnit = Mathf.Max(s_map.tilewidth, s_map.tileheight);
                s_importedTilesets = new ImportedTileset[s_map.tilesets.Length];
                for (int i = 0; i < s_map.tilesets.Length; i++)
                {
                    s_importedTilesets[i] = TiledTSXImporter.ImportFromTilesetReference(s_map.tilesets[i], s_tmxParentFolder, s_tilesetDir, s_cellWidth, s_cellHeight, s_pixelsPerUnit);
                    if (s_importedTilesets[i] == null || s_importedTilesets[i].tiles == null || s_importedTilesets[i].tiles[0] == null)
                    {
                        Debug.LogError("Imported tileset is incomplete");
                        return false;
                    }
                }

                // Setup statics
                // Unity hex grid only supports even stagger index, so we need a workaround to compensate
                TileLayerImporter.s_needsHexOddToEvenConversion = s_map.orientation == "hexagonal" && s_map.staggerindex == "odd";

                // Unity hex grid doesn't have a stagger axis option (only supports Y), so we need another workaround to compensate...
                // (we need to change the cell swizzle from XYZ to YXZ, and compensate by rotating and flipping the cells)
                TileLayerImporter.s_hexStaggerAxis = s_map.staggeraxis != null && s_map.staggeraxis.ToLowerInvariant() == "x" ? StaggerAxis.X : StaggerAxis.Y;

                // Unity's isometric rendering is 90 degrees rotated anti-clockwise on the grid compared to Tiled,
                // so we need to rotate the grid clockwise 90 degrees to look correct in Unity
                TileLayerImporter.s_needsGridRotationToMatchUnityIsometric = s_map.orientation == "isometric";

                // Set up the Grid to store everything in
                s_gridGO = PrepareGrid(filename, targetGrid);

                s_importOperations = ImportUtils.GetObjectsThatImplementInterface<ITilemapImportOperation>();
                s_sortOrder = TilemapRenderer.SortOrder.TopLeft;
                if (s_map.renderorder != null)
                {
                    if (s_map.renderorder.Equals("right-down"))
                    {
                        s_sortOrder = TilemapRenderer.SortOrder.TopLeft;
                    }
                    else if (s_map.renderorder.Equals("right-up"))
                    {
                        s_sortOrder = TilemapRenderer.SortOrder.BottomLeft;
                    }
                    else if (s_map.renderorder.Equals("left-down"))
                    {
                        s_sortOrder = TilemapRenderer.SortOrder.TopRight;
                    }
                    else if (s_map.renderorder.Equals("left-up"))
                    {
                        s_sortOrder = TilemapRenderer.SortOrder.BottomRight;
                    }
                }
                // Unity's isometric rendering only works well with TopRight sortorder
                if (s_map.orientation == "isometric")
                {
                    s_sortOrder = TilemapRenderer.SortOrder.TopRight;
                }


                ObjectLayerImporter.s_importedTemplates = new Dictionary<string, ImportedTemplate>();

                bool loadedLayers = false;
                if (s_map.topLevelLayers != null)
                {
                    loadedLayers = TreatLayers(s_gridGO, s_map.topLevelLayers);
                }
                else
                {
                    loadedLayers = true;
                }

                // Handle the complete map's properties
                if (loadedLayers)
                {
                    HandleCustomProperties(s_gridGO, s_map.properties);
                }
                if (s_targetPrefabToSaveAs != null)
                {
                    bool prefabSavedSuccessfully = false;
                    //Debug.Log("The importer was run on a prefab instance, so has tried to edit the prefab directly at " + s_targetPrefabToSaveAs);
                    PrefabUtility.SaveAsPrefabAsset(s_gridGO, s_targetPrefabToSaveAs, out prefabSavedSuccessfully);
                    if (!prefabSavedSuccessfully)
                    {
                        Debug.LogError("Importer failed to save the prefab to " + s_targetPrefabToSaveAs);
                    }
                    PrefabUtility.UnloadPrefabContents(s_gridGO);
                }
            }

            return true;
        }

        internal static void SetupLayerOffset(GameObject newLayer, float offsetX, float offsetY)
        {
            // offsety needs flipping because y+ is down in Tiled but up in Unity
            Grid targetGrid = s_gridGO.GetComponent<Grid>();
            newLayer.transform.localPosition = new Vector3 (offsetX * targetGrid.cellSize.x / (float)s_cellWidth, -offsetY * targetGrid.cellSize.y / (float)s_cellHeight, 0.0f);
        }

        static GameObject PrepareGrid(string filename, Grid targetGrid)
        {
            GameObject newGrid = null;
            if (targetGrid != null)
            {
                newGrid = targetGrid.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(newGrid))
                {
                    Debug.Log("We're importing into a prefab instance.  Unity 2018.3+ will complain about editing prefab instances, so we'll have to edit the prefab directly.");

                    if (s_allowPrefabEdits)
                    {
                        //GameObject rootOfInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(newGrid);
                        GameObject rootOfInstance = PrefabUtility.GetNearestPrefabInstanceRoot(newGrid);
                        if (rootOfInstance != newGrid.gameObject)
                        {
                            Debug.LogError("Importing into a prefab instance is only supported if the Grid is the top level object!");
                            return newGrid;
                        }
                        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(rootOfInstance);
                        if (prefab != null)
                        {
                            string prefabPath = AssetDatabase.GetAssetPath(prefab);
                            //Debug.Log("Found prefab: " + prefab + " at " + prefabPath);
                            GameObject loadedPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
                            newGrid = loadedPrefab;
                            s_targetPrefabToSaveAs = prefabPath;
                        }
                        else
                        {
                            Debug.LogError("You might have a disconnected prefab!  Unity will probably complain that you are Destroying objects in a prefab instance anyway.\n" +
                                "I'd suggest recreating the prefab or using the right click 'Unpack Completely' function to detach it altogether");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Unity will probably complain that you are Destroying objects in a prefab instance.\n" +
                            "Either toggle 'Allow Prefab Edits' to make the importer edit the prefab of the instance directly, or manually import into the prefab.");
                    }
                }
                for (int i = newGrid.transform.childCount - 1; i >= 0; --i)
                {
                    Undo.DestroyObjectImmediate(newGrid.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                newGrid = new GameObject(filename, typeof(Grid));
                Undo.RegisterCreatedObjectUndo(newGrid, "Import map to new Grid");
            }

            Grid newTileGrid = newGrid.GetComponent<Grid>();
            newTileGrid.cellSize = new Vector3(1.0f, 1.0f, 0.0f);
            
            switch (s_map.orientation)
            {
                case "orthogonal":
                    newTileGrid.cellLayout = GridLayout.CellLayout.Rectangle;
                    break;
                case "hexagonal":
                    newTileGrid.cellLayout = GridLayout.CellLayout.Hexagon;
                    newTileGrid.cellSize = new Vector3(1.0f, (float)s_map.tileheight / (float)s_map.tilewidth, 0.0f);
                    newTileGrid.cellSwizzle = TileLayerImporter.s_hexStaggerAxis == StaggerAxis.X ? GridLayout.CellSwizzle.YXZ : GridLayout.CellSwizzle.XYZ;
                    break;
                case "isometric":
                    newTileGrid.cellLayout = GridLayout.CellLayout.Isometric;
                    newTileGrid.cellSize = new Vector3(1.0f, (float)s_map.tileheight / (float)s_map.tilewidth, 0.0f);
                    break;
                default:
                    Debug.LogError("The TMX has an orientation of " + s_map.orientation + ", which is not yet supported :(");
                    break;
            }
            return newGrid;
        }

        static bool TreatLayers(GameObject parent, TMX.BaseLayerElement[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                TMX.BaseLayerElement layer = layers[i];
                GameObject layerObject;
                GenerateLayerObject(parent, layer, out layerObject);

                if (layerObject != null)
                {
                    if (!layer.visible)
                    {
                        if (s_setHiddenLayersToInactive)
                        {
                            layerObject.SetActive(false);
                        }
                        else
                        {
                            Renderer[] renderers = layerObject.GetComponentsInChildren<Renderer>();
                            foreach (Renderer r in renderers)
                            {
                                r.enabled = false;
                            }
                        }
                    }

                    HandleCustomProperties(layerObject, layer.properties);
                    Undo.RegisterCreatedObjectUndo(layerObject, "Import layer " + layerObject.name);
                }
            }
            return true;
        }
        internal static void HandleCustomProperties(GameObject gameObject, TMX.Properties tmxProperties)
        {
            IDictionary<string, string> properties = (tmxProperties == null ? new Dictionary<string, string>() : tmxProperties.ToDictionary());

            foreach (ITilemapImportOperation operation in s_importOperations)
            {
                operation.HandleCustomProperties(gameObject, properties);
            }
        }
        static bool GenerateLayerObject(GameObject parent, TMX.BaseLayerElement layer, out GameObject layerObject)
        {
            layerObject = null;
            if (layer is TMX.Layer)
            {
                TMX.Layer tmxLayer = layer as TMX.Layer;

                bool success = TileLayerImporter.ImportTileLayer(tmxLayer, parent, s_orderInLayer, s_map.infinite, out layerObject);
                s_orderInLayer++;
                if (!success)
                {
                    return false;
                }
            }
            else if (layer is TMX.ObjectGroup)
            {
                TMX.ObjectGroup objectGroup = layer as TMX.ObjectGroup;

                bool success = ObjectLayerImporter.ImportObjectGroup(objectGroup, ref s_orderInLayer, parent, out layerObject);
                s_orderInLayer++;
                if (!success)
                {
                    return false;
                }
            }
            else if (layer is TMX.ImageLayer)
            {
                if (!ImportUtils.CreateAssetFolderIfMissing(s_imageLayerSpriteDir, true))
                {
                    return false;
                }
                TMX.ImageLayer imageLayer = layer as TMX.ImageLayer;

                bool success = ImageLayerImporter.ImportImageLayer(imageLayer, parent, s_orderInLayer, out layerObject);
                s_orderInLayer++;
                if (!success)
                {
                    return false;
                }
            }
            else if (layer is TMX.GroupLayer)
            {
                TMX.GroupLayer groupLayer = layer as TMX.GroupLayer;

                GameObject newGroupLayer = new GameObject(groupLayer.name);
                newGroupLayer.transform.SetParent(parent.transform, false);
                layerObject = newGroupLayer;
                SetupLayerOffset(newGroupLayer, groupLayer.offsetx, groupLayer.offsety);

                if (groupLayer.childLayers != null)
                {
                    TreatLayers(newGroupLayer, groupLayer.childLayers);
                }
            }
            return true;
        }
    }
}
