﻿using UnityEngine;
//using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    [DefaultExecutionOrder(10000)]
    [AddComponentMenu("Malbers/Utilities/Aiming/Aim")]
    public class Aim : MonoBehaviour, IAim, IAnimatorListener
    {
        #region Vars and Props

        #region Public Variables
        [SerializeField, Tooltip("Is the Aim Active")]
        private BoolReference m_active = new BoolReference(true);

        [SerializeField, Tooltip("Point used for the Raycasting"),ContextMenuItem("Head as AimOrigin", "HeadAimOrigin")]
        private Transform m_aimOrigin;
        [SerializeField, Tooltip("Smoothness Lerp value to change from Active to Disable")]
        private float m_Smoothness = 10f;

        [SerializeField, Tooltip("Layers inlcuded on the Aiming Logic")]
        private LayerReference m_aimLayer = new LayerReference(-1);
        [SerializeField, Tooltip("Does the Aiming Logic ignore Colliders??")]
        private QueryTriggerInteraction m_Triggers = QueryTriggerInteraction.Ignore;

        [SerializeField, Tooltip("Forced a Target on the Aiming Logic. Calculate the Aim from the Aim Origin to a Target")]
        private TransformReference m_AimTarget = new TransformReference();

        [Tooltip("Transform Helper that stores the position of the Hit")]
        public TransformReference m_AimPositon = new TransformReference();

        [SerializeField, Tooltip("Set a Transform Hierarchy to Ignore on the Aim Ray")]
        private TransformReference m_Ignore = new TransformReference();

        [SerializeField, Tooltip("Camera Reference used for calculatin the Aim logic from the Camera Center.")]
        private TransformReference m_camera  =new TransformReference();
        private Camera cam;

        [SerializeField, Tooltip("Default screen center")]
        private Vector2Reference m_screenCenter = new Vector2Reference(0.5f, 0.5f);

        [SerializeField, Flag("Aim Side")] private AimSide m_AimSide = 0;

        [Tooltip("Update mode for the Aim Logic")]
        public UpdateType updateMode = UpdateType.LateUpdate;


        /// <summary>Radius for the Sphere Casting, if this is set to Zero they I will use a Ray Casting</summary>
        [Tooltip("Radius for the Sphere Casting, if this is set to Zero they I will use a Ray Casting")]
        public FloatReference rayRadius = new FloatReference(0.0f);
        /// <summary>Maximun Length for the Ray Casting</summary>
        [Tooltip("Maximun Length for the Ray Casting")]
        public float RayLength = 50f;
        /// <summary>Ray Counts for the Ray casting</summary>
        [Tooltip("Maximum Ray Hits for the Ray casting")]
        public int RayHits = 5;

        public TransformEvent OnAimRayTarget = new TransformEvent();
        public Vector3Event OnScreenCenter = new Vector3Event();
        public IntEvent OnAimSide = new IntEvent();
        public BoolEvent OnAiming = new BoolEvent();

        public bool debug;
        private string hitName;
        private int hitcount;
        #endregion

        #region Properties

        /// <summary>Main Camera</summary>
        public Transform MainCamera { get => m_camera.Value; set => m_camera.Value = value; }

        /// <summary>Sets the Aim Origin Transform </summary>
        public Transform AimOrigin
        {
            get => m_aimOrigin;
            set
            {
                if (value)
                    m_aimOrigin = value;
                else
                    m_aimOrigin = defaultOrigin;
            }
        }

        private Transform defaultOrigin;

        /// <summary>Set a Extra Transform to Ignore it (Used in case of the Mount for the Rider)</summary>
        public Transform IgnoreTransform { get => m_Ignore.Value; set => m_Ignore.Value = value; }

        /// <summary>Direction the GameObject is Aiming</summary>
        public Vector3 AimDirection => (AimPoint - AimOrigin.position).normalized;

        /// <summary>is the Current AimTarget a Target Assist?</summary>
        public bool IsTargetAssist { get; private set; }


        /// <summary>Smooth Aim Point the ray is Aiming</summary>
        public Vector3 AimPoint { get; private set; }

        /// <summary>RAW Aim Point the ray is Aiming</summary>
        public Vector3 RawPoint { get; private set; }

        public float HorizontalAngle { get; private set; }
        public float VerticalAngle { get; private set; }



        /// <summary>Default Screen Center</summary>
        public Vector3 ScreenCenter { get; private set; }

        public IAimTarget LastAimTarget;

        /// <summary>Is the Aiming Logic Active?</summary>
        public bool Active
        {
            get => m_active;
            set
            {
                if (m_active.Value != value)
                {
                    m_active.Value = value;

                    if (!m_active.Value)
                    {
                        ExitAim();
                    }
                    else
                    {
                        var aimingSide = CalculateCameraTargetSide();
                        if (AimingSide == aimingSide) this.aimingSide ^= true; //Make it different
                        AimingSide = aimingSide;
                        GetAimDirection(0, true);
                        CalculateAngles();
                        enabled = true;
                        OnAiming.Invoke(true);
                        if (AimPositon) AimPositon.gameObject.SetActive(true); //Hide the Helper
                    }
                }
            }
        }


        public void ExitAim()
        {
            m_active.Value = false;
            OnScreenCenter.Invoke(ScreenCenter);
            OnAimRayTarget.Invoke(null);
            OnAimSide.Invoke(0);
            enabled = false;
            OnAiming.Invoke(false);
            if (AimPositon) AimPositon.gameObject.SetActive(false); //Hide the Helper
        }

        /// <summary>Limit the Aiming via Angle limit Which means the Aiming is Active but should not be used</summary>
        public bool Limited { get; set; }

        //private readonly float delay =1.5f;
        //private float CurrentTimedelay;

        private bool aimingSide;
        /// <summary>Check if the camera is in the right:true or Left: False side of the Character </summary>
        public bool AimingSide
        {
            get => aimingSide;
            private set
            {
               // if (MTools.ElapsedTime(CurrentTimedelay, delay))
                {
                    if (value != aimingSide) //If the values are Different
                    {
                        if (AimSide == (AimSide)(~0))           // Meaning is set to Aimm in Both Sides
                            OnAimSide.Invoke(value ? 1 : -1);
                        else if ((AimSide & AimSide.Left) == AimSide.Left)
                            OnAimSide.Invoke(-1);
                        else if ((AimSide & AimSide.Right) == AimSide.Right)
                            OnAimSide.Invoke(1);

                       // CurrentTimedelay = Time.time;
                    }
                    aimingSide = value;
                }
            }
        }

        /// <summary> Last Raycast stored for calculating the Aim</summary>
        private RaycastHit aimHit;

        /// <summary> Last Raycast stored for calculating the Aim</summary>
        public RaycastHit AimHit => aimHit;

        private Transform m_AimTargetAssist;

        /// <summary>Target Transform Stored from the AimRay</summary>
        public Transform AimRayTargetAssist
        {
            get => m_AimTargetAssist;
            set
            {
                if (m_AimTargetAssist != value)
                {
                    m_AimTargetAssist = value;
                    OnAimRayTarget.Invoke(value);
                }
            }
        }

        /// <summary>Forced Target on the Aiming Logic</summary>
        public Transform AimTarget { get => m_AimTarget.Value; set => m_AimTarget.Value = value; }
        public Transform AimPositon { get => m_AimPositon.Value; set => m_AimPositon.Value = value; }

        /// <summary>Layer to Aim and Hit</summary>
        public LayerMask Layer { get => m_aimLayer.Value; set => m_aimLayer.Value = value; }

        public QueryTriggerInteraction TriggerInteraction { get => m_Triggers; set => m_Triggers = value; }

        public AimSide AimSide { get => m_AimSide; set => m_AimSide = value; }

        #endregion
        #endregion

        public int EditorTab1 = 1;


        void Awake()
        {
            if (MainCamera == null)
            {
                cam = MTools.FindMainCamera();
                MainCamera = cam.transform;
            }
            else
            {
                cam = MainCamera.GetComponent<Camera>();
            }
            
            if (AimOrigin)
                defaultOrigin = AimOrigin;
            else
                AimOrigin = defaultOrigin = transform;


            GetCenterScreen();
        }


        void Start()
        {
            var aimingSide = CalculateCameraTargetSide();
            if (AimingSide == aimingSide) this.aimingSide ^= true; //Make it different
            AimingSide = aimingSide;
            GetAimDirection(0, true);
            CalculateAngles();
          //  OnAiming.Invoke(true);

            if (AimPositon) AimPositon.gameObject.SetActive(false);
        }



        private void FixedUpdate()
        {
            if (Active && updateMode == UpdateType.FixedUpdate)
                AimLogic(Time.fixedDeltaTime);
        }


        private void LateUpdate()
        {
            if (Active && updateMode == UpdateType.LateUpdate)
                AimLogic(Time.deltaTime);
        }

        public virtual void AimLogic(float time = 0)
        {
            GetAimDirection(time);
            AimingSide = CalculateCameraTargetSide();
            CalculateAngles();
        }

        public void Active_Set(bool value) => Active = value;

        public void Active_Toggle() => Active ^= true;

        public void SetTarget(Transform target) { AimTarget = target; }

        /// <summary>Calculates the Camera/Target Horizontal Angle Normalized </summary>
        public void CalculateAngles()
        {
            var Dir = (AimPoint - AimOrigin.position);
            Vector3 HorizontalDir = Vector3.ProjectOnPlane(Dir, Vector3.up).normalized;

            HorizontalAngle = Vector3.Angle(HorizontalDir, transform.root.forward) * (AimingSide ? 1 : -1); //Get the Normalized value for the look direction
            VerticalAngle = (Vector3.Angle(transform.up, Dir) - 90); //Get the Normalized value for the look direction
        }

        private bool CalculateCameraTargetSide()
        {
            float aiminginSide = 0;

            if (MainCamera)
            {
                aiminginSide = Vector3.Dot((AimOrigin.position - AimPoint).normalized, transform.right);
                if (AimTarget)
                    return IsLeft(MainCamera.position, AimTarget.position, transform.position);
            }
            else if (AimTarget)
            {
                aiminginSide = Vector3.Dot((AimOrigin.position - AimPoint).normalized, transform.right);
            }
         
            return aiminginSide > 0;                                                                    //Get the Camera Side Left/Right
        }

        public static bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
        {
            return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
        }

        /// <summary>Calcultate the Direction</summary>
        private void GetAimDirection(float time, bool Raw = false)
        {
            if (AimTarget)
            {
                DirectionFromTarget(out aimHit);
                RawPoint = aimHit.point;
            }
            else if (MainCamera)
            {
                DirectionFromCamera(out aimHit);
                RawPoint = aimHit.point;
            }

            float t = time * m_Smoothness;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);


            var isRaw = Raw || m_Smoothness == 0 || time == 0;

            AimPoint = isRaw ? RawPoint : Vector3.Lerp(AimPoint, RawPoint, t);

            if (AimPositon != null)
            {
                AimPositon.position = AimPoint;
                AimPositon.up = isRaw ? aimHit.normal : Vector3.Lerp(AimPositon.up, aimHit.normal, t);
            } 
        }

        private void GetCenterScreen()
        {
            var SC = new Vector3(Screen.width * m_screenCenter.Value.x, Screen.height * m_screenCenter.Value.y); //Gets the Center of the Aim Dot Transform

            if (SC != ScreenCenter)
            {
                ScreenCenter = SC;
                OnScreenCenter.Invoke(ScreenCenter);
            }
        }

        public void DirectionFromCamera( out RaycastHit hit)
        {
            GetCenterScreen();

            Ray ray = cam.ScreenPointToRay(ScreenCenter);

            hit = new RaycastHit()
            {
                distance = float.MaxValue,
                point = ray.GetPoint(RayLength)
            };

            var Hits = new RaycastHit[RayHits];

            if (rayRadius > 0)
                hitcount = Physics.SphereCastNonAlloc(ray, rayRadius, Hits, RayLength, Layer, m_Triggers);
            else
                hitcount = Physics.RaycastNonAlloc(ray, Hits, RayLength, Layer, m_Triggers);

            if (hitcount > 0)
            {
                foreach (RaycastHit rHit in Hits)
                {
                    if (rHit.transform == null) break;                                          //Means nothing was found

                    if (rHit.transform.root == IgnoreTransform) continue;                      //Dont Hit anything the Ignore
                    if (rHit.transform.root == AimOrigin.root) continue;                       //Dont Hit anything in this hierarchy

                    if (Vector3.Distance(MainCamera.position, rHit.point) < Vector3.Distance(MainCamera.position, AimOrigin.position)) continue; //If I hit something behind me skip

                    if (hit.distance > rHit.distance) hit = rHit;
                }
            }

            hit = GetAimAssist(hit);
        }
        public void DirectionFromTarget (out RaycastHit hit)
        {
            Ray ray = new Ray(AimOrigin.position, (AimTarget.position - AimOrigin.position).normalized);

            hit = new RaycastHit()
            {
                distance = float.MaxValue,
                point = ray.GetPoint(RayLength)
            };

            var Hits = new RaycastHit[RayHits];

            if (rayRadius > 0)
                hitcount = Physics.SphereCastNonAlloc(ray, rayRadius, Hits, RayLength, Layer, m_Triggers);
            else
                hitcount = Physics.RaycastNonAlloc(ray, Hits, RayLength, Layer, m_Triggers);

            if (hitcount > 0)
            {
                foreach (RaycastHit rHit in Hits)
                {
                    if (rHit.transform == null) break;                                          //Means nothing was found

                    if (rHit.transform.root == IgnoreTransform) continue;                      //Dont Hit anything the Ignore
                    if (rHit.transform.root == AimOrigin.root) continue;                       //Dont Hit anything in this hierarchy

                    if (Vector3.Distance(MainCamera.position, rHit.point) < Vector3.Distance(MainCamera.position, AimOrigin.position)) continue; //If I hit something behind me skip

                    if (hit.distance > rHit.distance) hit = rHit;
                }
            }

            hit = GetAimAssist(hit);
        }

        private RaycastHit GetAimAssist(RaycastHit hit)
        {
            hitName = hit.collider ? hit.collider.name : string.Empty;
            IAimTarget IAimTarg = hit.collider ? hit.collider.GetComponent<IAimTarget>() : null;
            IsTargetAssist = false;

            if (IAimTarg != null)
            {
                if (IAimTarg.AimAssist)
                {
                    hit.point = hit.collider.bounds.center;
                    IsTargetAssist = true;
                    AimRayTargetAssist = hit.collider.transform;
                }


                if (IAimTarg != LastAimTarget)
                {
                    LastAimTarget = IAimTarg;
                    LastAimTarget.IsBeenAimed(true);
                }
            }
            else
            {
                if (LastAimTarget != null)
                {
                    LastAimTarget.IsBeenAimed(false);
                    LastAimTarget = null;
                }

                AimRayTargetAssist = null;
            }

            return hit;
        }


        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);


        // [ContextMenu("Origin[Head]")]
        private void HeadAimOrigin()
        {
            var anim = GetComponent<Animator>();
            if (anim)
            {
                var head = anim.GetBoneTransform(HumanBodyBones.Head);

                if (head) AimOrigin = head;
            }
        }

