using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIGroup))]
public class AIGroupEditor : Editor
{
    
    AIGroup _target;
    SerializedProperty _groupRole;
    SerializedProperty _groupTrait;
    SerializedProperty _groupIntention;
    SerializedProperty _collaboratorGroup;
    
    void OnEnable()
    {
        _target = target as AIGroup;
        _groupRole = serializedObject.FindProperty("GroupRole");
        _groupTrait = serializedObject.FindProperty("GroupTrait");
        _groupIntention = serializedObject.FindProperty("GroupIntention");
        _collaboratorGroup = serializedObject.FindProperty("CollaboratorGroup");
        OnFormationDataChanged();
        FormationsDB.Instance.Formations.OnChanged += OnFormationDataChanged;
       
    }

    private void OnDisable()
    {
       
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _target.SelectedFormationIndex = EditorGUILayout.Popup(new GUIContent("Formation", "The current formation of this enemy group"), _target.SelectedFormationIndex, _target._formationNames.ToArray());
        EditorGUILayout.PropertyField(_groupRole);
        EditorGUILayout.PropertyField(_groupTrait);
        EditorGUILayout.PropertyField(_groupIntention);
        EditorGUILayout.PropertyField(_collaboratorGroup);
        serializedObject.ApplyModifiedProperties();
    }

    private void OnFormationDataChanged()
    {
        _target._formationNames = FormationsDB.Instance.GetNames();
    }
}
