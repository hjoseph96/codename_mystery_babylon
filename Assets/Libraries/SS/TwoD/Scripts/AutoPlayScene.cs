using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    [ExecuteInEditMode]
    public class AutoPlayScene : MonoBehaviour
    {
        #if UNITY_EDITOR
        public static void Play()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                new GameObject("Auto Play Scene").AddComponent<AutoPlayScene>();
            }
        }
        
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