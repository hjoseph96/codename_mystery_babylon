using System.Collections;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FormationBuilderView : EditorWindow
{
    public static GUISkin Skin;



    private static Texture _arrowTex;
    private static string formationName;
    private static Vector2 formationBounds = new Vector2(2, 2);
    private static AIFormation formation;
    private static List<int> currentlySetPositions = new List<int>();



    public static void ShowWindow(AIFormation _formation)
    {
        EditorWindow.GetWindow(typeof(FormationBuilderView));
        Clear();

        formation = _formation;
        formationName = formation.Name;
        formationBounds = new Vector2(formation.Width, formation.Height);

        LoadUI();
    }

    [MenuItem("Window/Formation Builder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FormationBuilderView));
        Clear();
        formation = new AIFormation(formationBounds, Vector2Int.zero);

        LoadUI();
    }

    void OnGUI()
    {
        GUI.skin = Skin;
        GUILayout.Label("Formation", EditorStyles.boldLabel);
        formationName = EditorGUILayout.TextField("Name", formationName);

        formationBounds = EditorGUILayout.Vector2Field("Bounds", formationBounds);

        if (formation == null)
            formation = new AIFormation(formationBounds, Vector2Int.zero, formationName);
        else if (formation.Width != (int)formationBounds.x || formation.Height != (int)formationBounds.y)
            formation.Init((int)formationBounds.x, (int)formationBounds.y);
        //else if (formation.Name != formationName)
        //    formation = new AIFormation(formationBounds, Vector2Int.zero, formationName);

        formation.Name = formationName;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        GUILayout.Label(_arrowTex, GUILayout.Height(60f), GUILayout.Width(20f));

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();


        GenerateGrid(formationBounds);

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false), GUILayout.MaxHeight(30f), GUILayout.MaxWidth(700f)))
        {
            Clear();
        }

        if (GUILayout.Button("Save", GUILayout.ExpandWidth(false), GUILayout.MaxHeight(30f), GUILayout.MaxWidth(700f)))
        {
            Save();
        }

        GUILayout.EndHorizontal();

       
    }

    private void GenerateGrid(Vector2 boundsf)
    {
        Vector2Int bounds = new Vector2Int((int)boundsf.x, (int)boundsf.y);
        for (int i = 0; i < bounds.y; i++)
        {
            for (int j = 0; j < bounds.x; j++)
            {
                if (j == 0)
                    GUILayout.BeginHorizontal();

                GUI.backgroundColor = new Color(180f / 255f, 152f / 255f, 123f / 255f);

                var gridButtonContent = formation[j, i] != -1 ? formation[j, i].ToString() : "";

                if (GUILayout.Button(gridButtonContent, GUILayout.ExpandWidth(false), GUILayout.MinWidth(5f), GUILayout.MinHeight(5f), GUILayout.MaxHeight(50f), GUILayout.MaxWidth(50f)))
                {
                    if (formation[j, i] != -1)
                    {
                        formation.UpdatePositionIndexesBeyond(formation[j, i]);
                        formation[j, i] = -1;
                    }
                    else
                    {
                        var index = formation.GetNewPositionIndex();
                        formation[j, i] = index;
                    }
                }

                if (j == bounds.x - 1)
                    GUILayout.EndHorizontal();

            }
        }
    }



    private void Save()
    {
        FormationsDB.Instance.Save(formation);
    }

    private static void LoadUI()
    {
        Skin = Resources.Load<GUISkin>("Skins/FormationBuilder");
        _arrowTex = Resources.Load<Texture>("Skins/Textures/Arrow");
    }

    private static void Clear()
    {
        formation = null;
    }
}

#endif