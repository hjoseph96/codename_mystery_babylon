using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Iteratively finds a child (on any level) with a Gameobject name of the string provided
    /// </summary>
    /// <param name="aParent"></param>
    /// <param name="aName"></param>
    /// <returns></returns>
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }
}
