﻿using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(Zone))]
    [CanEditMultipleObjects]
    public class ZoneEditor : Editor
    {
        private Zone M;

        SerializedProperty
            HeadOnly, stateAction, HeadName, zoneType, stateID, modeID, modeIndex, ActionID, auto, StatModifierOnEnter, StatModifierOnExit, ShowStatModifiers,
            AutomaticDisabled, stanceAction,layer, statModifier, stanceID, RemoveAnimalOnActive, m_tag, ModeFloat, Force, EnterAceleration, ExitAceleration, Bounce;
            


        MonoScript script;
        private void OnEnable()
        {
            M = ((Zone)target);
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            HeadOnly = serializedObject.FindProperty("HeadOnly");
            RemoveAnimalOnActive = serializedObject.FindProperty("RemoveAnimalOnActive");
            HeadName = serializedObject.FindProperty("HeadName");
            layer = serializedObject.FindProperty("Layer");


            Force = serializedObject.FindProperty("Force");
            EnterAceleration = serializedObject.FindProperty("EnterDrag");
            ExitAceleration = serializedObject.FindProperty("ExitDrag");
            Bounce = serializedObject.FindProperty("Bounce");


            m_tag = serializedObject.FindProperty("tags");
            ModeFloat = serializedObject.FindProperty("ModeFloat");
            zoneType = serializedObject.FindProperty("zoneType");
            stateID = serializedObject.FindProperty("stateID");
            stateAction = serializedObject.FindProperty("stateAction");
            stanceAction = serializedObject.FindProperty("stanceAction");
            modeID = serializedObject.FindProperty("modeID");
            stanceID = serializedObject.FindProperty("stanceID");
            modeIndex = serializedObject.FindProperty("modeIndex");
            ActionID = serializedObject.FindProperty("ActionID");
            auto = serializedObject.FindProperty("automatic");
            AutomaticDisabled = serializedObject.FindProperty("AutomaticDisabled");

            statModifier = serializedObject.FindProperty("StatModifierOnActive");
            StatModifierOnEnter = serializedObject.FindProperty("StatModifierOnEnter");
            StatModifierOnExit = serializedObject.FindProperty("StatModifierOnExit");
            ShowStatModifiers = serializedObject.FindProperty("ShowStatModifiers");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Area to modify States, Stances or Modes on an Animal");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //zoneType.intValue = (int)(ZoneType)EditorGUILayout.EnumPopup(new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"), (ZoneType)zoneType.intValue);
                EditorGUILayout.PropertyField(zoneType, new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"));
                EditorGUILayout.PropertyField(layer, new GUIContent("Animal Layer", "What Layer to detect"));

                ZoneType zone = (ZoneType)zoneType.intValue;


                switch (zone)
                {
                    case ZoneType.Mode:
                        EditorGUILayout.PropertyField(modeID, new GUIContent("Mode ID", "Which Mode to Set when entering the Zone"));

                        serializedObject.ApplyModifiedProperties();


                        if (M.modeID != null && M.modeID == 4)
                        {
                            EditorGUILayout.PropertyField(ActionID, new GUIContent("Action Index", "Which Action to Set when entering the Zone"));

                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Action ID", MessageType.Error);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(modeIndex, new GUIContent("Ability Index", "Which Ability to Set when entering the Zone"));
                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Ability ID", MessageType.Error);
                            }
                        }

                        EditorGUILayout.PropertyField(ModeFloat);
                            EditorGUILayout.PropertyField(auto, new GUIContent("Automatic", "As soon as the animal enters the zone play the action"));
                            if (auto.boolValue)
                            {
                                EditorGUILayout.PropertyField(AutomaticDisabled, new GUIContent("Disabled", "The zone will be disable after x seconds"));
                                if (AutomaticDisabled.floatValue < 0) AutomaticDisabled.floatValue = 0;
                            }
                        break;
                    case ZoneType.State:
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State ID", "Which State will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stateAction, new GUIContent("Action", "Set what action for State the animal will apply when entering the zone"));
                        if (stateID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an State ID", MessageType.Error);
                        }
                        break; 
                    case ZoneType.Stance:
                        EditorGUILayout.PropertyField(stanceID, new GUIContent("Stance ID", "Which Stance will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stanceAction, new GUIContent("Action", "Set what action for stance the animal will apply when entering the zone"));
                        if (stanceID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an Stance ID", MessageType.Error);
                        }
                        break;
                    case ZoneType.Force:
                        EditorGUILayout.PropertyField(Force);
                        EditorGUILayout.PropertyField(EnterAceleration);
                        EditorGUILayout.PropertyField(ExitAceleration);
                        EditorGUILayout.PropertyField(Bounce);
                        break;
                    default:
                        break;
                }



                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    if (zone != ZoneType.Force)
                    EditorGUILayout.PropertyField(RemoveAnimalOnActive, new GUIContent("Remove Animal on Active", "Remove the Stored Animal on the Zone when the zones gets Active, Reseting the zone to its default state"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_tag, new GUIContent("Tags", "Set this parameter if you want the zone to Interact only with gameObject with that tag"));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(HeadOnly, new GUIContent("Head Only", "Activate when the Head Bone the Zone.\nThe Head Bone needs a collider and be Named Head"));
                    if (HeadOnly.boolValue)
                    {
                        EditorGUILayout.PropertyField(HeadName, new GUIContent("Head Name", "Name for the Head Bone"));
                    }
                }
                EditorGUILayout.EndVertical();


                if (zone == ZoneType.Mode)
                {
                   
                }


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    ShowStatModifiers.boolValue = EditorGUILayout.Foldout(ShowStatModifiers.boolValue, "Stat Modifiers");
                    EditorGUI.indentLevel--;

                    if (ShowStatModifiers.boolValue)
                    {
                        EditorGUILayout.PropertyField(StatModifierOnEnter, new GUIContent("On Enter", "Modify the Stat entering the Zone"), true);
                        EditorGUILayout.PropertyField(StatModifierOnExit, new GUIContent("On Exit", "Modify the Stat exiting the Zone"), true);
                        EditorGUILayout.PropertyField(statModifier, new GUIContent("On Active", "Modify the Stat when the Zone is Active"), true);
                    }
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                M.EditorShowEvents = EditorGUILayout.Foldout(M.EditorShowEvents, "Events");
                EditorGUI.indentLevel--;

                if (M.EditorShowEvents)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("On Animal Enter"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExit"), new GUIContent("On Animal Exit"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnZoneActivation"), new GUIContent("On Zone Active"));
                }
                EditorGUILayout.EndVertical();
 
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zone Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}