using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyItem", menuName = "ScriptableObjects/KeyItems", order = 6)]
public class ScriptableKeyItem : ScriptableItem
{
    public override Item GetItem() => new KeyItem(this);
}
