using System.Collections.Generic;
using Tazdraperm_Utility;
using UnityEngine;


public class Healthbar : MonoBehaviour
{
    public const int HealthRowSize = 20;

    [SerializeField] private HealthRow _healthRowPrefab;
    [SerializeField] private Sprite _fillSprite;

    private readonly List<HealthRow> _healthRows = new List<HealthRow>();

    public void Fill(int maxHealth, int currentHealth)
    {
        var rowCount = Mathf.CeilToInt((float) maxHealth / HealthRowSize);
        for (var i = 0; i < rowCount; i++)
        {
            var row = Instantiate(_healthRowPrefab, transform, false);
            row.Init(_fillSprite);

            row.Fill(MathUtility.ClampInt( maxHealth, 0, HealthRowSize), MathUtility.ClampInt(currentHealth, 0, HealthRowSize));
            maxHealth -= HealthRowSize;
            currentHealth -= HealthRowSize;

            _healthRows.Add(row);
        }
    }

    public void Clear(int rowId, int pointId)
    {
        _healthRows[rowId].Clear(pointId);
    }

    public void OnDisable()
    {
        foreach (var row in _healthRows)
            Destroy(row.gameObject);

        _healthRows.Clear();
    }
}
