using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Articy.Codename_Mysterybabylon;

public class ArticyChapter
{
    Dialogue _chapterObject;

    public ArticyChapter(Dialogue chapterObj)
    {
        _chapterObject = chapterObj;
    }

    public List<string> DialogueSequenceNames()
    {
        var sequencesNames = new List<string>();

        foreach (var sequence in DialogSequences())
            sequencesNames.Add(sequence.DisplayName);

        return sequencesNames;
    }

    public Dialogue GetDialogueSequence(string sequenceName)
    {
        return DialogSequences()
            .Where((obj) => obj.DisplayName == sequenceName)
            .First();
    }

    private List<Dialogue> DialogSequences()
    {
        var dialogueSequences = new List<Dialogue>();

        for (var i = 0; i < _chapterObject.Children.Count; i++)
        {
            var childObj = _chapterObject.Children[i];

            if (childObj is Dialogue == false)
                continue;

            dialogueSequences.Add(childObj as Dialogue);
        }

        return dialogueSequences;
    }
}
