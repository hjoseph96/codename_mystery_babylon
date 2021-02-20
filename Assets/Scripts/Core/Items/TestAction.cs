using UnityEngine;

[CreateAssetMenu(fileName = "NewTestAction", menuName = "ScriptableObjects/Actions/TestAction", order = 1)]
public class TestAction : ScriptableAction
{
    public int Value;

    public override void Use(Unit unit)
    {
        Debug.Log(unit.transform.name + "___" + Value);
    }
}