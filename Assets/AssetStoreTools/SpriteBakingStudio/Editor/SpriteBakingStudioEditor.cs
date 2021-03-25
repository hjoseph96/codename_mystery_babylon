using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace SBS
{
    [CustomEditor(typeof(SpriteBakingStudio))]
    public class StudioBakingStudioEditor : Editor
    {
        private SpriteBakingStudio studio = null;
        private StudioSetting setting = null;

        private bool variationExcludingShadowBackup = false;
        private bool shadowWithoutModel_ = false;

        private int currModelUnitDegree = 0;
        private int currModelDegree = 0;

        private Texture2D previewTexture = null;

        private ModelBaker modelBaker = null;

        private bool IsCapturable()
        {
            if (!studio.isSamplingReady)
                return false;
            if (FrameSampler.GetInstance().IsSamplingNow())
                return false;
            if (modelBaker != null && modelBaker.IsBakingNow())
                return false;
            return true;
        }

        void OnEnable()
        {
            studio = (SpriteBakingStudio)target;

            if (studio.setting == null)
                studio.setting = new StudioSetting();
            setting = studio.setting;

            studio.folding = EditorPrefs.GetBool(Global.FOLDING_KEY, false);

            if (setting.IsStaticModelGroup())
                setting.GetStaticModelGroup().RefreshModels();
        }

        void OnDisable()
        {
            if (modelBaker != null)
                EditorApplication.update -= modelBaker.UpdateState;

            FrameSampler sampler = FrameSampler.GetInstance();
            if (sampler != null)
                EditorApplication.update -= sampler.UpdateState;
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            if (studio == null)
                return;

            Undo.RecordObject(studio, "Sprite Baking Studio");

            studio.isSamplingReady = true;
            studio.isBakingReady = true;

#if UNITY_WEBPLAYER
            EditorGUILayout.HelpBox("Don't set 'Build Setting > Platform' to WebPlayer!", MessageType.Error);
            studio.isSamplingReady = false;
#endif

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck(); // check any changes
            {
                bool modelChanged = DrawModelFields();
                if (modelChanged)
                {
                    studio.samplings.Clear();
                    studio.selectedFrames.Clear();
                }

                if (setting.model.obj == null)
                {
                    EditorGUILayout.HelpBox("No model!", MessageType.Error);
                    studio.isSamplingReady = false;
                    setting.model.opened = true;
                }
                if (setting.model.obj != null && !setting.model.obj.IsReady())
                {
                    EditorGUILayout.HelpBox("Target model not ready!", MessageType.Error);
                    studio.isSamplingReady = false;
                    setting.model.opened = true;
                }

                EditorGUILayout.Space();

                DrawCameraFields();

                if (Camera.main == null)
                {
                    EditorGUILayout.HelpBox("No main camera!", MessageType.Error);
                    studio.isSamplingReady = false;
                    setting.camera.opened = true;
                }

                EditorGUILayout.Space();

                DrawLightFields();

                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                DrawViewFields();
                bool viewChanged = EditorGUI.EndChangeCheck();

                if (setting.view.checkedViews.Count == 0)
                    setting.view.opened = true;

                if (setting.view.CountCheckedViews() == 0)
                {
                    EditorGUILayout.HelpBox("No selected view!", MessageType.Error);
                    studio.isSamplingReady = false;
                    studio.isBakingReady = false;
                    setting.view.opened = true;
                }

                EditorGUILayout.Space();

                bool modelViewChanged = modelChanged || viewChanged;
                DrawShadowFields(modelViewChanged);

                EditorGUILayout.Space();

                DrawExtractFields();

                EditorGUILayout.Space();

                DrawVariationFields();

                EditorGUILayout.Space();

                DrawPreviewFields();

                EditorGUILayout.Space();

                DrawFrameFields();

                if (setting.frame.resolution.x < 1f || setting.frame.resolution.y < 1f)
                {
                    EditorGUILayout.HelpBox("Too small Resolution!", MessageType.Error);
                    studio.isSamplingReady = false;
                    setting.frame.opened = true;
                }

                DrawSamplingButton();

                EditorGUILayout.Space();
            }
            bool anyChanged = EditorGUI.EndChangeCheck();

            if (setting.preview.on)
            {
                if (IsCapturable() && (anyChanged || previewTexture == null))
                    UpdatePreviewTexture();
            }

            //---------------------------------- Output -----------------------------------

            DrawTrimFields();

            EditorGUILayout.Space();

            DrawOutputFields();

            EditorGUILayout.Space();

            DrawPathFields();

            if (setting.path.fileName == null || setting.path.fileName.Length == 0)
            {
                EditorGUILayout.HelpBox("No file name!", MessageType.Error);
                studio.isBakingReady = false;
                setting.path.opened = true;
            }

            if (setting.path.directoryPath == null || setting.path.directoryPath.Length == 0 || !Directory.Exists(setting.path.directoryPath))
            {
                EditorGUILayout.HelpBox("Invalid directory!", MessageType.Error);
                studio.isBakingReady = false;
                setting.path.opened = true;
            }
            else
            {
                if (setting.path.directoryPath.IndexOf(Application.dataPath) < 0)
                {
                    EditorGUILayout.HelpBox(string.Format("{0} is out of the Assets folder.", setting.path.directoryPath), MessageType.Error);
                    studio.isBakingReady = false;
                    setting.path.opened = true;
                }
            }

            EditorGUILayout.Space();

            DrawBakingButton();
        }

        private bool DrawModelFields()
        {
            if (!DrawGroupOrPass("Model & Animation", ref setting.model.opened))
                return false;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            EditorGUI.BeginChangeCheck();

            StudioModel prevModel = setting.model.obj;
            setting.model.obj = (StudioModel)EditorGUILayout.ObjectField("Model", setting.model.obj, typeof(StudioModel), true);
            if (setting.model.obj == null)
                setting.model.obj = FindObjectOfType<StudioModel>();

            if (setting.model.obj != null)
            {
                if (setting.model.obj != prevModel)
                {
                    if (prevModel != null)
                        prevModel.gameObject.SetActive(false);
                    setting.model.obj.gameObject.SetActive(true);

                    TransformUtils.UpdateCamera(setting);
                }

                if (setting.IsAnimatedModel())
                {
                    StudioAnimatedModel animatedModel = setting.GetAnimatedModel();
                    animatedModel.animClip = (AnimationClip)EditorGUILayout.ObjectField("Animation", animatedModel.animClip, typeof(AnimationClip), true);
                }

                if (setting.IsStaticModelGroup())
                {
                    StaticModelGroup group = setting.GetStaticModelGroup();
                    if (DrawingUtils.DrawMiddleButton("Refresh Sub Models"))
                        group.RefreshModels();
                    group.InitModelsPosition();
                }
                setting.model.obj.transform.position = Vector3.zero;
            }

            bool modelChanged = EditorGUI.EndChangeCheck();

            GUILayout.EndVertical(); // HelpBox

            return modelChanged;
        }

        private void DrawCameraFields()
        {
            if (!DrawGroupOrPass("Camera", ref setting.camera.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);
            
            if (Camera.main != null)
            {
                if (Camera.main.orthographic)
                {
                    EditorGUILayout.BeginHorizontal();
                    Camera.main.orthographicSize = EditorGUILayout.FloatField("Camera Size", Camera.main.orthographicSize);

                    if (setting.model.obj != null && !setting.IsParticleModel())
                    {
                        if (DrawingUtils.DrawNarrowButton("Adjust Camera"))
                        {
                            TransformUtils.UpdateCamera(setting);
                            TransformUtils.AdjustCamera(setting);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Orthographic projection would be better.", MessageType.Warning);
                    if (DrawingUtils.DrawMiddleButton("Change to Orthographic"))
                        Camera.main.orthographic = true;
                }
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawLightFields()
        {
            if (!DrawGroupOrPass("Light", ref setting.light.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.light.obj = (Light)EditorGUILayout.ObjectField("Main Light", setting.light.obj, typeof(Light), true);
            if (setting.light.obj == null)
            {
                GameObject lightObj = null;
#if UNITY_2018_1_OR_NEWER
                lightObj = GameObject.Find("Directional Light");
#else
                lightObj = GameObject.Find("Directional light");
#endif
                if (lightObj != null)
                    setting.light.obj = lightObj.GetComponent<Light>();
            }

            if (setting.light.obj != null)
            {
                EditorGUI.BeginChangeCheck();
                setting.light.followCamera = EditorGUILayout.Toggle("Follow Camera", setting.light.followCamera);
                if (EditorGUI.EndChangeCheck())
                {
                    if (setting.light.followCamera && Camera.main != null)
                    {
                        setting.light.obj.transform.position = Camera.main.transform.position;
                        setting.light.obj.transform.rotation = Camera.main.transform.rotation;
                    }
                }

                if (!setting.light.followCamera)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    setting.light.obj.transform.position = EditorGUILayout.Vector3Field("Position", setting.light.obj.transform.position);
                    if (EditorGUI.EndChangeCheck() || DrawingUtils.DrawMiddleButton("Look At Model"))
                        TransformUtils.LookAtModel(setting.light.obj.transform, setting.model.obj);
                    EditorGUI.indentLevel--;
                }
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawViewFields()
        {
            if (!DrawGroupOrPass("View", ref setting.view.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            EditorGUI.BeginChangeCheck();
            setting.view.slopeAngle = EditorGUILayout.FloatField("Slope Angle (0~90)", setting.view.slopeAngle);
            setting.view.slopeAngle = Mathf.Clamp(setting.view.slopeAngle, 0f, 90f);
            bool slopeAngleChanged = EditorGUI.EndChangeCheck();

            if (setting.IsSideView() && slopeAngleChanged)
                setting.view.showTile = false;
            else
                DrawTileFields(ref slopeAngleChanged);

            if (slopeAngleChanged)
                TransformUtils.UpdateCamera(setting);

            EditorGUI.BeginChangeCheck();
            setting.view.size = EditorGUILayout.IntField("View Size", setting.view.size);
            bool viewSizeChanged = EditorGUI.EndChangeCheck();

            if (setting.view.size < 1)
                setting.view.size = 1;

            float unitDegree = 360f / setting.view.size;

            if (viewSizeChanged || setting.view.size != setting.view.toggleAndNames.Length)
            {
                ToggleAndName[] oldToggleAndNames = (ToggleAndName[])setting.view.toggleAndNames.Clone();
                setting.view.toggleAndNames = new ToggleAndName[setting.view.size];
                for (int i = 0; i < setting.view.toggleAndNames.Length; ++i)
                    setting.view.toggleAndNames[i] = new ToggleAndName(false);
                MigrateViews(oldToggleAndNames);

                int integerUnitDegree = (int)unitDegree;
                if (currModelUnitDegree % integerUnitDegree != 0)
                    currModelDegree = 0;
                currModelUnitDegree = integerUnitDegree;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            setting.view.initialDegree = EditorGUILayout.FloatField(string.Format("Initial Degree (0~{0})", unitDegree), setting.view.initialDegree);
            bool initialDegreeChanged = EditorGUI.EndChangeCheck();
            setting.view.initialDegree = Mathf.Clamp(setting.view.initialDegree, 0, unitDegree);

            List<CheckedView> checkedViews = new List<CheckedView>();

            for (int i = 0; i < setting.view.size; i++)
            {
                float degree = unitDegree * i;
                ModelRotationCallback callback = () =>
                {
#if UNITY_2018_1_OR_NEWER
                    if (setting.IsSkinnedModel())
                    {
                        StudioSkinnedModel animatedModel = setting.model.obj as StudioSkinnedModel;
                        animatedModel.currDegree = setting.view.initialDegree + degree;
                    }
#endif
                    TransformUtils.RotateModel(setting.model.obj, setting.view.initialDegree + degree);
                };

                int intDegree = Mathf.RoundToInt(setting.view.initialDegree + degree);

                if (DrawEachView(string.Format("{0}", intDegree) + "º", callback, setting.view.toggleAndNames[i]))
                {
                    currModelDegree = intDegree;
                    callback();
                }

                if (setting.view.toggleAndNames[i].toggle)
                {
                    string viewName = (setting.view.toggleAndNames[i].name.Length > 0) ? setting.view.toggleAndNames[i].name : intDegree + "deg";
                    checkedViews.Add(new CheckedView(intDegree, viewName, callback));
                }
            }

            setting.view.checkedViews = checkedViews;

            EditorGUI.indentLevel--;

            if (setting.view.size > 1)
                DrawViewSelectionButtons(setting.view.toggleAndNames);

            if (viewSizeChanged || initialDegreeChanged)
                TransformUtils.RotateModel(setting.model.obj, setting.view.initialDegree + currModelDegree);

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawTileFields(ref bool slopeAngleChanged)
        {
            EditorGUI.BeginChangeCheck();
            setting.view.showTile = EditorGUILayout.Toggle("Show Tile", setting.view.showTile);
            bool tileShowingChanged = EditorGUI.EndChangeCheck();

            bool aspectRatioChanged = false;

            if (setting.view.showTile)
            {
                if (setting.IsSideView())
                {
                    if (tileShowingChanged)
                    {
                        setting.view.slopeAngle = 30;
                        slopeAngleChanged = true;
                    }
                }
                else
                {
                    EditorGUI.indentLevel++;

                    setting.view.tileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", setting.view.tileType);

                    EditorGUI.BeginChangeCheck();
                    setting.view.tileAspectRatio = EditorGUILayout.Vector2Field("Aspect Ratio", setting.view.tileAspectRatio);
                    aspectRatioChanged = EditorGUI.EndChangeCheck();

                    if (setting.view.tileAspectRatio.x < 1f)
                        setting.view.tileAspectRatio.x = 1f;
                    if (setting.view.tileAspectRatio.y < 1f)
                        setting.view.tileAspectRatio.y = 1f;
                    if (setting.view.tileAspectRatio.x < setting.view.tileAspectRatio.y)
                        setting.view.tileAspectRatio.x = setting.view.tileAspectRatio.y;

                    EditorGUI.indentLevel--;
                }

                if (tileShowingChanged || slopeAngleChanged)
                {
                    setting.view.tileAspectRatio.x = setting.view.tileAspectRatio.y / Mathf.Sin(setting.view.slopeAngle * Mathf.Deg2Rad);
                }
                else if (aspectRatioChanged)
                {
                    setting.view.slopeAngle = Mathf.Asin(setting.view.tileAspectRatio.y / setting.view.tileAspectRatio.x) * Mathf.Rad2Deg;
                    slopeAngleChanged = true;
                }
            }
            
            if (IsCapturable())
            {
                if (setting.view.showTile)
                    CreateObject(Global.TILES_OBJECT_NAME, new Vector3(0f, -0.1f, 0f));
                else
                    DeleteObject(Global.TILES_OBJECT_NAME);

                TileUtils.UpdateTile(setting);
            }
        }

        private void MigrateViews(ToggleAndName[] oldToggleAndNames)
        {
            if (oldToggleAndNames.Length < setting.view.toggleAndNames.Length)
            {
                for (int oldIndex = 0; oldIndex < oldToggleAndNames.Length; ++oldIndex)
                {
                    float ratio = (float)oldIndex / oldToggleAndNames.Length;
                    int newIndex = Mathf.FloorToInt(setting.view.toggleAndNames.Length * ratio);
                    setting.view.toggleAndNames[newIndex].name = oldToggleAndNames[oldIndex].name;

                    if (oldToggleAndNames[oldIndex].toggle)
                        setting.view.toggleAndNames[newIndex].toggle = true;
                }
            }
            else if (oldToggleAndNames.Length > setting.view.toggleAndNames.Length)
            {
                for (int newIndex = 0; newIndex < setting.view.toggleAndNames.Length; ++newIndex)
                {
                    float ratio = (float)newIndex / setting.view.toggleAndNames.Length;
                    int oldIndex = Mathf.FloorToInt(oldToggleAndNames.Length * ratio);
                    setting.view.toggleAndNames[newIndex].name = oldToggleAndNames[oldIndex].name;

                    if (oldToggleAndNames[oldIndex].toggle)
                        setting.view.toggleAndNames[newIndex].toggle = true;
                }
            }
        }

        private void DrawViewSelectionButtons(ToggleAndName[] toggleAndNames)
        {
            EditorGUILayout.BeginHorizontal();
            if (DrawingUtils.DrawNarrowButton("Select all"))
            {
                for (int i = 0; i < toggleAndNames.Length; i++)
                    toggleAndNames[i].toggle = true;
            }
            if (DrawingUtils.DrawNarrowButton("Clear all"))
            {
                for (int i = 0; i < toggleAndNames.Length; i++)
                    toggleAndNames[i].toggle = false;
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool DrawEachView(string label, ModelRotationCallback callback, ToggleAndName toggleAndName)
        {
            bool applied = false;
            Rect rect = EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                toggleAndName.toggle = EditorGUILayout.Toggle(label, toggleAndName.toggle);
                bool toggleChanged = EditorGUI.EndChangeCheck();

                Rect textFieldRect = new Rect(rect.x + 50, rect.y, rect.width * 0.2f, Global.NARROW_BUTTON_HEIGHT);
                if (!toggleAndName.toggle) GUI.enabled = false;
                toggleAndName.name = EditorGUI.TextField(textFieldRect, toggleAndName.name);
                if (!toggleAndName.toggle) GUI.enabled = true;

                bool applyButtonClicked = DrawingUtils.DrawNarrowButton("Apply", 60);
                if (applyButtonClicked || (toggleChanged && toggleAndName.toggle))
                    applied = true;
            }
            EditorGUILayout.EndHorizontal();

            return applied;
        }

        private void DrawShadowFields(bool modelViewChanged)
        {
            if (!DrawGroupOrPass("Shadow", ref setting.shadow.opened))
                return;

            if (setting.model.obj == null)
                return;

            if (Camera.main.transform.position.y <= 0f || Camera.main.transform.forward.y >= 0f)
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            EditorGUI.BeginChangeCheck();
            setting.shadow.type = (ShadowType)EditorGUILayout.EnumPopup("Shadow Type", setting.shadow.type);
            bool shadowTypeChanged = EditorGUI.EndChangeCheck();

            if (shadowTypeChanged)
            {
                if (setting.shadow.type != ShadowType.None)
                {
                    setting.shadow.shadowOnly = shadowWithoutModel_;
                    if (setting.variation.excludeShadow)
                    {
                        setting.shadow.shadowOnly = false;
                        shadowWithoutModel_ = false;
                    }
                }
                else
                {
                    shadowWithoutModel_ = setting.shadow.shadowOnly;
                    setting.shadow.shadowOnly = false;
                }
            }

            if (setting.shadow.type == ShadowType.Simple)
            {
                EditorGUI.indentLevel++;

                DeleteObjectUnder("Shadow", setting.model.obj.transform); // old shadow object
                DeleteObject(Global.STATIC_SHADOW_NAME);
                DeleteObject(Global.DYNAMIC_SHADOW_NAME);

                EditorGUI.BeginChangeCheck();

                if (setting.model.obj.simpleShadow.gameObject != null)
                {
                    if (setting.model.obj.transform != setting.model.obj.simpleShadow.gameObject.transform.parent)
                        setting.model.obj.simpleShadow.gameObject = null;
                }
                if (setting.model.obj.simpleShadow.gameObject == null)
                    setting.model.obj.simpleShadow.gameObject = CreateObject(Global.SIMPLE_SHADOW_NAME, Vector3.zero, setting.model.obj.transform);
                AttachDontApplyUniformShader(setting.model.obj.simpleShadow.gameObject);

                EditorGUILayout.BeginHorizontal();
                {
                    Vector3 prevScale = setting.model.obj.simpleShadow.scale;
                    setting.model.obj.simpleShadow.scale = EditorGUILayout.Vector2Field("Scale", setting.model.obj.simpleShadow.scale);

                    if (setting.model.obj.simpleShadow.autoScale) GUI.enabled = false;
                    if (DrawingUtils.DrawNarrowButton("Unify", 50))
                    {
                        if (setting.model.obj.simpleShadow.scale.y != prevScale.y)
                            setting.model.obj.simpleShadow.scale.x = setting.model.obj.simpleShadow.scale.y;
                        else
                            setting.model.obj.simpleShadow.scale.y = setting.model.obj.simpleShadow.scale.x;
                    }
                    if (setting.model.obj.simpleShadow.autoScale) GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();

                if (setting.model.obj is StudioSkinnedModel)
                {
                    EditorGUI.BeginChangeCheck();
                    setting.model.obj.simpleShadow.autoScale = EditorGUILayout.Toggle("Auto Scale", setting.model.obj.simpleShadow.autoScale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Vector3 ratio = setting.model.obj.GetRatioBetweenSizes();
                        Vector2 scale = setting.model.obj.simpleShadow.scale;
                        if (setting.model.obj.simpleShadow.autoScale)
                            setting.model.obj.simpleShadow.scale = new Vector2(scale.x * ratio.x, scale.y * ratio.z);
                        else
                            setting.model.obj.simpleShadow.scale = new Vector2(scale.x / ratio.x, scale.y / ratio.z);
                    }
                }

                DrawShadowOpacityField(setting.model.obj.simpleShadow.gameObject);

                DrawShadowOnlyField();

                bool anyChanged = EditorGUI.EndChangeCheck();

                if (modelViewChanged || shadowTypeChanged || anyChanged)
                    setting.model.obj.RescaleSimpleShadow();

                EditorGUI.indentLevel--;
            }
            else if (setting.shadow.type == ShadowType.Real)
            {
                EditorGUI.indentLevel++;
                setting.shadow.method = (RealShadowMethod)EditorGUILayout.EnumPopup("Method", setting.shadow.method);
                EditorGUI.indentLevel--;

                if (setting.shadow.method == RealShadowMethod.Dynamic)
                {
                    if (setting.IsParticleModel())
                    {
                        EditorGUILayout.HelpBox("Dynamic method is not supported for ParticleSystem.", MessageType.Info);
                        GUILayout.EndVertical(); // HelpBox
                        studio.isSamplingReady = false;
                        return;
                    }

                    EditorGUI.indentLevel++;

                    DeleteObjectUnder(Global.SIMPLE_SHADOW_NAME, setting.model.obj.transform);
                    DeleteObject(Global.STATIC_SHADOW_NAME);

                    SetupShadowCameraAndFields(Global.DYNAMIC_SHADOW_NAME);

                    //DrawRealShadowThingsField();

                    DrawShadowOpacityField(setting.shadow.fieldObj);

                    DrawShadowOnlyField();

                    EditorGUI.indentLevel--;
                }
                else if (setting.shadow.method == RealShadowMethod.Static)
                {
                    EditorGUI.indentLevel++;

                    DeleteObjectUnder(Global.SIMPLE_SHADOW_NAME, setting.model.obj.transform);
                    DeleteObject(Global.DYNAMIC_SHADOW_NAME);

                    SetupShadowCameraAndFields(Global.STATIC_SHADOW_NAME);

                    if (setting.shadow.fieldObj != null)
                        setting.shadow.fieldObj.SetActive(setting.shadow.staticShadowVisible);

                    //DrawRealShadowThingsField();

                    DrawShadowOpacityField(setting.shadow.fieldObj);

                    DrawShadowOnlyField();

                    if (!setting.preview.on)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (DrawingUtils.DrawNarrowButton("Update"))
                        {
                            setting.shadow.staticShadowVisible = true;
                            StudioUtility.BakeStaticShadow(setting);
                        }
                        if (DrawingUtils.DrawNarrowButton("Hide"))
                        {
                            setting.shadow.staticShadowVisible = false;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                DeleteObjectUnder(Global.SIMPLE_SHADOW_NAME, setting.model.obj.transform);
                DeleteObject(Global.STATIC_SHADOW_NAME);
                DeleteObject(Global.DYNAMIC_SHADOW_NAME);
                setting.shadow.shadowOnly = false;
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void SetupShadowCameraAndFields(string shadowName)
        {
            if (setting.shadow.camera == null || setting.shadow.fieldObj == null)
            {
                GameObject shadowObj = CreateObject(shadowName, Vector3.zero);
                if (shadowObj != null)
                {
                    Transform cameraTransform = shadowObj.transform.Find("Camera");
                    if (cameraTransform != null)
                    {
                        setting.shadow.camera = cameraTransform.gameObject.GetComponent<Camera>();
                        setting.shadow.camera.orthographicSize = Camera.main.orthographicSize;
                    }
                    
                    Transform fieldTransform = shadowObj.transform.Find("Field");
                    if (fieldTransform != null)
                        setting.shadow.fieldObj = fieldTransform.gameObject;

                    StudioUtility.UpdateShadowFieldSize(setting.shadow.camera, setting.shadow.fieldObj);

                    if (cameraTransform != null)
                    {
                        cameraTransform.position = setting.shadow.cameraPosition;
                        TransformUtils.LookAtModel(setting.shadow.camera.transform, setting.model.obj);
                    }

                    if (fieldTransform != null)
                    {
                        fieldTransform.position = setting.shadow.fieldPosition;
                        fieldTransform.rotation = Quaternion.Euler(0f, setting.shadow.fieldRotation, 0f);
                    }   
                }
            }
        }

        private void DrawRealShadowThingsField()
        {
            if (setting.shadow.camera == null || setting.shadow.fieldObj == null)
                return;

            EditorGUI.indentLevel++;

            GUIStyle style = new GUIStyle("label");
            style.fontSize = 10;
            style.fontStyle = FontStyle.Italic;
            EditorGUILayout.LabelField("experimental", style);

            EditorGUI.BeginChangeCheck();
            setting.shadow.cameraPosition = EditorGUILayout.Vector3Field("Camera Position", setting.shadow.cameraPosition);
            bool cameraPositionChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            setting.shadow.autoAdjustField = EditorGUILayout.Toggle("Auto Adjust Field", setting.shadow.autoAdjustField);
            bool autoAdjustingChanged = EditorGUI.EndChangeCheck();

            if ((cameraPositionChanged && setting.shadow.autoAdjustField) || (autoAdjustingChanged && setting.shadow.autoAdjustField))
            {
                setting.shadow.camera.transform.position = setting.shadow.cameraPosition;
                TransformUtils.LookAtModel(setting.shadow.camera.transform, setting.model.obj);

                Vector3 dirToModel = setting.model.obj.ComputedCenter - setting.shadow.cameraPosition;

                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = new Ray(setting.shadow.cameraPosition, dirToModel);
                float distance = 0;
                if (plane.Raycast(ray, out distance))
                    setting.shadow.fieldObj.transform.position = setting.shadow.fieldPosition = ray.GetPoint(distance) / 2;

                dirToModel.y = 0;
                setting.shadow.fieldRotation = Vector3.Angle(dirToModel, Vector3.forward);
                if (setting.shadow.cameraPosition.x > 0)
                    setting.shadow.fieldRotation *= -1;
                setting.shadow.fieldObj.transform.rotation = Quaternion.Euler(0f, setting.shadow.fieldRotation, 0f);
            }

            if (Mathf.Abs(setting.shadow.cameraPosition.x) > Mathf.Epsilon || Mathf.Abs(setting.shadow.cameraPosition.z) > Mathf.Epsilon)
            {
                EditorGUI.BeginChangeCheck();
                setting.shadow.fieldPosition = EditorGUILayout.Vector3Field("Field Position", setting.shadow.fieldPosition);
                if (EditorGUI.EndChangeCheck())
                    setting.shadow.fieldObj.transform.position = setting.shadow.fieldPosition;

                EditorGUI.BeginChangeCheck();
                setting.shadow.fieldRotation = EditorGUILayout.FloatField("Field Rotation", setting.shadow.fieldRotation);
                if (EditorGUI.EndChangeCheck())
                    setting.shadow.fieldObj.transform.rotation = Quaternion.Euler(0f, setting.shadow.fieldRotation, 0f);

                EditorGUI.BeginChangeCheck();
                setting.shadow.fieldScale = EditorGUILayout.Vector3Field("Field Scale", setting.shadow.fieldScale);
                if (EditorGUI.EndChangeCheck())
                    setting.shadow.fieldObj.transform.localScale = setting.shadow.fieldScale;
            }

            EditorGUI.indentLevel--;
        }

        private void DrawShadowOpacityField(GameObject shadowObj)
        {
            if (shadowObj == null)
                return;

            Renderer renderer = shadowObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.sharedMaterial.color;
                float opacity = EditorGUILayout.Slider("Opacity", color.a, 0, 1);
                color.a = Mathf.Clamp01(opacity);
                renderer.sharedMaterial.color = color;
            }
        }

        private void DrawShadowOnlyField()
        {
            if (setting.variation.on && setting.variation.excludeShadow) GUI.enabled = false;
            setting.shadow.shadowOnly = EditorGUILayout.Toggle("Shadow Only", setting.shadow.shadowOnly);
            if (setting.variation.on && setting.variation.excludeShadow) GUI.enabled = true;
        }

        private void DrawExtractFields()
        {
            if (!DrawGroupOrPass("Extract", ref setting.extract.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.extract.obj = (ExtractorBase)EditorGUILayout.ObjectField("Extractor", setting.extract.obj, typeof(ExtractorBase), true);
            if (setting.extract.obj == null)
            {
                string[] assetGuids = AssetDatabase.FindAssets(Global.DEFAULT_EXTRACTOR_NAME);
                foreach (string guid in assetGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prf = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prf != null)
                    {
                        setting.extract.obj = prf.GetComponent<DefaultExtractor>();
                        break;
                    }
                }
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawVariationFields()
        {
            if (!DrawGroupOrPass("Variation", ref setting.variation.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            EditorGUI.BeginChangeCheck();
            setting.variation.on = EditorGUILayout.Toggle("Use Variation", setting.variation.on);
            bool variationUsingChanged = EditorGUI.EndChangeCheck();

            if (variationUsingChanged)
            {
                if (setting.variation.on)
                {
                    setting.variation.excludeShadow = variationExcludingShadowBackup;
                    if (setting.shadow.shadowOnly)
                    {
                        setting.variation.excludeShadow = false;
                        variationExcludingShadowBackup = false;
                    }
                }
                else
                {
                    variationExcludingShadowBackup = setting.variation.excludeShadow;
                    setting.variation.excludeShadow = false;
                }
            }

            if (setting.variation.on)
            {
                EditorGUI.indentLevel++;

                setting.variation.tintColor = EditorGUILayout.ColorField("Tint Color", setting.variation.tintColor);
                setting.variation.tintBlendFactor = (BlendFactor)EditorGUILayout.EnumPopup("Tint Blend Factor", setting.variation.tintBlendFactor);
                setting.variation.imageBlendFactor = (BlendFactor)EditorGUILayout.EnumPopup("Image Blend Factor", setting.variation.imageBlendFactor);

                if (setting.shadow.type != ShadowType.None)
                {
                    if (setting.shadow.shadowOnly) GUI.enabled = false;
                    {
                        setting.variation.excludeShadow = EditorGUILayout.Toggle("Exclude Shadow", setting.variation.excludeShadow);

                        if (setting.variation.excludeShadow)
                        {
                            EditorGUI.indentLevel++;
                            setting.variation.bodyBlendFactor = (BlendFactor)EditorGUILayout.EnumPopup("Body Blend Factor", setting.variation.bodyBlendFactor);
                            setting.variation.shadowBlendFactor = (BlendFactor)EditorGUILayout.EnumPopup("Shadow Blend Factor", setting.variation.shadowBlendFactor);
                            EditorGUI.indentLevel--;
                        }
                    }
                    if (setting.shadow.shadowOnly) GUI.enabled = true;
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawPreviewFields()
        {
            if (!DrawGroupOrPass("Preview", ref setting.preview.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            EditorGUI.BeginChangeCheck();
            setting.preview.on = EditorGUILayout.Toggle("Show Preview", setting.preview.on);
            bool previewChanged = EditorGUI.EndChangeCheck();

            if (setting.preview.on)
            {
                EditorGUI.indentLevel++;

                setting.preview.backgroundType = (PreviewBackgroundType)EditorGUILayout.EnumPopup("Background", setting.preview.backgroundType);
                if (setting.preview.backgroundType == PreviewBackgroundType.SingleColor)
                {
                    EditorGUI.indentLevel++;
                    setting.preview.backgroundColor = EditorGUILayout.ColorField("Color", setting.preview.backgroundColor);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                if (setting.preview.on)
                {
                    if (previewChanged || DrawingUtils.DrawMiddleButton("Update Preview"))
                        UpdatePreviewTexture();
                }

                if (setting.IsStaticRealShadow())
                    EditorGUILayout.HelpBox("When the real shadow method is Static, it slows down overall.", MessageType.Warning);
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawFrameFields()
        {
            if (!DrawGroupOrPass("Frame", ref setting.frame.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.frame.resolution = EditorGUILayout.Vector2Field("Resolution", setting.frame.resolution);
            setting.frame.resolution = new Vector2
            (
                Mathf.Round(setting.frame.resolution.x),
                Mathf.Round(setting.frame.resolution.y)
            );

            if (!setting.IsSingleStaticModel())
            {
                if (setting.IsAnimatedModel() || setting.IsParticleModel())
                {
                    EditorGUI.BeginChangeCheck();
                    setting.frame.frameSize = EditorGUILayout.IntField("Frame Size", setting.frame.frameSize);
                    if (setting.frame.frameSize < 1)
                        setting.frame.frameSize = 1;
                    if (EditorGUI.EndChangeCheck())
                    {
                        studio.samplings.Clear();
                        studio.selectedFrames.Clear();
                    }
                }

                if (setting.IsAnimatedModel() && IsCapturable())
                {
                    bool simulatedFrameChanged = false;
                    if (setting.frame.frameSize > 1)
                    {
                        EditorGUI.BeginChangeCheck();
                        string label = string.Format("Simulate (0~{0})", setting.frame.frameSize - 1);
                        setting.frame.simulatedFrame = EditorGUILayout.IntSlider(label, setting.frame.simulatedFrame, 0, setting.frame.frameSize - 1);
                        simulatedFrameChanged = EditorGUI.EndChangeCheck();
                    }
                    else
                    {
                        setting.frame.simulatedFrame = 0;
                    }

                    if (simulatedFrameChanged)
                    {
                        float frameRatio = 0.0f;
                        if (setting.frame.simulatedFrame > 0 && setting.frame.simulatedFrame < setting.frame.frameSize)
                            frameRatio = (float)setting.frame.simulatedFrame / (float)(setting.frame.frameSize - 1);

                        float frameTime = setting.model.obj.GetTimeForRatio(frameRatio);
                        setting.model.obj.UpdateModel(new Frame(setting.frame.simulatedFrame, frameTime));
                    }
                }

                setting.frame.delay = EditorGUILayout.DoubleField("Delay", setting.frame.delay);
                if (setting.frame.delay < 0.0)
                    setting.frame.delay = 0.0;
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawSamplingButton()
        {
            if (!IsCapturable())
                return;

            if (DrawingUtils.DrawWideButton("Sample"))
            {
                HideSelectorAndViewer();
                FrameSampler sampler = FrameSampler.GetInstance();
                sampler.OnEnd = ShowSelectorAndPreviewer;
                sampler.SampleFrames(studio);
            }

            if (studio.samplings.Count > 0)
            {
                string buttonText;
                if (studio.selectedFrames.Count == 0)
                    buttonText = "Select Frames!";
                else
                    buttonText = studio.selectedFrames.Count + " frame(s) selected.";

                if (DrawingUtils.DrawWideButton(buttonText))
                    ShowSelectorAndPreviewer();
            }
        }

        public void ShowSelectorAndPreviewer()
        {
            if (FrameSelector.instance != null || FramePreviewer.instance != null)
                return;

            FrameSelector selector = ScriptableWizard.DisplayWizard<FrameSelector>("Frame Selector");
            if (selector != null)
                selector.SetStudio(studio);

            FramePreviewer previewer = ScriptableWizard.DisplayWizard<FramePreviewer>("Frame Previewer");
            if (previewer != null)
                previewer.SetStudio(studio);
        }

        public void HideSelectorAndViewer()
        {
            if (FrameSelector.instance != null)
                FrameSelector.instance.Close();

            if (FramePreviewer.instance != null)
                FramePreviewer.instance.Close();
        }

        private void DrawTrimFields()
        {
            if (!DrawGroupOrPass("Trim", ref setting.trim.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.trim.on = EditorGUILayout.Toggle("Use Trim", setting.trim.on);
            if (setting.trim.on)
            {
                EditorGUI.indentLevel++;

                setting.trim.spriteMargin = EditorGUILayout.IntField("Sprite Margin", setting.trim.spriteMargin);

                setting.trim.useUnifiedSize = EditorGUILayout.Toggle("Unified Size", setting.trim.useUnifiedSize);
                if (setting.trim.useUnifiedSize)
                {
                    EditorGUI.indentLevel++;

                    setting.trim.pivotSymmetrically =
                        EditorGUILayout.Toggle("Pivot-Symmetric",
                        (!setting.IsSingleStaticModel() && setting.trim.allUnified) ? false : setting.trim.pivotSymmetrically);

                    if (!setting.IsSingleStaticModel())
                    {
                        setting.trim.allUnified =
                            EditorGUILayout.Toggle(setting.IsStaticModelGroup() ? "for All Models" : "for All Views",
                            setting.trim.pivotSymmetrically ? false : setting.trim.allUnified);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawOutputFields()
        {
            if (!DrawGroupOrPass("Output", ref setting.output.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.output.type = (OutputType)EditorGUILayout.EnumPopup("Output Type", setting.output.type);

            if (setting.output.type == OutputType.SpriteSheet)
            {
                EditorGUI.indentLevel++;

                setting.output.algorithm = (PackingAlgorithm)EditorGUILayout.EnumPopup("Packing Algorithm", setting.output.algorithm);

                EditorGUI.indentLevel++;
                if (setting.output.algorithm == PackingAlgorithm.Optimized)
                    setting.output.atlasSizeIndex = EditorGUILayout.Popup("Max Size", setting.output.atlasSizeIndex, studio.atlasSizes);
                else if (setting.output.algorithm == PackingAlgorithm.InOrder)
                    setting.output.atlasSizeIndex = EditorGUILayout.Popup("Min Size", setting.output.atlasSizeIndex, studio.atlasSizes);
                EditorGUI.indentLevel--;

                setting.output.spritePadding = EditorGUILayout.IntField("Padding", setting.output.spritePadding);
                if (setting.output.spritePadding < 0)
                    setting.output.spritePadding = 0;

                if (setting.IsStaticModel())
                    setting.output.allInOneAtlas = EditorGUILayout.Toggle("All In One Atlas", setting.output.allInOneAtlas);
                else if (setting.IsAnimatedModel())
                    setting.output.loopAnimationClip = EditorGUILayout.Toggle("Loop Animation Clip", setting.output.loopAnimationClip);

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void DrawPathFields()
        {
            if (!DrawGroupOrPass("Path", ref setting.path.opened))
                return;

            GUILayout.BeginVertical(Global.HELP_BOX_STYLE);

            setting.path.autoFileNaming = EditorGUILayout.Toggle("Auto File Naming", setting.path.autoFileNaming);
            if (setting.path.autoFileNaming)
                AutoMakeFileName();

            EditorGUI.BeginChangeCheck();
            if (setting.path.autoFileNaming) GUI.enabled = false;
            setting.path.fileName = EditorGUILayout.TextField("File Name", setting.path.fileName);
            if (setting.path.autoFileNaming) GUI.enabled = true;
            bool fileNameChanged = EditorGUI.EndChangeCheck();

            if (fileNameChanged && setting.path.fileName != null && setting.path.fileName.Length > 0)
            {
                char[] invalidPathChars = Path.GetInvalidPathChars();
                string validFileName = "";

                for (int i = 0; i < setting.path.fileName.Length; ++i)
                {
                    bool invalid = false;
                    foreach (char invalidChar in invalidPathChars)
                    {
                        if (setting.path.fileName[i] == invalidChar)
                        {
                            invalid = true;
                            break;
                        }
                    }
                    validFileName += invalid ? '_' : setting.path.fileName[i];
                }

                setting.path.fileName = validFileName;
            }

            setting.path.directoryPath = EditorGUILayout.TextField("Output Directory", setting.path.directoryPath);

            if (DrawingUtils.DrawMiddleButton("Choose Directory"))
            {
                setting.path.directoryPath = EditorUtility.SaveFolderPanel("Choose a directory", Application.dataPath, "spritesheets");
                GUIUtility.ExitGUI();
            }

            GUILayout.EndVertical(); // HelpBox
        }

        private void AutoMakeFileName()
        {
            if (setting.model.obj == null)
                return;

            setting.path.fileName = setting.model.obj.name;

            if (setting.IsAnimatedModel())
            {
                StudioAnimatedModel animatedModel = setting.model.obj as StudioAnimatedModel;
                if (animatedModel.animClip != null)
                    setting.path.fileName += "_" + animatedModel.animClip.name;
            }
        }

        private void DrawBakingButton()
        {
            if (!IsCapturable() || !studio.isBakingReady)
                return;

            if ((studio.samplings.Count > 0 && studio.selectedFrames.Count > 0) || studio.samplings.Count == 0)
            {
                string postText = studio.selectedFrames.Count > 0 ? "selected frames" : "all frames";

                if (DrawingUtils.DrawWideButton("Bake " + postText))
                {
                    HideSelectorAndViewer();
                    if (setting.IsStaticModelGroup())
                        modelBaker = new StaticModelGroupBaker();
                    else
                        modelBaker = new AnimatedModelBaker();
                    modelBaker.Bake(studio);
                }
            }
        }

        private bool DrawGroupOrPass(string name, ref bool opened)
        {
            if (!studio.folding)
                return true;

            Rect rect = EditorGUILayout.BeginVertical();
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0,
                EditorGUIUtility.isProSkin ? new Color32(90, 90, 90, 255) : new Color32(170, 170, 170, 255));
            tex.Apply();
            GUI.DrawTexture(rect, tex);
            opened = EditorGUILayout.Foldout(opened, name);
            EditorGUILayout.EndVertical();

            return opened;
        }

        private GameObject CreateObject(string name, Vector3 position, Transform parent = null)
        {
            GameObject obj = GameObject.Find(name);

            if (obj == null)
            {
                GameObject prefab = null;
                
                string[] assetGuids = AssetDatabase.FindAssets(name);
                foreach (string guid in assetGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prf = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (prf != null)
                    {
                        prefab = prf;
                        break;
                    }
                }

                if (prefab != null)
                {
                    obj = Instantiate(prefab, position, Quaternion.identity);
                    obj.name = name;

                    if (parent != null)
                    {
                        obj.transform.parent = parent;
                        obj.transform.localRotation = Quaternion.identity;
                    }
                }
            }

            return obj;
        }

        private void DeleteObject(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
                DestroyImmediate(obj);
        }

        private void DeleteObjectUnder(string name, Transform parent)
        {
            Transform child = parent.Find(name);
            if (child != null && child.gameObject != null)
                DestroyImmediate(child.gameObject);
        }

        private void AttachDontApplyUniformShader(GameObject obj)
        {
            Debug.Assert(obj.GetComponentsInChildren<Renderer>().Length == 1);

            Renderer renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                if (renderer.gameObject.GetComponent<DontApplyUniformShader>() == null)
                    renderer.gameObject.AddComponent<DontApplyUniformShader>();
            }
        }

        private void UpdatePreviewTexture()
        {
            Camera.main.targetTexture = new RenderTexture((int)setting.frame.resolution.x, (int)setting.frame.resolution.y, 24, RenderTextureFormat.ARGB32);
            CameraClearFlags tmpCamClearFlags = Camera.main.clearFlags;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Color tmpCamBgColor = Camera.main.backgroundColor;

            TileUtils.HideAllTiles();

            previewTexture = StudioUtility.PrepareShadowAndExtractTexture(setting);

            TileUtils.UpdateTile(setting);

            Camera.main.targetTexture = null;
            Camera.main.clearFlags = tmpCamClearFlags;
            Camera.main.backgroundColor = tmpCamBgColor;
        }

        public override bool HasPreviewGUI()
        {
            return setting != null ? setting.preview.on : false;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Preview");
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (rect.width <= 1 || rect.height <= 1)
                return;

            if (previewTexture == null)
                return;

            Rect scaledRect = ScalePreviewRect(rect);
            Texture2D scaledTex = TextureUtils.ScaleTexture(previewTexture, (int)scaledRect.width, (int)scaledRect.height);
            scaledTex.Apply();

            if (setting.preview.backgroundType == PreviewBackgroundType.Checker)
            {
                EditorGUI.DrawTextureTransparent(scaledRect, scaledTex);
            }
            else if (setting.preview.backgroundType == PreviewBackgroundType.SingleColor)
            {
                EditorGUI.DrawRect(scaledRect, setting.preview.backgroundColor);
                GUI.DrawTexture(scaledRect, scaledTex);
            }
        }

        private Rect ScalePreviewRect(Rect rect)
        {
            float aspectRatio = (float)previewTexture.width / (float)previewTexture.height;

            float widthScaleRatio = previewTexture.width / rect.width;
            float heightScaleRatio = previewTexture.height / rect.height;

            float scaledWidth = rect.width, scaledHeight = rect.height;

            if (previewTexture.width > rect.width && previewTexture.height > rect.height)
            {
                if (widthScaleRatio < heightScaleRatio)
                    ScaleByHeight(rect.height, aspectRatio, out scaledWidth, out scaledHeight);
                else
                    ScaleByWidth(rect.width, aspectRatio, out scaledWidth, out scaledHeight);
            }
            else if (previewTexture.width > rect.width && previewTexture.height < rect.height)
            {
                ScaleByHeight(rect.height, aspectRatio, out scaledWidth, out scaledHeight);
                ScaleMoreByWidthIfOver(rect.width, ref scaledWidth, ref scaledHeight);
            }
            else if (previewTexture.width < rect.width && previewTexture.height > rect.height)
            {
                ScaleByWidth(rect.width, aspectRatio, out scaledWidth, out scaledHeight);
                ScaleMoreByHeightIfOver(rect.height, ref scaledWidth, ref scaledHeight);
            }
            else
            {
                if (widthScaleRatio < heightScaleRatio)
                {
                    ScaleByHeight(rect.height, aspectRatio, out scaledWidth, out scaledHeight);
                    ScaleMoreByWidthIfOver(rect.width, ref scaledWidth, ref scaledHeight);
                }
                else
                {
                    ScaleByWidth(rect.width, aspectRatio, out scaledWidth, out scaledHeight);
                    ScaleMoreByHeightIfOver(rect.height, ref scaledWidth, ref scaledHeight);
                }
            }

            float scaledX = rect.x + (rect.width - scaledWidth) / 2;
            float scaledY = rect.y + (rect.height - scaledHeight) / 2;

            return new Rect(scaledX, scaledY, scaledWidth, scaledHeight);
        }

        private void ScaleByHeight(float height, float aspectRatio, out float outWidth, out float outHeight)
        {
            outWidth = height * aspectRatio;
            outHeight = height;
        }

        private void ScaleByWidth(float width, float aspectRatio, out float outWidth, out float outHeight)
        {
            outWidth = width;
            outHeight = width / aspectRatio;
        }

        private void ScaleMoreByWidthIfOver(float width, ref float scaledWidth, ref float scaledHeight)
        {
            if (scaledWidth > width)
            {
                float scale = scaledWidth / width;
                scaledWidth /= scale;
                scaledHeight /= scale;
            }
        }

        private void ScaleMoreByHeightIfOver(float height, ref float scaledWidth, ref float scaledHeight)
        {
            if (scaledHeight > height)
            {
                float scale = scaledHeight / height;
                scaledWidth /= scale;
                scaledHeight /= scale;
            }
        }
    }
}
