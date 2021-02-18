using UnityEngine;
using UnityEditor;

namespace SS.TwoD
{
    [CustomEditor(typeof(SpriteParticleGenerator))]
    public class SpriteParticleGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpriteParticleGenerator sg = (SpriteParticleGenerator)target;

            SpriteGeneratorManager sgm = sg.gameObject.GetComponent<SpriteGeneratorManager>();
            int maxDirection = (int)sgm.maxDirection;
            int step = maxDirection / 4;
            string[] directions = new string[maxDirection];

            for (int i = 0; i < maxDirection; i++)
            {
                if (i == 0)
                {
                    directions[i] = i.ToString() + " (Forward)";
                }
                else if (i == step)
                {
                    directions[i] = i.ToString() + " (Right)";
                }
                else if (i == step * 2)
                {
                    directions[i] = i.ToString() + " (Back)";
                }
                else if (i == step * 3)
                {
                    directions[i] = i.ToString() + " (Left)";
                }
                else
                {
                    directions[i] = i.ToString();
                }
            }

            sg.animationName = EditorGUILayout.TextField("Animation Name", sg.animationName);
            sg.originalAnimationDuration = EditorGUILayout.FloatField("Original Duration", sg.originalAnimationDuration);
            sg.animationDuration = EditorGUILayout.FloatField("Animation Duration", sg.animationDuration);
            sg.animationFrameRate = EditorGUILayout.FloatField("Animation FrameRate", sg.animationFrameRate);
            sg.animationLoop = EditorGUILayout.Toggle("Animation Loop", sg.animationLoop);
            sg.animationDirections = EditorGUILayout.MaskField("Animation Directions", sg.animationDirections, directions);
            sg.particleSystemPrefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab", sg.particleSystemPrefab, typeof(GameObject), true);
            sg.outputMaterial = (Material)EditorGUILayout.ObjectField("Output material", sg.outputMaterial, typeof(Material), true);

			if (GUI.changed)
			{
                SS.Tools.SceneTools.MarkCurrentSceneDirty();
			}
        }
    } 
}