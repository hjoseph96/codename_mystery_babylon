using UnityEngine;


public class HealthRow : MonoBehaviour
{
    private HealthPoint[] _healthPoints;

    public void Init(Sprite fillSprite)
    {
        _healthPoints = GetComponentsInChildren<HealthPoint>();
        Debug.Assert(_healthPoints.Length == Healthbar.HealthRowSize);

        foreach (var hp in _healthPoints)
            hp.Init(fillSprite);
    }

    public void Fill(int maxHealth, int currentHealth)
    {
        for (var i = 0; i < Healthbar.HealthRowSize; i++)
        {
            if (i >= maxHealth)
                _healthPoints[i].Hide();
            else if (i >= currentHealth)
                _healthPoints[i].Clear();
            else
                _healthPoints[i].Fill();
        }
    }

    public void Clear(int pointId)
    {
        _healthPoints[pointId].Clear();
    }
}
