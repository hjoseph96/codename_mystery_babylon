using System;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;
using Sirenix.OdinInspector;

public class DialogueChoiceGUI : MonoBehaviour
{
    [HideInInspector] public Action<Branch> OnChoiceSelected;

    [SerializeField] private List<SingleDialogChoice> dialogGUIs;

    private IList<Branch> _aBranches;
    public void Show(IList<Branch> articyBranches)
    {
        _aBranches = articyBranches;

        if (_aBranches.Count > 4)
            throw new Exception($"[DialogueChoiceGUI] Too many dialogue choices given: #{articyBranches.Count}");

        for (var i = 0; i < _aBranches.Count; i++)
        {
            var dialogOption = dialogGUIs[i];
            var dialogueFragment = _aBranches[i].Target as DialogueFragment;

            dialogOption.OnChoiceSelected += delegate (Branch chosenBranch)
           {
               OnChoiceSelected.Invoke(chosenBranch);
               OnChoiceSelected = null;

               this.SetActive(false);
           };
            dialogOption.Populate(_aBranches[i], dialogueFragment.MenuText);
        }

        this.SetActive(true);
    }
}
