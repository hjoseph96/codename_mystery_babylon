using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS
{
    public enum TileType
    {
        Square,
        Hexagon
    }

    [Serializable]
    public class ToggleAndName
    {
        public bool toggle;
        public string name;

        public ToggleAndName(bool toggle = true)
        {
            this.toggle = toggle;
            name = "";
        }
    }

    [Serializable]
    public class CheckedView
    {
        public int degree;
        public string name;
        public ModelRotationCallback func;

        public CheckedView(int degree, string name, ModelRotationCallback func)
        {
            this.degree = degree;
            this.name = name;
            this.func = func;
        }
    }

    [Serializable]
    public class ViewProperty : PropertyBase
    {
        public const int VIEW_INITIAL_SIZE = 4;

        public float slopeAngle = 30;

        public bool showTile = false;
        public TileType tileType = TileType.Square;
        public Vector2 tileAspectRatio = new Vector2(2.0f, 1.0f);

        public int size = VIEW_INITIAL_SIZE;
        public ToggleAndName[] toggleAndNames = new ToggleAndName[VIEW_INITIAL_SIZE];
        public float initialDegree = 0f;

        public List<CheckedView> checkedViews = new List<CheckedView>();

        public ViewProperty()
        {
            for (int i = 0; i < toggleAndNames.Length; ++i)
                toggleAndNames[i] = new ToggleAndName();
        }

        public int CountCheckedViews()
        {
            int viewCount = 0;
            foreach (ToggleAndName pair in toggleAndNames)
            {
                if (pair.toggle)
                    viewCount++;
            }
            return viewCount;
        }
    }
}
