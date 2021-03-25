// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor
{
    [CustomEditor(typeof(RogueLikeMapGenerator))]
	public class RogueLikeMapGeneratorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Generate Map"))
            {
                (target as RogueLikeMapGenerator).GenerateMap();
            }
        }
    }
}
