using UnityEngine;
using UnityEditor;

namespace SBS
{
    public class StudioAnimatedModelEditor : StudioModelEditor
    {
        protected void DrawAnimationField(StudioAnimatedModel model)
        {
            model.animClip = (AnimationClip)EditorGUILayout.ObjectField("Animation", model.animClip, typeof(AnimationClip), true);
            if (model.animClip == null)
                EditorGUILayout.HelpBox("No animation clip!", MessageType.Error);
        }
    }
}
