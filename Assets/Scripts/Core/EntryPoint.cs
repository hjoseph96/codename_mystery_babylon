using Sirenix.OdinInspector;

public interface IInitializable
{
    void Init();
}

public class EntryPoint : SerializedMonoBehaviour
{
    public IInitializable[] ToInit;

    private void Awake()
    {
        foreach (var initializable in ToInit)
        {
            initializable.Init();
        }
    }
}
