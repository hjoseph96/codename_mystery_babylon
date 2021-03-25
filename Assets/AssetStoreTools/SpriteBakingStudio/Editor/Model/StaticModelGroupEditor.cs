using System.IO;
using UnityEngine;
using UnityEditor;

namespace SBS
{
    [CustomEditor(typeof(StaticModelGroup))]
    public class StaticModelGroupEditor : StudioModelEditor
    {
        private StaticModelGroup group = null;

        void OnEnable()
        {
            group = (StaticModelGroup)target;

            if (group.rootDirectory != null && group.rootDirectory.Length > 0 && Directory.Exists(group.rootDirectory))
                group.RefreshModels();
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            if (group == null)
                return;

            Undo.RecordObject(group, "Static Model Group");

            DrawRootDirectoryFields();

            EditorGUILayout.Space();

            DrawModelToggles();

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            DrawPivotOffsetFields(group);
            if(EditorGUI.EndChangeCheck())
            {
                foreach (StaticModelPair pair in group.modelPairs)
                {
                    pair.Model.pivotOffset = group.pivotOffset;
                    pair.Model.directionDependentPivot = group.directionDependentPivot;
                }
            }

            EditorGUILayout.Space();

            if (group.GetCheckedModels().Count == 0)
            {
                EditorGUILayout.HelpBox("At least one sub model should be checked!", MessageType.Error);
                return;
            }

            DrawModelSettingButton(group);
        }

        private void DrawRootDirectoryFields()
        {
            EditorGUI.BeginChangeCheck();
            group.rootDirectory = EditorGUILayout.TextField("Objects Root Directory", group.rootDirectory);
            bool needRefresh = EditorGUI.EndChangeCheck();

            EditorGUILayout.BeginHorizontal();
            {
                if (DrawingUtils.DrawNarrowButton("Choose Directory"))
                {
                    group.rootDirectory = EditorUtility.OpenFolderPanel("Choose a directory",
                        (group.rootDirectory != null && group.rootDirectory.Length > 0) ? group.rootDirectory : Application.dataPath, "");
                    needRefresh = true;
                }
                needRefresh |= DrawingUtils.DrawNarrowButton("Refresh Sub Models");
            }
            EditorGUILayout.EndHorizontal();
            
            if (group.rootDirectory == null || group.rootDirectory.Length == 0 || !Directory.Exists(group.rootDirectory))
                return;

            if (group.rootDirectory.IndexOf(Application.dataPath) < 0)
            {
                EditorGUILayout.HelpBox("A directory should be in this project's Asset directory!", MessageType.Error);
                return;
            }

            if (needRefresh)
                group.RefreshModels();
        }

        private void DrawModelToggles()
        {
            if (group.modelPairs.Count == 0)
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);
            {
                foreach (StaticModelPair pair in group.modelPairs)
                {
                    if (pair.Model != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginChangeCheck();
                            pair.Checked = EditorGUILayout.Toggle(pair.Model.name, pair.Checked);
                            bool changed = EditorGUI.EndChangeCheck();
                            bool buttonClicked = DrawingUtils.DrawNarrowButton("Show", 60);
                            if ((changed && pair.Checked) || buttonClicked)
                                group.SetDisplayModel(pair.Model);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndVertical(); // HelpBox

            if (group.modelPairs.Count > 1)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Select all"))
                {
                    foreach (StaticModelPair pair in group.modelPairs)
                    {
                        if (pair.Model != null)
                            pair.Checked = true;
                    }
                }
                if (GUILayout.Button("Clear all"))
                {
                    foreach (StaticModelPair pair in group.modelPairs)
                    {
                        if (pair.Model != null)
                            pair.Checked = false;
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        protected override void OnModelSelected(StudioModel model)
        {
            group.RefreshModels();

            base.OnModelSelected(model);
        }
    }
}
