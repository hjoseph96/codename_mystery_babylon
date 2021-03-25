using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Interaction/Interactor")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/interactor")]
    public class MInteractor : UnityUtils, IInteractor
    {
        [Tooltip ("ID for the Interacter")]
        public IntReference m_ID = new IntReference(0);

        [Tooltip ("Collider set as Trigger to Find Interactables OnTrigger Enter")]
        public Collider InteractionArea;

        [Tooltip("When an Interaction is executed these events will be invoked." +
         "\n\nOnInteractWithGO(GameObject) -> will have the *INTERACTABLE* gameObject as parameter" +
         "\n\nOnInteractWith(Int) -> will have the *INTERACTABLE* ID as parameter")]
        public InteractionEvents events = new InteractionEvents();
        public GameObjectEvent OnFocused = new GameObjectEvent();
        
        public int ID => m_ID.Value;

        public bool Active { get => !enabled; set => enabled = !value; }
        
        public Transform Owner => transform;
       
        /// <summary> Current Interactable this interactor has on its Interaction Area </summary>
        public IInteractable FocusedInt  { get; internal set; }

        /// <summary>Interaction Trigger Proxy to Subsribe to OnEnter OnExit Trigger</summary>
        public TriggerProxy TriggerArea { get; set; }

        private void OnEnable()
        {
            //Trigger Area
            if (InteractionArea)
            {
                CheckTriggerProxy();
            }

            if (TriggerArea)
            {
                TriggerArea.OnTrigger_Enter.AddListener(TriggerEnter);
                TriggerArea.OnTrigger_Exit.AddListener(TriggerExit);
            }
        }


        private void OnDisable()
        {
            if (TriggerArea)
            {
                TriggerArea.OnTrigger_Enter.RemoveListener(TriggerEnter);
                TriggerArea.OnTrigger_Exit.RemoveListener(TriggerExit);
            }
        }

        private void TriggerEnter(Collider collider)
        {
            if (FocusedInt != null) FocusedInt.Focused = false; //Just in case it has an interactable stored

           var NewInter = collider.FindInterface<IInteractable>();



            if (NewInter != null && NewInter.CanInteract) //Ignore One Interaction Interactables
            {
                FocusedInt = NewInter; 

                if (FocusedInt.Auto)
                {
                    Interact(FocusedInt); //Interact if the interacter is on Auto
                }
                else
                {
                    FocusedInt.Focused = true;
                    OnFocused.Invoke(FocusedInt.Owner.gameObject);
                }
            }
        }


        private void TriggerExit(Collider collider)
        {
           var exit = collider.FindInterface<IInteractable>();

            if (FocusedInt != null && exit == FocusedInt)
            {
                FocusedInt.Focused = false;
                OnFocused.Invoke(null);
                FocusedInt = null;
            }
        }



        /// <summary> Receive an Interaction from the Interacter </summary>
        public void Interact(IInteractable inter)
        {
            if (inter != null && inter.CanInteract)
            {
                FocusedInt = inter;

                events.OnInteractWithGO.Invoke(FocusedInt.Owner.gameObject);
                events.OnInteractWith.Invoke(FocusedInt.ID);

                FocusedInt.Interact(this);
            }
        }

       

        public void Interact(GameObject interactable)
        {
            if (interactable)  Interact(interactable.FindInterface<IInteractable>());
        }

        public void Interact(Component interactable)
        {
            if (interactable) Interact(interactable.FindInterface<IInteractable>());
        }

        public void Interact() => Interact(FocusedInt);

        private void OnValidate()
        {
            if (InteractionArea)
                CheckTriggerProxy();
        }

        private void CheckTriggerProxy()
        {
            if (TriggerArea == null)
            {
                TriggerArea = InteractionArea.GetComponent<TriggerProxy>();
                if (TriggerArea == null) TriggerArea = InteractionArea.gameObject.AddComponent<TriggerProxy>();
            }
        }

        [SerializeField] private int Editor_Tabs1;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MInteractor))]
    public class MInteractorEditor : UnityEditor.Editor
    {
        SerializedProperty m_ID, InteractionArea, events, OnFocused, Editor_Tabs1;
        protected string[] Tabs1 = new string[] { "General", "Events" };
        private void OnEnable()
        {
            m_ID = serializedObject.FindProperty("m_ID");
            InteractionArea = serializedObject.FindProperty("InteractionArea");
            events = serializedObject.FindProperty("events");
            OnFocused = serializedObject.FindProperty("OnFocused");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Interactor element that invoke events when interacts with an Interactable");
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue == 0) DrawGeneral();
            else DrawEvents();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(m_ID);
            EditorGUILayout.PropertyField(InteractionArea);
            EditorGUILayout.EndVertical();
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(events, true);
            events.isExpanded = true;
            EditorGUILayout.PropertyField(OnFocused); 
        }
    }
#endif
}