#if UNITY_EDITOR
        void Reset()
        {
            if (MainCamera == null)
            {
                cam = MTools.FindMainCamera();
                MainCamera = cam.transform;
            }
            else
            {
                cam = MainCamera.GetComponent<Camera>();
            }


            MEvent FollowUITransform = MTools.GetInstance<MEvent>("Follow UI Transform");

            AimOrigin = defaultOrigin = transform;

            if (FollowUITransform != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnAimRayTarget, FollowUITransform.Invoke);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnScreenCenter, FollowUITransform.Invoke);
            }
        }

        void OnDrawGizmos()
        {
            if (debug && Application.isPlaying)
            {
                if (Active && !Limited && AimOrigin)
                {
                    //float radius = RayRadius > 0.01f ? RayRadius.Value : 0.05f;
                    float radius = 0.05f;
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(AimPoint, radius);
                    Gizmos.DrawSphere(AimPoint, radius);
                    Gizmos.DrawRay(AimOrigin.position, AimDirection);
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(AimOrigin.position, AimPoint);

                    GUIStyle style = new GUIStyle(UnityEditor.EditorStyles.label)
                    {
                        fontStyle = FontStyle.Bold,
                        fontSize = 10
                    };

                    style.normal.textColor = Color.green;

                    UnityEditor.Handles.Label(AimPoint, hitName, style);
                }
            }
        }

       
