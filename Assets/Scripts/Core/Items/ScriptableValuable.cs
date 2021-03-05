using UnityEngine;

[CreateAssetMenu(fileName = "NewValuable", menuName = "ScriptableObjects/Valuable", order = 5)]
public class ScriptableValuable : ScriptableItem
{
    public override Item GetItem()
    {
        return new Valuable(this);
    }
}