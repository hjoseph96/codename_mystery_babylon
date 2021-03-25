using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace GamingGarrison
{
    public class TiledTMXImporterWindow : EditorWindow
    {
        string m_sourcePath;
        List<string> m_pathHistory;
        Grid m_targetGrid;
        string m_tilesetDir;
        string m_imageLayerSpriteDir = "ImageLayerSprites";
        bool m_validationMode;
        bool m_setHiddenLayersToInactive;
        bool m_forceGridColliders;
        bool m_useCompositeColliders;
        bool m_allowPrefabEdits;

        [MenuItem("Window/Tiled TMX Importer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<TiledTMXImporterWindow>(false, "TMX Importer", true);
        }

        public static string DropZone(string title, int w, int h, string extension)
        {
            GUILayout.Box(title, GUILayout.Width(w), GUILayout.Height(h));
            if ((DragAndDrop.paths == null || DragAndDrop.paths.Length == 0) && DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) // Don't want this to block regular object dragging
            {
                EditorGUILayout.HelpBox("Drag from your operating system, not from your Unity Project", MessageType.Warning);
                return null;
            }
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
            {
                return null;
            }
            if (DragAndDrop.paths.Length != 1)
            {
                EditorGUILayout.HelpBox("Drag a single file", MessageType.Warning);
                return null;
            }
            if (Path.GetExtension(DragAndDrop.paths[0]) != extension)
            {
                EditorGUILayout.HelpBox("Drag a file with the extension .tmx", MessageType.Warning);
                return null;
            }
            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.paths[0] : null;
        }

        string GetDefaultTilesetDir()
        {
            return Application.dataPath + Path.AltDirectorySeparatorChar + "TileSets";
        }

        private void OnEnable()
        {
            m_tilesetDir = GetDefaultTilesetDir();
            m_pathHistory = new List<string>();

            // but load preferences if we have valid ones
            string projectPath = null;
            SetIfExistsInPrefs(ref projectPath, "ProjectPath");
            if (projectPath != null && projectPath.Equals(Application.dataPath))
            {
                // Then the rest are valid
                SetIfExistsInPrefs(ref m_sourcePath, "TMXSourcePath");
                SetIfExistsInPrefs(ref m_tilesetDir, "TilesetDir");
                string pathHistoryString = null;
                SetIfExistsInPrefs(ref pathHistoryString, "TMXPathHistory");
                if (pathHistoryString != null)
                {
                    m_pathHistory = StringToStringList(pathHistoryString);
                }
            }
            SetIfExistsInPrefs(ref m_validationMode, "ValidationMode");
            SetIfExistsInPrefs(ref m_setHiddenLayersToInactive, "SetHiddenLayersToInactive");
            SetIfExistsInPrefs(ref m_forceGridColliders, "ForceGridColliders");
            SetIfExistsInPrefs(ref m_useCompositeColliders, "UseCompositeColliders");
            SetIfExistsInPrefs(ref m_allowPrefabEdits, "AllowPrefabEdits");

            RefreshSelectionAndGrid();

            EditorApplication.hierarchyChanged += RefreshSelectionAndGrid;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= RefreshSelectionAndGrid;
            EditorPrefs.SetString(GetFullPrefsKey("ProjectPath"), Application.dataPath); // If the base path has changed, the old path preferences values need invalidating
            EditorPrefs.SetString(GetFullPrefsKey("TMXSourcePath"), m_sourcePath);
            EditorPrefs.SetString(GetFullPrefsKey("TilesetDir"), m_tilesetDir);
            EditorPrefs.SetString(GetFullPrefsKey("TMXPathHistory"), StringListToString(m_pathHistory));
            EditorPrefs.SetBool(GetFullPrefsKey("ValidationMode"), m_validationMode);
            EditorPrefs.SetBool(GetFullPrefsKey("SetHiddenLayersToInactive"), m_setHiddenLayersToInactive);
            EditorPrefs.SetBool(GetFullPrefsKey("ForceGridColliders"), m_forceGridColliders);
            EditorPrefs.SetBool(GetFullPrefsKey("UseCompositeColliders"), m_useCompositeColliders);
            EditorPrefs.SetBool(GetFullPrefsKey("AllowPrefabEdits"), m_allowPrefabEdits);
        }

        string StringListToString(List<string> list)
        {
            List<string> shortenedList = new List<string>(list);
            int needRemoving = shortenedList.Count - 5;
            if (needRemoving > 0)
            {
                shortenedList.RemoveRange(0, needRemoving);
            }
            string totalString = "";
            for(int i = 0; i < shortenedList.Count; i++)
            {
                if (i > 0)
                {
                    totalString += "|";
                }
                totalString += shortenedList[i];
            }
            return totalString;
        }
        List<string> StringToStringList(string totalString)
        {
            string[] stringArray = totalString.Split('|');
            List<string> list = new List<string>(stringArray);
            return list;
        }

        void SetIfExistsInPrefs(ref string target, string keyName)
        {
            string fullKey = GetFullPrefsKey(keyName);
            if (EditorPrefs.HasKey(fullKey))
            {
                target = EditorPrefs.GetString(fullKey);
            }
        }

        void SetIfExistsInPrefs(ref bool target, string keyName)
        {
            string fullKey = GetFullPrefsKey(keyName);
            if (EditorPrefs.HasKey(fullKey))
            {
                target = EditorPrefs.GetBool(fullKey);
            }
        }

        String GetFullPrefsKey(string name)
        {
            return "GamingGarrison.TiledTMXImporter." + name;
        }

        void RefreshSelectionAndGrid()
        {
            Grid[] gridsInScene = FindObjectsOfType<Grid>();
            if (gridsInScene != null && gridsInScene.Length == 1)
            {
                m_targetGrid = gridsInScene[0];
            }
            
            foreach(Grid grid in gridsInScene)
            {
                grid.enabled = !grid.enabled;
                grid.enabled = !grid.enabled;  //Turn it off and on again to work-around the fact that Unity's Grid doesn't update on a redo
            }
        }

        void DoHistoryDropdown()
        {
            if (m_pathHistory != null && m_pathHistory.Count > 0)
            {
                string[] options = m_pathHistory.ToArray();
                for(int i = 0; i < options.Length; i++)
                {
                    options[i] = options[i].Replace('/', '\\');
                }
                int selected = m_pathHistory.IndexOf(m_sourcePath);
                selected = EditorGUILayout.Popup("History:", selected, options);
                if (selected >= 0)
                {
                    m_sourcePath = m_pathHistory[selected];
                }
            }
        }

        void OnGUI()
        {
            GUILayout.Label(new GUIContent("Drop a TMX file below:", "Drag and drop from your operating system, not from Unity"), EditorStyles.boldLabel);

            string path = DropZone("Drop here", 100, 100, ".tmx");
            if (path != null)
            {
                m_sourcePath = path;
            }

            bool pathExists = false;

            if (m_sourcePath != null && m_sourcePath.Length > 0)
            {
                pathExists = true;
                if (!File.Exists(m_sourcePath))
                {
                    pathExists = false;
                }

                if (pathExists && !m_pathHistory.Contains(m_sourcePath))
                {
                    m_pathHistory.Add(m_sourcePath);
                }

                GUILayout.Label(new GUIContent(m_sourcePath, m_sourcePath));

                if (GUILayout.Button(new GUIContent("Clear Path", "De-select the currently dropped TMX file")))
                {
                    m_sourcePath = null;
                }
            }

            DoHistoryDropdown();

            m_targetGrid = EditorGUILayout.ObjectField(new GUIContent("Target Tilemap Grid:", "A grid in the scene to place the map"), m_targetGrid, typeof(Grid), true) as Grid;
            GUILayout.Label("(Leave this empty to spawn a new Grid in the scene)");
            GUILayout.Space(10);

            GUILayout.Label(new GUIContent("Target Tileset directory: " + m_tilesetDir, m_tilesetDir));
            if (GUILayout.Button(new GUIContent("Change tileset directory", "Change where the generated Unity Sprite and Tile assets will be placed")))
            {
                string newTilesetDir = EditorUtility.OpenFolderPanel("Choose tileset directory", "Assets", "TileSets");
                if (newTilesetDir != null && newTilesetDir.Length > 0)
                {
                    m_tilesetDir = newTilesetDir;
                    AssetDatabase.Refresh();
                }
            }
            if (!m_tilesetDir.Equals(GetDefaultTilesetDir()))
            {
                if (GUILayout.Button(new GUIContent("Choose default tileset directory", "Reset target tileset directory to the default of " + GetDefaultTilesetDir())))
                {
                    m_tilesetDir = GetDefaultTilesetDir();
                }
            }
            GUILayout.Space(10);

            bool tilesetDirValid = m_tilesetDir != null && m_tilesetDir.Length > 0 && m_tilesetDir.StartsWith(Application.dataPath) && m_tilesetDir.Length > Application.dataPath.Length;

            if (pathExists && tilesetDirValid)
            {
                GUI.backgroundColor = Color.green;
                GUI.enabled = true;
            }
            else
            {
                if (!pathExists)
                {
                    EditorGUILayout.HelpBox("Drop a TMX file from your operating system", MessageType.Warning);
                }
                else if (!tilesetDirValid)
                {
                    if (!m_tilesetDir.StartsWith(Application.dataPath))
                    {
                        EditorGUILayout.HelpBox("Imported tileset folder must be inside the project assets folder", MessageType.Warning);
                    }
                    else if (m_tilesetDir.Length <= Application.dataPath.Length)
                    {
                        EditorGUILayout.HelpBox("Imported tileset folder must be a sub-directory of the project assets folder", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Select a folder to store the imported tilesets", MessageType.Warning);
                    }
                }
                GUI.enabled = false;
                GUI.backgroundColor = Color.red;
            }
            if (GUILayout.Button(new GUIContent("Import", "Import the chosen TMX and any referenced TSX Tilesets")))
            {
                EditorApplication.delayCall += DelayedCall;
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            m_validationMode = GUILayout.Toggle(m_validationMode, new GUIContent("Data Validation Mode", "Output error messages and cancel import if the TMX file contains data the importer doesn't recognise"));
            m_setHiddenLayersToInactive = GUILayout.Toggle(m_setHiddenLayersToInactive, new GUIContent("Set hidden layers to inactive", "Make layers hidden in Tiled to be inactive in the scene rather than just invisible"));
            m_forceGridColliders = GUILayout.Toggle(m_forceGridColliders, new GUIContent("Force Grid Colliders", "All imported tiles with collision data will be forced to 'Grid' collider type rather than 'Sprite'"));
            m_useCompositeColliders = GUILayout.Toggle(m_useCompositeColliders, new GUIContent("Use Composite Colliders", "If a tile layer has collision, should it use a composite collider?"));
            m_allowPrefabEdits = GUILayout.Toggle(m_allowPrefabEdits, new GUIContent("Allow Prefab Edits", "When importing into a prefab instance, Unity 2018.3+ will throw the error\n" +
                "'InvalidOperationException: Destroying a GameObject inside a Prefab instance is not allowed.'\n" +
                "To avoid this, either perform the import in Prefab Edit Mode, with the prefab's Grid selected, or allow the importer to perform the edits directly to the prefab"));
        }

        void DelayedCall()
        {
            EditorApplication.delayCall -= DelayedCall;
            DoImport();
        }

        void DoImport()
        {
            EditorUtility.UnloadUnusedAssetsImmediate(true); // To test
            string relativeTilesetDir = m_tilesetDir.Substring(Application.dataPath.Length);
            relativeTilesetDir = "Assets" + Path.DirectorySeparatorChar + relativeTilesetDir.TrimStart('/', '\\');
            ImportUtils.s_validationMode = m_validationMode;
            string imageLayerSpriteDir = "Assets" + Path.DirectorySeparatorChar + m_imageLayerSpriteDir.TrimStart('/','\\');
            bool success = TiledTMXImporter.ImportTMXFile(m_sourcePath, relativeTilesetDir, m_targetGrid, imageLayerSpriteDir, m_setHiddenLayersToInactive, m_forceGridColliders, m_useCompositeColliders, m_allowPrefabEdits);
            if (success)
            {
                Debug.Log("Import succeeded!");
                RefreshSelectionAndGrid();
            }
            else
            {
                Debug.Log("Import failed :(");
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
