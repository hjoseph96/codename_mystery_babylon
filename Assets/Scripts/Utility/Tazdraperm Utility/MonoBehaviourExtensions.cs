using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static void SetActive(this MonoBehaviour mb, bool state)
    {
        mb.gameObject.SetActive(state);
    }

    public static bool IsActive(this MonoBehaviour mb)
    {
        return mb.gameObject.activeSelf;
    }

    public static void Show(this MonoBehaviour mb)
    {
        mb.SetActive(true);
    }

    public static void Hide(this MonoBehaviour mb)
    {
        mb.SetActive(false);
    }

    public static void SetLayer(this GameObject go, int layer, bool isRecursive = false)
    {
        go.layer = layer;
        if (isRecursive)
        {
            foreach (Transform child in go.transform)
            {
                SetLayer(child.gameObject, layer, true);
            }
        }
    }

    public static void SetLayer(this MonoBehaviour mb, int layer, bool isRecursive = false)
    {
        mb.gameObject.layer = layer;
        if (isRecursive)
        {
            foreach (Transform child in mb.transform)
            {
                SetLayer(child.gameObject, layer, true);
            }
        }
    }
}