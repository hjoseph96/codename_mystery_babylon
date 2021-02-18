using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace SS.TwoD
{
    public class AnimationGenerator
    {
        public static void GenerateSpriteAnimation(float animLength, float frameRate, bool loop, Sprite[] body, string bodyPath, Sprite[] shadow, string shadowPath, string animPath, string animName, Sprite[] color = null, string colorPath = null)
        {
#if UNITY_EDITOR
			// Create animation clip
			AnimationClip clip = new AnimationClip();
            clip.legacy = false;
            clip.frameRate = frameRate;

            // Add curves
            AddSpritesToClip(clip, animLength, body, bodyPath);
            AddSpritesToClip(clip, animLength, shadow, shadowPath);
            AddSpritesToClip(clip, animLength, color, colorPath);

            // Other settings
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Save file
            AssetDatabase.CreateAsset(clip, System.IO.Path.Combine(animPath, animName));
            AssetDatabase.SaveAssets();
#endif
        }

#if UNITY_EDITOR
        static void AddSpritesToClip(AnimationClip clip, float animLength, Sprite[] sprites, string rendererObjectRelativePath)
        {
            if (sprites != null && sprites.Length > 0 && sprites[0] != null)
            {
                // Create binding
                EditorCurveBinding binding = new EditorCurveBinding();
                binding.path = rendererObjectRelativePath;
                binding.type = typeof(SpriteRenderer);
                binding.propertyName = "m_Sprite";

                // Create key frames
                int count = sprites.Length;
                float step = (animLength > 0) ? animLength / count : 1;

                ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[count];

                for (int i = 0; i < count; i++)
                {
                    keyFrames[i] = new ObjectReferenceKeyframe();
                    keyFrames[i].time = i * step;
                    keyFrames[i].value = sprites[i];
                }

                // Set curve
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyFrames);
            }
        }
#endif
    }
}