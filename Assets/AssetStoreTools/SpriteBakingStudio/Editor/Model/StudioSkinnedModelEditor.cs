using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SBS
{
    [CustomEditor(typeof(StudioSkinnedModel))]
    public class StudioSkinnedModelEditor : StudioAnimatedModelEditor
    {
        private StudioSkinnedModel model = null;

        void OnEnable()
        {
            model = (StudioSkinnedModel)target;

            if (model != null)
            {
                model.computedMinPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                model.computedMaxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    Bounds bounds = renderers[i].bounds;
                    model.computedMinPos.x = Mathf.Min(model.computedMinPos.x, bounds.min.x);
                    model.computedMinPos.y = Mathf.Min(model.computedMinPos.y, bounds.min.y);
                    model.computedMinPos.z = Mathf.Min(model.computedMinPos.z, bounds.min.z);
                    model.computedMaxPos.x = Mathf.Max(model.computedMaxPos.x, bounds.max.x);
                    model.computedMaxPos.y = Mathf.Max(model.computedMaxPos.y, bounds.max.y);
                    model.computedMaxPos.z = Mathf.Max(model.computedMaxPos.z, bounds.max.z);
                }

                model.computedSize = model.computedMaxPos - model.computedMinPos;
            }
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            if (model == null)
                return;

            Undo.RecordObject(model, "Studio Skinned Model");

            DrawAnimationField(model);

            DrawCustomizerField(model);

            EditorGUILayout.Space();

            model.mainMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Main Mesh Renderer", model.mainMeshRenderer, typeof(SkinnedMeshRenderer), true);
            if (model.mainMeshRenderer == null)
            {
                SkinnedMeshRenderer[] meshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (meshRenderers.Length > 0)
                {
                    Dictionary<float, SkinnedMeshRenderer> dic = new Dictionary<float, SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer rndr in meshRenderers)
                        dic.Add(rndr.bounds.size.sqrMagnitude, rndr);
                    var sortedDic = dic.OrderByDescending(obj => obj.Key);
                    KeyValuePair<float, SkinnedMeshRenderer> biggest = sortedDic.ElementAt(0);
                    model.mainMeshRenderer = biggest.Value;
                }
            }

            EditorGUILayout.Space();

            model.originType = (StudioModel.OriginType)EditorGUILayout.EnumPopup("Origin Position", model.originType);

            model.forwordType = (StudioModel.ForwardType)EditorGUILayout.EnumPopup("Forward Direction", model.forwordType);

            EditorGUILayout.Space();

            DrawPivotOffsetFields(model);

            EditorGUILayout.Space();

            model.rootMotion = EditorGUILayout.Toggle("Root Motion", model.rootMotion);
            if (model.rootMotion)
            {
                EditorGUI.indentLevel++;

                model.xzCenterBone = (Transform)EditorGUILayout.ObjectField("X-Z Center Bone", model.xzCenterBone, typeof(Transform), true);
                if (model.xzCenterBone == null)
                    EditorGUILayout.HelpBox("No center bone object!", MessageType.Warning);

                model.fixYtoBottom = EditorGUILayout.Toggle("Fix Y to Bottom", model.fixYtoBottom);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            DrawModelSettingButton(model);
        }
    }
}
