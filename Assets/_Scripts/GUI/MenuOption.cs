using UnityEngine;

public abstract class MenuOption : MonoBehaviour
{
    public virtual void SetSelected()
    { }

    public virtual void SetPressed()
    { }

    public virtual void SetNormal()
    { }

    public virtual void Execute()
    { }
}

public class MenuOption<T> : MenuOption
    where T : Menu
{
    public T Menu { get; set; }
}