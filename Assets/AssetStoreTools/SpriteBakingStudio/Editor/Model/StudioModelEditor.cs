using UnityEngine;
using UnityEditor;

namespace SBS
{
    public class StudioModelEditor : Editor
    {
        protected void DrawCustomizerField(StudioModel model)
        {
            model.customizer = (FrameUpdater)EditorGUILayout.ObjectField("Customizer", model.customizer, typeof(FrameUpdater), true);
        }

        protected void DrawPivotOffsetFields(StudioModel model)
        {
            model.pivotOffset = EditorGUILayout.Vector3Field("Pivot Offset", model.pivotOffset);
            if (model.pivotOffset.sqrMagnitude > 0)
            {
                EditorGUI.indentLevel++;
                model.directionDependentPivot = EditorGUILayout.Toggle("Direction Dependent", model.directionDependentPivot);
                EditorGUI.indentLevel--;
            }
        }

		protected void DrawModelSettingButton(StudioModel newModel)
        {
            SpriteBakingStudio studio = FindObjectOfType<SpriteBakingStudio>();
            if (studio == null)
                return;

            StudioModel oldModel = studio.setting.model.obj;
            if (DrawingUtils.DrawWideButton("Set as the Model"))
            {
                if (oldModel != null && oldModel != newModel)
                    oldModel.gameObject.SetActive(false);

                OnModelSelected(newModel);
                studio.setting.model.obj = newModel;

                studio.samplings.Clear();
                studio.selectedFrames.Clear();

                TransformUtils.UpdateCamera(studio.setting);
            }
        }

        protected virtual void OnModelSelected(StudioModel model)
        {
            Transform transform = model.transform;
            while (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(true);
                transform = transform.parent;
            }

            model.gameObject.SetActive(true);
        }
    }
}
