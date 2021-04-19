using UnityEngine;

public static class Color32Extensions
{
    public static bool EqualsTo(this Color32 col1, Color32 col2)
    {
        return col1.r == col2.r && col1.g == col2.g && col1.b == col2.b && col1.a == col2.a;
    }
}