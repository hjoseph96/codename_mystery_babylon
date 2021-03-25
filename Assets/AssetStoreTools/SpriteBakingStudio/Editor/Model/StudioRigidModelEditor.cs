using UnityEngine;
using UnityEditor;

namespace SBS
{
    [CustomEditor(typeof(StudioRigidModel))]
    public class StudioRigidModelEditor : StudioAnimatedModelEditor
    {
        private StudioRigidModel model = null;

        void OnEnable()
        {
            model = (StudioRigidModel)target;
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            if (model == null)
                return;

            Undo.RecordObject(model, "Studio Rigid Model");

            DrawAnimationField(model);

            DrawCustomizerField(model);

            EditorGUILayout.Space();

            model.meshRndr = (MeshRenderer)EditorGUILayout.ObjectField("Mesh Renderer", model.meshRndr, typeof(MeshRenderer), true);
            if (model.meshRndr == null)
                model.meshRndr = model.GetComponentInChildren<MeshRenderer>();

            EditorGUILayout.Space();

			DrawPivotOffsetFields(model);

            EditorGUILayout.Space();

			DrawModelSettingButton(model);
        }
    }
}
