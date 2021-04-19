using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public interface IInitializable
{
    void Init();
}

public class EntryPoint : SerializedMonoBehaviour, IInitializable
{
    [SerializeField] private bool _InitOnAwake = true;
    public bool InitOnAwake { get => _InitOnAwake; }
    public IInitializable[] ToInit;

    private void Awake()
    {
        if (_InitOnAwake)
            Init();
    }

    public void Init()
    {
        for (var i = 0; i < ToInit.Count(); i++)
        {
            //if (i == ToInit.Length - 1)
            var initializable = ToInit[i];
            initializable.Init();
        }
    }
}
