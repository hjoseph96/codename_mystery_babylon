using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "ScriptableObjects/Consumable", order = 4)]
public class ScriptableConsumable : ScriptableItem
{
    [SerializeField] public bool canHeal;

    public ScriptableAction Action;
    public int CurrentDurability;
    public int MaxDurability;

    public override Item GetItem() => new Consumable(this);
}