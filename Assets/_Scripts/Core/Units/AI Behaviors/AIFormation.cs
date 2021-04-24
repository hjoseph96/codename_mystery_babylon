using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIFormation
{
    public string Name;

    public int[,] Grid;
    public int[] grid;
    [SerializeField]
    private int _width;
    public int Width { get { return _width; } private set { _width = value; } }
    [SerializeField]
    private int _height;
    public int Height { get { return _height; } private set { _height = value; } }
    public Vector2Int Pivot;

    [Button("Show In Builder")]
    private void ShowInBuilder()
    {
        FormationBuilderView.ShowWindow(this);
    }

    public AIFormation()
    {
        Pivot = new Vector2Int(0, 0);
        Name = "Unnamed";
        Init(2, 2);

    }

    public AIFormation(Vector2Int bounds, Vector2Int pivot, string name = "Unnamed")
    {
        Pivot = pivot;
        Name = name;
        Init(bounds.x, bounds.y);

    }

    public AIFormation(Vector2 bounds, Vector2Int pivot, string name = "Unnamed")
    {
        Pivot = pivot;
        Name = name;
        Init((int)bounds.x, (int)bounds.y);
    }

    public void UpdatePositionIndexesBeyond(int index)
    {


        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (this[i, j] > index)
                    this[i, j]--;
            }
        }

    }

    public int GetNewPositionIndex()
    {
        var max = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (this[i, j] > max)
                    max = this[i, j];
            }
        }

        return ++max;
    }

    public void Init(int x, int y)
    {
        Width = x;
        Height = y;
        grid = new int[x * y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                grid[i + x * j] = -1;
            }
        }
    }

    public Dictionary<int, Vector2Int> GetEffectivePositions()
    {
        Dictionary<int, Vector2Int> effectivePositions = new Dictionary<int, Vector2Int>();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (this[i, j] != -1)
                    effectivePositions.Add(this[i, j], new Vector2Int(i, j));
            }
        }

        return effectivePositions;
    }

    public Dictionary<int, Vector2Int> GetEffectivePositions(Vector2Int worldPos)
    {
        Dictionary<int, Vector2Int> effectivePositions = new Dictionary<int, Vector2Int>();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (this[i, j] != -1)
                    effectivePositions.Add(this[i, j], new Vector2Int(worldPos.x + i, worldPos.y + j));
            }
        }

        return effectivePositions;
    }

    public int this[int i, int j]
    {
        get
        {
            return grid[i + Width * j];
        }
        set
        {
            grid[i + Width * j] = value;

        }
    }

}
