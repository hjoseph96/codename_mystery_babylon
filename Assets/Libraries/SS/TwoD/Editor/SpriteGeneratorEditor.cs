using UnityEngine;
using UnityEditor;

namespace SS.TwoD
{
    [CustomEditor(typeof(SpriteGenerator))]
    public class SpriteGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpriteGenerator sg = (SpriteGenerator)target;

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
            sg.animationDuration = EditorGUILayout.FloatField("Animation Duration", sg.animationDuration);
            sg.animationFrameRate = EditorGUILayout.FloatField("Animation FrameRate", sg.animationFrameRate);
            sg.animationLoop = EditorGUILayout.Toggle("Animation Loop", sg.animationLoop);
            sg.animationDirections = EditorGUILayout.MaskField("Animation Directions", sg.animationDirections, directions);

			if (GUI.changed)
			{
                SS.Tools.SceneTools.MarkCurrentSceneDirty();
			}
        }
    } 
}