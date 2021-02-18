using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SS.Generic;

namespace SS.Cache
{
    public class CacheManager
    {
        const int MAX_CAPACITY = 200;

        static Dictionary<int, List<GameObject>> m_Cache = new Dictionary<int, List<GameObject>>();
        static Dictionary<int, List<GameObject>> m_Using = new Dictionary<int, List<GameObject>>();

        public static GameObject Instantiate(GameObject prefab)
        {
            // Get key id of prefab
            int keyId = GetKeyId(prefab);

            // Prefab pair dictionary keys
            MakeLists(keyId);

            // Result
            GameObject r = null;

            if (m_Cache[keyId].Count == 0)
            {
                // Normal Instantiate
                r = GameObject.Instantiate<GameObject>(prefab);

                // Set key id
                if (r.GetComponent<CacheKey>() == null)
                {
                    CacheKey key = r.AddComponent<CacheKey>();
                    key.id = keyId;
                }
            }
            else
            {
                // Remove from cache
                r = m_Cache[keyId][0];
                m_Cache[keyId].RemoveAt(0);
                m_Using[keyId].Add(r);

                // Active object
                r.SetActive(true);
            }

            return r;
        }

        public static void Destroy(GameObject go)
        {
            CacheKey key = go.GetComponent<CacheKey>();

            if (key != null)
            {
                // Get key id
                int keyId = key.id;

                if (m_Using.ContainsKey(keyId) && SmartList<GameObject>.Contains(m_Using[keyId], go))
                {
                    // Deactive object
                    go.SetActive(false);
                    go.transform.parent = null;

                    // Remove from using
                    SmartList<GameObject>.Remove(m_Using[keyId], go);

                    // Add to cache
                    m_Cache[keyId].Add(go);
                }
            }
            else
            {
                // If not in cache
                GameObject.Destroy(go);
            }
        }

        /*
        #if UNITY_EDITOR
        public static void OnHierarchyWindowChanged()
        {
            if (!Application.isPlaying)
            {
                GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

                for (int i = 0; i < objects.Length; i++)
                {
                    Object prefabPart = UnityEditor.PrefabUtility.GetPrefabParent(objects[i]);
                    if (prefabPart != null)
                    {
                        GameObject objectRoot = UnityEditor.PrefabUtility.FindPrefabRoot(objects[i]);
                        GameObject prefabRoot = UnityEditor.PrefabUtility.GetPrefabParent(objectRoot) as GameObject;

                        if (UnityEditor.PrefabUtility.GetPrefabType(prefabRoot) == UnityEditor.PrefabType.Prefab)
                        {
                            CacheKey key = prefabRoot.GetComponent<CacheKey>();
                            if (key == null)
                            {
                                key = prefabRoot.AddComponent<CacheKey>();
                                UnityEditor.EditorUtility.SetDirty(prefabRoot);
                            }
                            if (key.id == 0)
                            {
                                key.id = prefabRoot.GetInstanceID();
                                if (key.id == 0)
                                {
                                    key.id = int.MaxValue;
                                }
                                UnityEditor.EditorUtility.SetDirty(prefabRoot);
                            }
                        }
                    }
                }
            }
        }
        #endif
        */

        public static void Add(GameObject go, int keyId)
        {
            MakeLists(keyId);
            m_Using[keyId].Add(go);
        }

        public static void Remove(GameObject go, int keyId)
        {
            if (m_Using.ContainsKey(keyId) && SmartList<GameObject>.Contains(m_Using[keyId], go))
            {
                // Remove from using
                m_Using[keyId].Remove(go);

                return;
            }

            if (m_Cache.ContainsKey(keyId) && SmartList<GameObject>.Contains(m_Cache[keyId], go))
            {
                // Remove from cache
                m_Cache[keyId].Remove(go);

                return;
            }
        }

        public static void MoveAllUsingToCache()
        {
            foreach (var item in m_Using)
            {
                foreach (var go in item.Value)
                {
                    // Deactive object
                    go.SetActive(false);
                    go.transform.parent = null;

                    // Add to cache
                    m_Cache[item.Key].Add(go);
                }
            }

            m_Using.Clear();
        }

        public static void ClearCache()
        {
            // Clear cache
            foreach (var cache in m_Cache)
            {
                foreach (var item in cache.Value)
                {
                    GameObject.Destroy(item);
                }
            }
            m_Cache.Clear();

            // Warning if using
            if (m_Using.Count > 0)
            {
                Debug.LogWarning("Some of gameobjects which have been using will not be destroyed");
            }
        }

        public static void ClearAll(bool unloadResources, bool gcCollect)
        {
            MoveAllUsingToCache();
            ClearCache();

            if (unloadResources)
            {
                Resources.UnloadUnusedAssets();
            }

            if (gcCollect)
            {
                System.GC.Collect();
            }
        }

        private static void MakeLists(int keyId)
        {
            if (!m_Cache.ContainsKey(keyId))
            {
                m_Cache.Add(keyId, new List<GameObject>(MAX_CAPACITY));
            }

            if (!m_Using.ContainsKey(keyId))
            {
                m_Using.Add(keyId, new List<GameObject>(MAX_CAPACITY));
            }
        }

        private static int GetKeyId(GameObject prefab)
        {
            int id = 0;
            CacheKey key = prefab.GetComponent<CacheKey>();

            if (key == null)
            {
                id = prefab.GetInstanceID();

                if (id == 0)
                {
                    id = int.MaxValue;
                }
            }
            else
            {
                id = key.id;
            }

            return id;
        }
    }
}