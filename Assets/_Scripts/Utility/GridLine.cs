using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLine : IEnumerable<Vector2Int>
{
    public readonly Vector2Int Start, End;
    public readonly LineOrientation Orientation;

    public Vector2Int this[int ind] => _points[ind];
    public int Length => _points.Count;

    private readonly List<Vector2Int> _points;
    private readonly GridLineEnumerator _enumerator;

    public GridLine(Vector2Int start, Vector2Int end)
    {
        Start = start;
        End = end;
        _points = new List<Vector2Int>();

        var dx = Mathf.Abs(end.x - start.x);
        var dy = Mathf.Abs(end.y - start.y);
        if (dy < dx)
        {
            if (start.x > end.x)
            {
                GetPointsLow(end, start, _points);
                _points.Reverse();
            }
            else
            {
                GetPointsLow(start, end, _points);
            }

            Orientation = LineOrientation.Horizontal;
        }
        else
        {
            if (start.y > end.y)
            {
                GetPointsHigh(end, start, _points);
                _points.Reverse();
            }
            else
            {
                GetPointsHigh(start, end, _points);
            }

            Orientation = dy == dx ? LineOrientation.Diagonal : LineOrientation.Vertical;
        }

        _enumerator = new GridLineEnumerator(_points);
    }

    public IEnumerator<Vector2Int> GetEnumerator()
    {
        _enumerator.Reset();
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Resolve(int x)
    {
        var tempStart = Start;
        var tempEnd = End;
        if (End.x == Start.x)
            return 99999;
        else if (End.x < Start.x)
        {
            var dummy = tempEnd;
            tempEnd = tempStart;
            tempStart = dummy;
        }

        float slope = (tempEnd.y - tempStart.y) / (tempEnd.x - tempStart.x);
        float constant = -(slope * tempStart.x) + tempEnd.y;
        return Mathf.CeilToInt(slope * x + constant);
    }

    private static void GetPointsHigh(Vector2Int start, Vector2Int end, ICollection<Vector2Int> pointsList)
    {
        var deltaX = Mathf.Abs(start.x - end.x);
        var deltaY = Mathf.Abs(start.y - end.y);
        var error = 0;
        var deltaError = deltaX + 1;
        var dx = Math.Sign(end.x - start.x);
        var x = start.x;

        for (var y = start.y; y <= end.y; y++)
        {
            if (error >= deltaY + 1)
            {
                x += dx;

                if (error != deltaY + 1)
                {
                    pointsList.Add(new Vector2Int(x, y - 1));
                }

                error -= deltaY + 1;
            }

            pointsList.Add(new Vector2Int(x, y));
            error += deltaError;
        }
    }

    private static void GetPointsLow(Vector2Int start, Vector2Int end, ICollection<Vector2Int> pointsList)
    {
        var deltaX = Mathf.Abs(start.x - end.x);
        var deltaY = Mathf.Abs(start.y - end.y);
        var error = 0;
        var deltaError = deltaY + 1;
        var dy = Math.Sign(end.y - start.y);
        var y = start.y;

        for (var x = start.x; x <= end.x; x++)
        {
            if (error >= deltaX + 1)
            {
                y += dy;

                if (error != deltaX + 1)
                {
                    pointsList.Add(new Vector2Int(x - 1, y));
                }

                error -= deltaX + 1;
            }

            pointsList.Add(new Vector2Int(x, y));
            error += deltaError;
        }
    }

    private class GridLineEnumerator : IEnumerator<Vector2Int>
    {
        public Vector2Int Current => _points[_currentIndex];
        object IEnumerator.Current => Current;

        private int _currentIndex = -1;
        private readonly List<Vector2Int> _points;

        public GridLineEnumerator(List<Vector2Int> points)
        {
            _points = points;
        }

        public bool MoveNext()
        {
            _currentIndex++;

            return _currentIndex < _points.Count;
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public void Dispose() { }
    }
}