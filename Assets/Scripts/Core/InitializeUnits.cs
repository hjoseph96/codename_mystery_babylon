using UnityEngine;

public class InitializeUnits : MonoBehaviour, IInitializable
{
    public void Init()
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            unit.Init();
        }
    }
}
