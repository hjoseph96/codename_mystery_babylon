using Sirenix.OdinInspector;

public abstract class ScriptableAction : SerializedScriptableObject
{
    public abstract void Use(Unit unit);
}