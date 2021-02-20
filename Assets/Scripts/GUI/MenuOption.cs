using UnityEngine;

public abstract class MenuOption : MonoBehaviour
{
    public virtual void SetSelected()
    { }

    public virtual void SetDeselected()
    { }

    public virtual void Execute()
    { }
}

public class MenuOption<T> : MenuOption
    where T : Menu
{
    public T Menu { get; set; }
}