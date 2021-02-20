using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "ScriptableObjects/Consumable", order = 4)]
public class ScriptableConsumable : ScriptableItem
{
    public ScriptableAction Action;

    public override Item GetItem()
    {
        return new Consumable(this);
    }
}