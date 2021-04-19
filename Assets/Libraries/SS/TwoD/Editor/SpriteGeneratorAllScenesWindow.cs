using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SS.TwoD
{
	public class SpriteGeneratorAllScenesWindow : EditorWindow
	{
		public string scenePath = "Scenes/Gen";
		public string[] sceneArray = { "" };

		[MenuItem("SS/TwoD/Auto play all scenes/Setup Window")]
		public static void ShowWindow()
		{
			SpriteGeneratorAllScenesWindow win = (SpriteGeneratorAllScenesWindow)(EditorWindow.GetWindow(typeof(SpriteGeneratorAllScenesWindow)));
			win.minSize = new Vector2(500, 350);

			win.scenePath = EditorPrefs.GetString("ss2d_scenePath_auto", "Scenes/Gen");
			win.sceneArray = EditorPrefs.GetString("ss2d_sceneArray_auto", string.Empty).Split(',');
		}

		void OnGUI()
		{
			GUILayout.Label("Auto Sprites Generator", EditorStyles.boldLabel);
			scenePath = EditorGUILayout.TextField("Scene path", scenePath);

			ScriptableObject target = this;
			SerializedObject so = new SerializedObject(target);
			SerializedProperty stringsProperty = so.FindProperty("sceneArray");

			EditorGUILayout.PropertyField(stringsProperty, true);
			so.ApplyModifiedProperties();

			if (GUILayout.Button("Generate all"))
			{
				SaveEditorPrefs();
				GenerateAll();
				this.Close();
			}
		}

		void GenerateAll()
		{
			if (sceneArray.Length > 0 && !string.IsNullOrEmpty(sceneArray[0]))
			{
				EditorPrefs.SetString("ss2d_sceneArray_auto_runtime", StringTools.MergeStringArray(sceneArray, ","));
				SpriteGeneratorManager.OpenThenPlayNextScene();
			}
			else
			{
				Debug.Log("You must input scene names");
			}
		}

		void SaveEditorPrefs()
		{
			EditorPrefs.SetString("ss2d_scenePath_auto", scenePath);
			EditorPrefs.SetString("ss2d_sceneArray_auto", StringTools.MergeStringArray(sceneArray, ","));
		}
	}
}