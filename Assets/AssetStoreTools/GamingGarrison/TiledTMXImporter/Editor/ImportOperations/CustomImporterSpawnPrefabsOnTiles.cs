using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GamingGarrison
{
    /// <summary>
    /// Allows you to add a property called "unity:prefab" that spawns the prefab with a particular name on:
    /// 1) All the tiles in a tile layer
    /// 2) On an individual object
    /// 3) On all the objects in an object group
    /// (The prefab must be in the Assets folder, and the property value must match the prefab path e.g. "Prefabs/Cube" would match to a prefab in "Assets/Prefabs/Cube")
    /// 
    /// If you want the prefab to REPLACE an object or tile, use the property "unity:prefabReplace".  Using both properties will choose the replace version.
    /// </summary>
    public class CustomImporterSpawnPrefabsOnTiles : ITilemapImportOperation
    {

        /// <summary>
        /// If your prefabs are in a nested folder structure,
        /// and you don't want to type the whole path in Tiled every time,
        /// then change this function to your custom base folder.
        /// There is no validation done on this path, so make sure it ends with a slash '/' and make sure it's relative to your project directory.
        /// </summary>
        string GetBasePrefabSearchPath()
        {
            // e.g. return "Assets" + Path.AltDirectorySeparatorChar + "TMXPrefabs" + Path.AltDirectorySeparatorChar;
            return "Assets" + Path.AltDirectorySeparatorChar;
        }


        void SpawnPrefabOnTile(GameObject toSpawn, GameObject gameObject, Vector3 worldCoord)
        {
            GameObject newObject = PrefabUtility.InstantiatePrefab(toSpawn) as GameObject;
            if (newObject == null)
            {
                Debug.LogError("Prefab of object " + toSpawn + "could not be instantiated");
                return;
            }
            newObject.transform.SetParent(gameObject.transform, false);
            newObject.transform.position += worldCoord;
        }

        void SpawnPrefabOnObject(GameObject toSpawn, GameObject gameObject, bool replace)
        {
            GameObject newObject = PrefabUtility.InstantiatePrefab(toSpawn) as GameObject;
            if (newObject == null)
            {
                Debug.LogError("Prefab of object " + toSpawn + "could not be instantiated");
                return;
            }
            newObject.transform.SetParent(gameObject.transform, false);
            if (gameObject.GetComponent<Renderer>() != null)
            {
                newObject.transform.position = gameObject.GetComponent<Renderer>().bounds.center;
            }
            else if (gameObject.GetComponent<Collider>() != null)
            {
                newObject.transform.position = gameObject.GetComponent<Collider>().bounds.center;
            }
            if (replace)
            {
                newObject.transform.SetParent(gameObject.transform.parent, true);
                GameObject.DestroyImmediate(gameObject);
            }
        }

        public void HandleCustomProperties(GameObject gameObject, IDictionary<string, string> customProperties)
        {
            string prefabName;
            if (customProperties.ContainsKey("unity:prefab") || customProperties.ContainsKey("unity:prefabReplace"))
            {
                bool replace = false;
                if (customProperties.ContainsKey("unity:prefabReplace"))
                {
                    prefabName = customProperties["unity:prefabReplace"];
                    replace = true;
                }
                else
                {
                    prefabName = customProperties["unity:prefab"];
                }

                string toSpawnPath = GetBasePrefabSearchPath() + prefabName + ".prefab";
                GameObject toSpawn = AssetDatabase.LoadMainAssetAtPath(toSpawnPath) as GameObject;
                if (toSpawn == null)
                {
                    Debug.LogError("CustomImporterSpawnPrefabsOnTiles Could not find a prefab called " + prefabName + " in the assets folder at path " + toSpawnPath);
                    return;
                }
                Tilemap tilemap = gameObject.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    // Look for tile instances, and spawn a prefab on each tile
                    for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
                    {
                        for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
                        {
                            Vector3Int tileCoord = new Vector3Int(x, y, 0);
                            if (tilemap.HasTile(tileCoord))
                            {
                                Vector3 worldCoord = tilemap.layoutGrid.GetCellCenterWorld(tileCoord);
                                SpawnPrefabOnTile(toSpawn, gameObject, worldCoord);

                                if (replace)
                                {
                                    tilemap.SetTile(tileCoord, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Just spawn as a child of the object (or on each child if there are children)
                    if (gameObject.transform.childCount > 0)
                    {
                        GameObject[] children = new GameObject[gameObject.transform.childCount];
                        for (int i = 0; i < gameObject.transform.childCount; i++)
                        {
                            children[i] = gameObject.transform.GetChild(i).gameObject;
                        }
                        for (int i = 0; i < children.Length; i++)
                        {
                            SpawnPrefabOnObject(toSpawn, children[i], replace);
                        }
                    }
                    else
                    {
                        SpawnPrefabOnObject(toSpawn, gameObject, replace);
                    }
                    
                }
            }
        }
    }
}
