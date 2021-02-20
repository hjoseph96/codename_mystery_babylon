using UnityEngine;

[CreateAssetMenu(fileName = "NewValuable", menuName = "ScriptableObjects/Valuable", order = 5)]
public class ScriptableValuable : ScriptableItem
{
    public uint Cost;

    public override Item GetItem()
    {
        return new Valuable(this);
    }
}