using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityObservables;

# if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(fileName = "Enemy Formations", menuName = "ScriptableObjects/Formations")]
public class FormationsDB : ScriptableObject, IInitializable
{
    public static FormationsDB Instance;
    [Button("Add Formation")]
    private void AddFormation()
    {
        Formations.Add(new AIFormation());
    #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
    #endif
    }

    public void Init()
    {
        Instance = Instance == null ? Resources.Load<FormationsDB>("Formations") : Instance;
    }
    
    
    [SerializeField]
    public ObservableList<AIFormation> Formations = new ObservableList<AIFormation>();

    public List<string> GetNames()
    {

        return Formations.Select(f => f.Name).ToList();
    }

    public AIFormation Get(string name)
    {
        return Formations.Select(f => f).Where(f => f.Name == name).FirstOrDefault();
    }

    public void Save(AIFormation formation)
    {
        Formations.Remove(formation);
        Formations.Add(formation);
        //EditorUtility.SetDirty(this);

    #if UNITY_EDITOR
        Undo.RecordObject(this, "Saving formations");
    #endif
    }

}
