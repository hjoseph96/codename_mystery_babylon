using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Codename_Mysterybabylon;
using Articy.Unity;

public class ArticyDataContainer : SerializedMonoBehaviour
{
    [ValueDropdown("ArticyObjectNames"), SerializeField]
    private string _articyObjectName;

    [Button("Add Articy Reference"), PropertyOrder(100)]
    private void AddArticyReference()
    {
        _dialogueDisplayNames.Add(_articyObjectName);
    }

    private List<string> ArticyObjectNames()
    {
        var objNames = new List<string>();
        var objects = ArticyDatabase.GetAllOfType<Dialogue>();

        foreach (var obj in objects)
            objNames.Add(obj.DisplayName);

        return objNames;
    }

    [SerializeField, PropertyOrder(101)]
    public List<string> _dialogueDisplayNames;

    private List<ArticyObject> _references = new List<ArticyObject>();
    public List<ArticyObject> References { get => _references; }

    private void Awake()
    {
        SetReferences();   
    }

    public void Empty()
    {
        _dialogueDisplayNames = new List<string>();
        _references = new List<ArticyObject>();

        var mapDialogue = GetComponent<MapDialogue>();
        if (mapDialogue != null)
            mapDialogue.Clear();
    }

    public void AddDialogue(string name)
    {
        if (_dialogueDisplayNames.Contains(name))
            return;

        if (!ArticyObjectNames().Contains(name))
            throw new Exception($"[ArticyDataContainer] There is no Dialogue in Articy titled: {name}");

        _dialogueDisplayNames.Add(name);
    }

    public void SetReferences()
    {
        var dialogues = ArticyDatabase.GetAllOfType<Dialogue>();

        foreach (var displayName in _dialogueDisplayNames)
        {
            var matchingDialogue = dialogues.Where((d) => d.DisplayName == displayName);

            if (matchingDialogue.Count() > 0)
                _references.Add(matchingDialogue.First());
            else
                throw new Exception($"[ArticyDataContainer] There is no Dialogue in Articy titled: {displayName}");
        }
    }

}
