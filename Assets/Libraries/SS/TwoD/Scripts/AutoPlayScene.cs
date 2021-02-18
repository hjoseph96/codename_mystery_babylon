using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    [ExecuteInEditMode]
    public class AutoPlayScene : MonoBehaviour
    {
        public static void Play()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                new GameObject("Auto Play Scene").AddComponent<AutoPlayScene>();
            }
        }
        
        #if UNITY_EDITOR
        void Update()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = true;
                DestroyImmediate(gameObject);
            }
        }
        #endif
    }
}