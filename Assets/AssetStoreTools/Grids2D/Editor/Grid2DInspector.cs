using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D {
    [CustomEditor (typeof(Grid2D))]
    public class Grids2DInspector : Editor {

        Grid2D grid;
        Texture2D _headerTexture, _blackTexture;
        string[] selectionModeOptions, topologyOptions, overlayModeOptions;
        int[] topologyOptionsValues;
        GUIStyle blackStyle, titleLabelStyle, infoLabelStyle;
        int cellSelectedIndex, cellHighlightedIndex = -1, cellTerritoryIndex, cellTextureIndex;
        Color colorSelection, cellColor;
        int textureMode, cellTag;
        static GUIStyle toggleButtonStyleNormal = null;
        static GUIStyle toggleButtonStyleToggled = null;
        SerializedProperty isDirty;

        void OnEnable () {

            _blackTexture = MakeTex (4, 4, EditorGUIUtility.isProSkin ? new Color (0.18f, 0.18f, 0.18f) : new Color (0.88f, 0.88f, 0.88f));
            _blackTexture.hideFlags = HideFlags.DontSave;
            _headerTexture = Resources.Load<Texture2D> ("EditorHeader");
            blackStyle = new GUIStyle ();
            blackStyle.normal.background = _blackTexture;

            selectionModeOptions = new string[] { "None", "Territories", "Cells" };
            overlayModeOptions = new string[] { "Overlay", "Ground" };
            topologyOptions = new string[] { "Irregular", "Box", "Hexagonal" };
            topologyOptionsValues = new int[] {
                (int)GRID_TOPOLOGY.Irregular,
                (int)GRID_TOPOLOGY.Box,
                (int)GRID_TOPOLOGY.Hexagonal
            };

            grid = (Grid2D)target;
            if (grid.territories == null) {
                grid.Init ();
            }

            colorSelection = new Color (1, 1, 0.5f, 0.85f);
            cellColor = Color.white;
            cellSelectedIndex = -1;
            isDirty = serializedObject.FindProperty ("isDirty");

            HideEditorMesh ();
        }

        public override void OnInspectorGUI () {
            EditorGUILayout.Separator ();
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
            GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;  

            EditorGUILayout.BeginVertical (blackStyle);

            EditorGUILayout.BeginHorizontal ();
            DrawTitleLabel ("Grid Configuration");
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Help")) {
                EditorUtility.DisplayDialog ("Grids 2D System", "Grids 2D is an advanced grid generator for Unity.\n\nFor a complete description of the options, please refer to the documentation guide (PDF) included in the asset.\nWe also invite you to visit and sign up on our support forum on kronnect.com where you can post your questions/requests.\n\nThanks for purchasing! Please rate Grids 2D on the Asset Store! Thanks.", "Close");
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Topology", GUILayout.Width (120));
            grid.gridTopology = (GRID_TOPOLOGY)EditorGUILayout.IntPopup ((int)grid.gridTopology, topologyOptions, topologyOptionsValues);
            EditorGUILayout.EndHorizontal ();

            if (grid.gridTopology == GRID_TOPOLOGY.Irregular) {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Cells (aprox.)", GUILayout.Width (120));
                grid.numCells = EditorGUILayout.IntSlider (grid.numCells, 2, Grid2D.MAX_CELLS);
                EditorGUILayout.EndHorizontal ();
            } else {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Columns", GUILayout.Width (120));
                grid.columnCount = EditorGUILayout.IntSlider (grid.columnCount, 2, Grid2D.MAX_CELLS_SQRT);
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Rows", GUILayout.Width (120));
                grid.rowCount = EditorGUILayout.IntSlider (grid.rowCount, 2, Grid2D.MAX_CELLS_SQRT);
                EditorGUILayout.EndHorizontal ();
            }
            if (grid.gridTopology == GRID_TOPOLOGY.Hexagonal) {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Regular Hexes", GUILayout.Width (120));
                grid.regularHexagons = EditorGUILayout.Toggle (grid.regularHexagons);
                EditorGUILayout.EndHorizontal ();
                if (grid.regularHexagons) {
                    EditorGUILayout.BeginHorizontal ();
                    GUILayout.Label ("   Hex Size", GUILayout.Width (120));
                    grid.hexSize = EditorGUILayout.FloatField (grid.hexSize);
                    EditorGUILayout.EndHorizontal ();
                }
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Even Layout", GUILayout.Width (120));
                grid.evenLayout = EditorGUILayout.Toggle (grid.evenLayout);
                EditorGUILayout.EndHorizontal ();
            }

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Curvature", GUILayout.Width (120));
            if (grid.numCells > Grid2D.MAX_CELLS_FOR_CURVATURE) {
                DrawInfoLabel ("not available with >" + Grid2D.MAX_CELLS_FOR_CURVATURE + " cells");
            } else {
                grid.gridCurvature = EditorGUILayout.Slider (grid.gridCurvature, 0, 0.1f);
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Relaxation", GUILayout.Width (120));
            if (grid.gridTopology != GRID_TOPOLOGY.Irregular) {
                DrawInfoLabel ("only available with irregular topology");
            } else if (grid.numCells > Grid2D.MAX_CELLS_FOR_RELAXATION) {
                DrawInfoLabel ("not available with >" + Grid2D.MAX_CELLS_FOR_RELAXATION + " cells");
            } else {
                grid.gridRelaxation = EditorGUILayout.IntSlider (grid.gridRelaxation, 1, 32);
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("Visibility Mask", "Alpha channel is used to determine cell visibility (0 = cell is not visible)"), GUILayout.Width (120));
            grid.gridMask = (Texture2D)EditorGUILayout.ObjectField (grid.gridMask, typeof(Texture2D), true);
            EditorGUILayout.EndHorizontal ();
            if (CheckTextureImportSettings (grid.gridMask)) {
                grid.ReloadGridMask ();
            }

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Seed", GUILayout.Width (120));
            grid.seed = EditorGUILayout.IntSlider (grid.seed, 1, 10000);
            if (GUILayout.Button ("Redraw")) {
                grid.Redraw ();
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Territories", GUILayout.Width (120));
            grid.enableTerritories = EditorGUILayout.Toggle (grid.enableTerritories);
            EditorGUILayout.EndHorizontal ();

            GUI.enabled = grid.enableTerritories;
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("   Count", GUILayout.Width (120));
            grid.numTerritories = EditorGUILayout.IntSlider (grid.numTerritories, 1, Mathf.Min (grid.numCells, Grid2D.MAX_TERRITORIES));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("   Region Texture", "Quickly create territories assigning a color texture in which each territory corresponds to a color."), GUILayout.Width (120));
            grid.territoriesTexture = (Texture2D)EditorGUILayout.ObjectField (grid.territoriesTexture, typeof(Texture2D), true);
            if (grid.territoriesTexture != null) {
                EditorGUILayout.EndHorizontal ();
                CheckTextureImportSettings (grid.territoriesTexture);
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label (new GUIContent ("  Neutral Color", "Color to be ignored."), GUILayout.Width (120));
                grid.territoriesTextureNeutralColor = EditorGUILayout.ColorField (grid.territoriesTextureNeutralColor, GUILayout.Width (50));
                EditorGUILayout.Space ();
                if (GUILayout.Button ("Generate Territories", GUILayout.Width (120))) {
                    if (grid.territoriesTexture == null) {
                        EditorUtility.DisplayDialog ("Missing territories texture!", "Assign a color texture to the territories texture slot.", "Ok");
                    } else {
                        grid.CreateTerritories (grid.territoriesTexture, grid.territoriesTextureNeutralColor);
                    }
                }
            }
            EditorGUILayout.EndHorizontal ();

            GUI.enabled = true;

            int cellsCreated = grid.cells == null ? 0 : grid.cells.Count;
            int territoriesCreated = grid.territories == null ? 0 : grid.territories.Count;

            EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            DrawInfoLabel ("Cells Created: " + cellsCreated + " / Territories Created: " + territoriesCreated + " / Vertex Count: " + grid.lastVertexCount);
            GUILayout.FlexibleSpace ();
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator ();
            EditorGUILayout.BeginVertical (blackStyle);

            DrawTitleLabel ("Grid Positioning");

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Hide Objects", GUILayout.Width (120));
            if (GUILayout.Button ("Toggle Grid")) {
                grid.gameObject.SetActive (!grid.gameObject.activeSelf);
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Depth Offset", GUILayout.Width (120));
            grid.gridDepthOffset = EditorGUILayout.IntSlider (grid.gridDepthOffset, -10, -1);
            EditorGUILayout.EndHorizontal ();


			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Camera", GUILayout.Width (120));
			grid.mainCamera = (Camera)EditorGUILayout.ObjectField (grid.mainCamera, typeof(Camera), true);
			EditorGUILayout.EndHorizontal ();

            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator ();
            EditorGUILayout.BeginVertical (blackStyle);

            DrawTitleLabel ("Grid Appearance");

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Show Territories", GUILayout.Width (120));
            grid.showTerritories = EditorGUILayout.Toggle (grid.showTerritories);
            if (grid.showTerritories) {
                GUILayout.Label ("Frontier Color", GUILayout.Width (120));
                grid.territoryFrontiersColor = EditorGUILayout.ColorField (grid.territoryFrontiersColor, GUILayout.Width (50));
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("  Highlight Color", GUILayout.Width (120));
            grid.territoryHighlightColor = EditorGUILayout.ColorField (grid.territoryHighlightColor, GUILayout.Width (50));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("  Colorize Territories", GUILayout.Width (120));
            grid.colorizeTerritories = EditorGUILayout.Toggle (grid.colorizeTerritories);
            GUILayout.Label ("Alpha");
            grid.colorizedTerritoriesAlpha = EditorGUILayout.Slider (grid.colorizedTerritoriesAlpha, 0.0f, 1.0f);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("  Outer Borders", GUILayout.Width (120));
            grid.showTerritoriesOuterBorders = EditorGUILayout.Toggle (grid.showTerritoriesOuterBorders);
            GUILayout.Label (new GUIContent ("Internal Territories", "Allows territories to be contained by other territories."), GUILayout.Width (110));
            grid.allowTerritoriesInsideTerritories = EditorGUILayout.Toggle (grid.allowTerritoriesInsideTerritories);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Show Cells", GUILayout.Width (120));
            grid.showCells = EditorGUILayout.Toggle (grid.showCells);
            if (grid.showCells) {
                GUILayout.Label ("Border Color", GUILayout.Width (120));
                grid.cellBorderColor = EditorGUILayout.ColorField (grid.cellBorderColor, GUILayout.Width (50));
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("  Highlight Color", GUILayout.Width (120));
            grid.cellHighlightColor = EditorGUILayout.ColorField (grid.cellHighlightColor, GUILayout.Width (50));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Highlight Fade", GUILayout.Width (120));
            grid.highlightFadeAmount = EditorGUILayout.Slider (grid.highlightFadeAmount, 0.0f, 1.0f);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Canvas Texture", GUILayout.Width (120));
            grid.canvasTexture = (Texture2D)EditorGUILayout.ObjectField (grid.canvasTexture, typeof(Texture2D), true);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Render Queue", GUILayout.Width(120));
            grid.renderQueue = (RENDER_QUEUE)EditorGUILayout.EnumPopup(grid.renderQueue);
            EditorGUILayout.EndHorizontal();

            if (grid.renderQueue == RENDER_QUEUE.Transparent) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Sorting Order", GUILayout.Width(120));
                grid.sortingOrder = EditorGUILayout.IntField(grid.sortingOrder);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator ();
            EditorGUILayout.BeginVertical (blackStyle);
				
            DrawTitleLabel ("Grid Behaviour");

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Selection Mode", GUILayout.Width (120));
            grid.highlightMode = (HIGHLIGHT_MODE)EditorGUILayout.Popup ((int)grid.highlightMode, selectionModeOptions);
            EditorGUILayout.EndHorizontal ();

            if (grid.highlightMode == HIGHLIGHT_MODE.Cells) {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  Include Invisible Cells", GUILayout.Width (120));
                grid.cellHighlightNonVisible = EditorGUILayout.Toggle (grid.cellHighlightNonVisible);
                EditorGUILayout.EndHorizontal ();
            }

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Overlay Mode", GUILayout.Width (120));
            grid.overlayMode = (OVERLAY_MODE)EditorGUILayout.Popup ((int)grid.overlayMode, overlayModeOptions);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Respect Other UI", GUILayout.Width (120));
            grid.respectOtherUI = EditorGUILayout.Toggle (grid.respectOtherUI);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator ();

            EditorGUILayout.BeginVertical (blackStyle);
            DrawTitleLabel ("Path Finding");
			
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Algorithm", GUILayout.Width (120));
            grid.pathFindingHeuristicFormula = (Grids2D.PathFinding.HeuristicFormula)EditorGUILayout.EnumPopup (grid.pathFindingHeuristicFormula);
            EditorGUILayout.EndHorizontal ();
			
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Max Search Cost", GUILayout.Width (120));
            grid.pathFindingMaxCost = EditorGUILayout.FloatField (grid.pathFindingMaxCost, GUILayout.Width (100));
            EditorGUILayout.EndHorizontal ();
			
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Max Steps", GUILayout.Width (120));
            grid.pathFindingMaxSteps = EditorGUILayout.IntField (grid.pathFindingMaxSteps, GUILayout.Width (100));
            EditorGUILayout.EndHorizontal ();

            if (grid.gridTopology == GRID_TOPOLOGY.Box) {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Use Diagonals", GUILayout.Width (120));
                grid.pathFindingUseDiagonals = EditorGUILayout.Toggle (grid.pathFindingUseDiagonals, GUILayout.Width (40));
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("   Diagonals Cost", GUILayout.Width (120));
                grid.pathFindingHeavyDiagonalsCost = EditorGUILayout.FloatField(grid.pathFindingHeavyDiagonalsCost, GUILayout.Width (60));
                EditorGUILayout.EndHorizontal ();
            }
			
            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator ();

            EditorGUILayout.BeginVertical (blackStyle);
			
            EditorGUILayout.BeginHorizontal ();
            DrawTitleLabel ("Grid Editor");
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Export Settings")) {
                if (EditorUtility.DisplayDialog ("Export Grid Settings", "This option will add a Grid Config component to this game object with current cell settings. You can restore this configuration just enabling this new component.", "Ok", "Cancel")) {
                    CreatePlaceholder ();
                }
            }
            if (GUILayout.Button ("Reset")) {
                if (EditorUtility.DisplayDialog ("Reset Grid", "Reset cells to their default values?", "Ok", "Cancel")) {
                    ResetCells ();
                    GUIUtility.ExitGUI ();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal ();
			
            if (cellSelectedIndex < 0 || grid.cells == null || cellSelectedIndex >= grid.cells.Count) {
                GUILayout.Label ("Click on a cell in Scene View to edit its properties.");
            } else {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("Selected Cell", GUILayout.Width (120));
                GUILayout.Label (cellSelectedIndex.ToString (), GUILayout.Width (120));
                EditorGUILayout.EndHorizontal ();
				
                bool needsRedraw = false;
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  Visible", GUILayout.Width (120));
                Cell selectedCell = grid.cells [cellSelectedIndex];
                bool cellVisible = selectedCell.visible;
                selectedCell.visible = EditorGUILayout.Toggle (cellVisible);
                if (selectedCell.visible != cellVisible) {

                    needsRedraw = true;
                }
                EditorGUILayout.EndHorizontal ();
				
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  Tag", GUILayout.Width (120));
                cellTag = EditorGUILayout.IntField (cellTag, GUILayout.Width (60));
                if (cellTag == selectedCell.tag)
                    GUI.enabled = false;
                if (GUILayout.Button ("Set Tag", GUILayout.Width (60))) {
                    grid.CellSetTag (cellSelectedIndex, cellTag);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  Territory Index", GUILayout.Width (120));
                cellTerritoryIndex = EditorGUILayout.IntField (cellTerritoryIndex, GUILayout.Width (40));
                if (cellTerritoryIndex == selectedCell.territoryIndex)
                    GUI.enabled = false;
                if (GUILayout.Button ("Set Territory", GUILayout.Width (100)) && cellTerritoryIndex != grid.cells [cellSelectedIndex].territoryIndex) {
                    grid.CellSetTerritory (cellSelectedIndex, cellTerritoryIndex);
                    needsRedraw = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal ();
				
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  Color", GUILayout.Width (120));
                cellColor = EditorGUILayout.ColorField (cellColor, GUILayout.Width (40));
                GUILayout.Label ("  Texture", GUILayout.Width (60));
                cellTextureIndex = EditorGUILayout.IntField (cellTextureIndex, GUILayout.Width (40));
                if (grid.CellGetColor (cellSelectedIndex) == cellColor && grid.CellGetTextureIndex (cellSelectedIndex) == cellTextureIndex)
                    GUI.enabled = false;
                if (GUILayout.Button ("Set", GUILayout.Width (40))) {
                    grid.CellToggle (cellSelectedIndex, true, cellColor, false, cellTextureIndex);
                    needsRedraw = true;
                }
                GUI.enabled = true;
                if (GUILayout.Button ("Clear", GUILayout.Width (40))) {
                    grid.CellHide (cellSelectedIndex);
                    needsRedraw = true;
                }
                EditorGUILayout.EndHorizontal ();
				
                if (needsRedraw) {
                    RefreshGrid ();
                    GUIUtility.ExitGUI ();
                    return;
                }
            }
			
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Textures", GUILayout.Width (120));
            EditorGUILayout.EndHorizontal ();
			
            if (toggleButtonStyleNormal == null) {
                toggleButtonStyleNormal = "Button";
                toggleButtonStyleToggled = new GUIStyle (toggleButtonStyleNormal);
                toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
            }
			
            int textureMax = grid.textures.Length - 1;
            while (textureMax >= 1 && grid.textures [textureMax] == null) {
                textureMax--;
            }
            textureMax++;
            if (textureMax >= grid.textures.Length)
                textureMax = grid.textures.Length - 1;
			
            for (int k = 1; k <= textureMax; k++) {
                EditorGUILayout.BeginHorizontal ();
                GUILayout.Label ("  " + k.ToString (), GUILayout.Width (40));
                grid.textures [k] = (Texture2D)EditorGUILayout.ObjectField (grid.textures [k], typeof(Texture2D), false);
                if (grid.textures [k] != null) {
                    if (GUILayout.Button (new GUIContent ("T", "Texture mode - if enabled, you can paint several cells just clicking over them."), textureMode == k ? toggleButtonStyleToggled : toggleButtonStyleNormal, GUILayout.Width (20))) {
                        textureMode = textureMode == k ? 0 : k;
                    }
                    if (GUILayout.Button (new GUIContent ("X", "Remove texture"), GUILayout.Width (20))) {
                        if (EditorUtility.DisplayDialog ("Remove texture", "Are you sure you want to remove this texture?", "Yes", "No")) {
                            grid.textures [k] = null;
                            GUIUtility.ExitGUI ();
                            return;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal ();
            }
			
            EditorGUILayout.EndVertical ();
            EditorGUILayout.Separator (); 
            if (grid.isDirty) {
#if UNITY_5_6_OR_NEWER
				serializedObject.UpdateIfRequiredOrScript();
																#else
                serializedObject.UpdateIfDirtyOrScript ();
#endif
                isDirty.boolValue = false;
                serializedObject.ApplyModifiedProperties ();
                EditorUtility.SetDirty (target);

                // Hide mesh in Editor
                HideEditorMesh ();
				
                SceneView.RepaintAll ();
            }
        }

        void OnSceneGUI () {
            if (grid == null || Application.isPlaying)
                return;
            Event e = Event.current;
            grid.CheckRay (HandleUtility.GUIPointToWorldRay (e.mousePosition));
            if (cellHighlightedIndex != grid.cellHighlightedIndex) {
                cellHighlightedIndex = grid.cellHighlightedIndex;
                SceneView.RepaintAll ();
            }
            int controlID = GUIUtility.GetControlID (FocusType.Passive);
            if (e.GetTypeForControl (controlID) == EventType.MouseDown) {
                if (cellHighlightedIndex != cellSelectedIndex) {
                    cellSelectedIndex = cellHighlightedIndex;
                    if (textureMode > 0) {
                        grid.CellToggle (cellSelectedIndex, true, Color.white, false, textureMode);
                        SceneView.RepaintAll ();
                    }
                    if (cellSelectedIndex >= 0) {
                        cellTerritoryIndex = grid.CellGetTerritoryIndex (cellSelectedIndex);
                        cellColor = grid.CellGetColor (cellSelectedIndex);
                        if (cellColor.a == 0)
                            cellColor = Color.white;
                        cellTextureIndex = grid.CellGetTextureIndex (cellSelectedIndex);
                        cellTag = grid.CellGetTag (cellSelectedIndex);
                    }
                    EditorUtility.SetDirty (target);
                }
            }
            if (cellSelectedIndex >= 0 && cellSelectedIndex < grid.cells.Count) {
                Vector3 pos = grid.CellGetPosition (cellSelectedIndex);
                Handles.color = colorSelection;
                Handles.DrawSolidDisc (pos, grid.transform.forward, HandleUtility.GetHandleSize (pos) * 0.075f);
            }
        }

        #region Utility functions

        void HideEditorMesh () {
            Renderer[] rr = grid.GetComponentsInChildren<Renderer> (true);
            for (int k = 0; k < rr.Length; k++) {
#if UNITY_5_5_OR_NEWER
                EditorUtility.SetSelectedRenderState (rr [k], EditorSelectedRenderState.Hidden);
#else
				EditorUtility.SetSelectedWireframeHidden (rr [k], true);
																#endif
            }
        }

        Texture2D MakeTex (int width, int height, Color col) {
            Color[] pix = new Color[width * height];
			
            for (int i = 0; i < pix.Length; i++)
                pix [i] = col;
			
            Texture2D result = new Texture2D (width, height);
            result.SetPixels (pix);
            result.Apply ();
			
            return result;
        }

        void DrawTitleLabel (string s) {
            if (titleLabelStyle == null)
                titleLabelStyle = new GUIStyle (GUI.skin.label);
            titleLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color (0.52f, 0.66f, 0.9f) : new Color (0.22f, 0.33f, 0.6f);
            titleLabelStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label (s, titleLabelStyle);
        }

        void DrawInfoLabel (string s) {
            if (infoLabelStyle == null)
                infoLabelStyle = new GUIStyle (GUI.skin.label);
            infoLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color (0.76f, 0.52f, 0.52f) : new Color (0.46f, 0.22f, 0.22f);
            GUILayout.Label (s, infoLabelStyle);
        }

        void ResetCells () {
            cellSelectedIndex = -1;
            cellColor = Color.white;
            grid.GenerateMap ();
            RefreshGrid ();
        }

        void RefreshGrid () {
            grid.Redraw ();
            HideEditorMesh ();
            EditorUtility.SetDirty (target);
            SceneView.RepaintAll ();
        }

        void CreatePlaceholder () {
            Grid2DConfig configComponent = grid.gameObject.AddComponent<Grid2DConfig> ();
            configComponent.textures = grid.textures;
            configComponent.config = grid.CellGetConfigurationData ();
            configComponent.enabled = false;
        }

        bool CheckTextureImportSettings(Texture2D tex) {
            if (tex == null)
                return false;
            string path = AssetDatabase.GetAssetPath (tex);
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath (path);
            if (!imp.isReadable) {
                EditorGUILayout.HelpBox ("Texture is not readable. Fix it?", MessageType.Warning);
                if (GUILayout.Button ("Fix texture import setting")) {
                    imp.isReadable = true;
                    imp.SaveAndReimport ();
                    return true;
                }
            }
            return false;
        }
        #endregion
    }

}