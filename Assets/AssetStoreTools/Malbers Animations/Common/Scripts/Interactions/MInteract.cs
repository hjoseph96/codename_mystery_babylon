using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Interaction/Interactable")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/interactable")]
    public class MInteract : UnityUtils, IInteractable
    {
        [Tooltip("ID for the Interactable")]
        public IntReference m_ID = new IntReference(0);
        [Tooltip("If greater that zero, this Interactable can only interact with Interactors that has this ID\n" +
            "By default its -1, which means that can be activated by anyone")]
        public IntReference m_InteracterID = new IntReference(-1);

        [Tooltip("If the Interactor has this Interactable focused, it will interact with it automatically.\n" +
            "It also is used by the AI Animals. If the Animal Reaches this gameobject to Interact with it this needs to be set to true")]
        [SerializeField] private BoolReference m_Auto = new BoolReference(true);

        [Tooltip("Interact Once, after that it cannot longer work, unlest the Interactable is Restarted. Disable the component")]
        [SerializeField] private BoolReference m_singleInteraction = new BoolReference(true);

        [Tooltip("Delay time to activate the events on the Interactable")]
        public FloatReference m_Delay = new FloatReference(0);

        [Tooltip("CoolDown between Interactions when the Interactable is NOT a Single/One time interaction")]
        public FloatReference m_CoolDown = new FloatReference(0); 

        [Tooltip("When an Interaction is executed these events will be invoked." +
            "\n\nOnInteractWithGO(GameObject) -> will have the *INTERACTER* gameObject as parameter" +
            "\n\nOnInteractWith(Int) -> will have the *INTERACTER* ID as parameter")]
        public InteractionEvents events = new InteractionEvents();

        public UnityEvent OnFocused = new UnityEvent();
        public UnityEvent OnUnfocused = new UnityEvent();

        public int ID => m_ID;

        public bool CanInteract { get => enabled && !InCooldown; set => enabled = value; }
        public bool SingleInteraction { get => m_singleInteraction.Value; set => m_singleInteraction.Value = value; }
        public bool Auto { get => m_Auto.Value; set => m_Auto.Value = value; }

        /// <summary>Delay time to Activate the Interaction on the Interactable</summary>
        public float Delay { get => m_Delay.Value; set => m_Delay.Value = value; }

        /// <summary>CoolDown Between Interactions</summary>
        public float Cooldown { get => m_CoolDown.Value; set => m_CoolDown.Value = value; }

        /// <summary>Is the Interactable in CoolDown?</summary>
        public bool InCooldown => !MTools.ElapsedTime(CurrentActivationTime, Cooldown);

        private bool focused;
        public bool Focused
        {
            get => focused;
            set
            {
                focused = value;
                if (focused) OnFocused.Invoke();
                else OnUnfocused.Invoke();
            }
        }

      

        public Transform Owner => transform;

        private float CurrentActivationTime;
        [SerializeField] private int Editor_Tabs1;


        private void OnEnable() => Restart();

        /// <summary> Receive an Interaction from the Interacter </summary>
        /// <param name="InteracterID">ID of the Interacter</param>
        /// <param name="interacter">Interacter's GameObject</param>
        public void Interact(int InteracterID, GameObject interacter)
        {
            if (CanInteract)
            {
                if (m_InteracterID <= 0 || m_InteracterID == InteracterID) //Check for Interactor ID
                {
                    CurrentActivationTime = Time.time;

                    StartCoroutine(DelayedEvents(InteracterID, interacter));

                    if (SingleInteraction)
                    {
                        Focused = false;
                        CanInteract = false;
                    }
                }
            }
        }


        private IEnumerator DelayedEvents(int InteracterID, GameObject interacter)
        {
            if (Delay > 0) yield return new WaitForSeconds(Delay);
            events.OnInteractWithGO.Invoke(interacter);
            events.OnInteractWith.Invoke(InteracterID);
            yield return null;
        }


        /// <summary>  Receive an Interaction from an gameObject </summary>
        /// <param name="InteracterID">ID of the Interacter</param>
        /// <param name="interacter">Interacter's GameObject</param>
        public void Interact(IInteractor interacter)
        {
            if (interacter != null) Interact(interacter.ID, interacter.Owner.gameObject);
        }

        /// <summary>  Receive an empty from an gameObject </summary>
        public void Interact() => Interact(-1, null);

        public virtual void Restart()
        {
            Focused = false;
            CanInteract = true;
            CurrentActivationTime = -Cooldown;
        }
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MInteract))]
    public class MInteractEditor : UnityEditor.Editor
    {
        SerializedProperty m_ID, m_InteracterID, m_Auto, m_singleInteraction, m_Delay, m_CoolDown, events, OnFocused, OnUnfocused, Editor_Tabs1;
        protected string[] Tabs1 = new string[] { "General", "Events" };
        MInteract M;
        private void OnEnable()
        {
            M = (MInteract)target;
            m_ID = serializedObject.FindProperty("m_ID");
            m_InteracterID = serializedObject.FindProperty("m_InteracterID");
            m_Auto = serializedObject.FindProperty("m_Auto");
            m_singleInteraction = serializedObject.FindProperty("m_singleInteraction");
            m_Delay = serializedObject.FindProperty("m_Delay");
            m_CoolDown = serializedObject.FindProperty("m_CoolDown");
            events = serializedObject.FindProperty("events");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnUnfocused = serializedObject.FindProperty("OnUnfocused");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Interactable Element that invoke events when an Interactor interact with it");
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            if (Editor_Tabs1.intValue == 0) DrawGeneral();
            else DrawEvents();
           

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral()
        {
            EditorGUIUtility.labelWidth = 73;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_ID);
                EditorGUILayout.PropertyField(m_InteracterID, new GUIContent("Interacter"));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Auto);
                EditorGUILayout.PropertyField(m_singleInteraction, new GUIContent("Single"));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Delay);
                if (!M.SingleInteraction)  EditorGUILayout.PropertyField(m_CoolDown,new GUIContent("Cooldown"));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = 0;
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(events,true);
            events.isExpanded = true;
            EditorGUILayout.PropertyField(OnFocused);
            EditorGUILayout.PropertyField(OnUnfocused);
        }
    }
#endif
}