#endif
    }
    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(Aim))]
    public class AimEditor : Editor
    {
        Aim m;

        SerializedProperty m_active, m_aimOrigin, m_Smoothness, m_aimLayer, m_Triggers, m_AimTarget, m_AimPositon, m_AimSide, debug, m_UpdateMode,  OnAiming,
            m_Ignore, m_camera, m_screenCenter, rayRadius, RayHits, OnAimRayTarget, OnScreenCenter, OnAimSide, EditorTab1, RayLength;
        private void OnEnable()
        {
            m = (Aim)target;

            m_active = serializedObject.FindProperty("m_active");
            m_aimOrigin = serializedObject.FindProperty("m_aimOrigin");
            m_Smoothness = serializedObject.FindProperty("m_Smoothness");
            m_aimLayer = serializedObject.FindProperty("m_aimLayer");
            m_Triggers = serializedObject.FindProperty("m_Triggers");
            m_AimTarget = serializedObject.FindProperty("m_AimTarget");
            m_AimPositon = serializedObject.FindProperty("m_AimPositon");
            m_Ignore = serializedObject.FindProperty("m_Ignore");
            m_camera = serializedObject.FindProperty("m_camera");
            m_screenCenter = serializedObject.FindProperty("m_screenCenter");
            m_AimSide = serializedObject.FindProperty("m_AimSide");
            rayRadius = serializedObject.FindProperty("rayRadius");
            RayLength = serializedObject.FindProperty("RayLength");
            RayHits = serializedObject.FindProperty("RayHits");
            EditorTab1 = serializedObject.FindProperty("EditorTab1");
            debug = serializedObject.FindProperty("debug");
            m_UpdateMode = serializedObject.FindProperty("updateMode");

            OnAimRayTarget = serializedObject.FindProperty("OnAimRayTarget");
            OnScreenCenter = serializedObject.FindProperty("OnScreenCenter");
            OnAimSide = serializedObject.FindProperty("OnAimSide");
            OnAiming = serializedObject.FindProperty("OnAiming");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Aim Logic, Cast a Ray from the Aim Origin to the Camera Forward axis or a Target");

            EditorTab1.intValue = GUILayout.Toolbar(EditorTab1.intValue, new string[] { "General", "References", "Events" });

            //First Tabs
            int Selection = EditorTab1.intValue;

            if (Selection == 0) ShowGeneral();
            else if (Selection == 1) ShowReferences();
            else if (Selection == 2) ShowEvents();


            serializedObject.ApplyModifiedProperties();
        }

      

        private void ShowGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_active);
            if (EditorGUI.EndChangeCheck())
            {
                m.enabled = m.Active;
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.PropertyField(m_UpdateMode);
            EditorGUILayout.PropertyField(m_AimSide);
            EditorGUILayout.PropertyField(m_aimOrigin);
            if (m_aimOrigin.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Please Select an Aim Origin Reference", MessageType.Error);
            EditorGUILayout.EndVertical();
         


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Layer Interaction", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_aimLayer, new GUIContent("Layer"));
            EditorGUILayout.PropertyField(m_Triggers);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Ray Casting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rayRadius);
            EditorGUILayout.PropertyField(RayLength);
            EditorGUILayout.PropertyField(RayHits);
            EditorGUILayout.PropertyField(m_Smoothness);
            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(debug);
        }

        private void ShowReferences()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_camera);
            EditorGUILayout.PropertyField(m_screenCenter);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_AimTarget);
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Extras", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Ignore);
            EditorGUILayout.PropertyField(m_AimPositon);
            EditorGUILayout.EndVertical();
        }

        private void ShowEvents()
        {
            EditorGUILayout.PropertyField(OnAiming);
            EditorGUILayout.PropertyField(OnAimRayTarget);
            EditorGUILayout.PropertyField(OnScreenCenter);
            EditorGUILayout.PropertyField(OnAimSide);
        }
    }
#endif
    #endregion
}