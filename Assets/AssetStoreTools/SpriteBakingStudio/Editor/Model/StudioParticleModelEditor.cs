using UnityEngine;
using UnityEditor;

namespace SBS
{
    [CustomEditor(typeof(StudioParticleModel))]
    public class StudioParticleModelEditor : StudioModelEditor
    {
        private StudioParticleModel model = null;

        private int checkingFrameSize = 10;
        private int currCheckingFrame;

        void OnEnable()
        {
            model = (StudioParticleModel)target;
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            if (model == null)
                return;

            Undo.RecordObject(model, "Studio Particle Model");

            EditorGUI.BeginChangeCheck();
            model.mainParticleSystem = (ParticleSystem)EditorGUILayout.ObjectField("Main Particle System", model.mainParticleSystem, typeof(ParticleSystem), true);
            bool mainParticleChanged = EditorGUI.EndChangeCheck();

            ParticleSystem[] particleSystems = model.GetComponentsInChildren<ParticleSystem>();

            if (model.mainParticleSystem == null)
            {
                if (particleSystems.Length > 0)
                    model.mainParticleSystem = particleSystems[0];
            }

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps.main.prewarm)
                    EditorGUILayout.HelpBox("Turn off \'Prewarm\' of " + ps.gameObject.name, MessageType.Warning);
            }

            if (mainParticleChanged || model.mainParticleSystem == null)
                model.targetChecked = false;

            if (model.mainParticleSystem != null && !model.targetChecked && !model.targetChecking)
            {
                model.targetChecking = true;
                currCheckingFrame = 0;
                EditorApplication.update += OnEditorUpdate;
            }

            DrawCustomizerField(model);

            EditorGUILayout.Space();

			DrawPivotOffsetFields(model);

            EditorGUILayout.Space();

			DrawModelSettingButton(model);
        }

        void OnEditorUpdate()
        {
            if (!model.targetChecking)
                return;

            float frameRatio = (float)currCheckingFrame / (float)checkingFrameSize;
            EditorUtility.DisplayProgressBar("Check the model...", "Check the model...", frameRatio);

            float frameTime = model.GetTimeForRatio(frameRatio);
            model.UpdateModel(new Frame(currCheckingFrame, frameTime));

            ParticleSystemRenderer[] psRndrs = model.mainParticleSystem.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer rndr in psRndrs)
            {
                if (model.maxSize.magnitude < rndr.bounds.size.magnitude)
                {
                    Vector3 targetSize = rndr.bounds.size;
                    model.maxSize = new Vector3(targetSize.x, targetSize.y, targetSize.z);
                }

                model.minPos.x = Mathf.Min(model.minPos.x, rndr.bounds.min.x);
                model.minPos.y = Mathf.Min(model.minPos.y, rndr.bounds.min.y);
                model.minPos.z = Mathf.Min(model.minPos.z, rndr.bounds.min.z);
                model.maxPos.x = Mathf.Max(model.maxPos.x, rndr.bounds.max.x);
                model.maxPos.y = Mathf.Max(model.maxPos.y, rndr.bounds.max.y);
                model.maxPos.z = Mathf.Max(model.maxPos.z, rndr.bounds.max.z);
            }

            currCheckingFrame++;

            if (currCheckingFrame > checkingFrameSize)
            {
                model.targetChecking = false;
                model.targetChecked = true;
                model.originType = StudioModel.OriginType.Center;
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= OnEditorUpdate;
            }
        }
    }
}
