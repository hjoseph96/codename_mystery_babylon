using System;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class DoorLock : MonoBehaviour
{
    [SerializeField] private ScriptableItem _itemToOpen;

    private Animator _animator;

    [ShowInInspector] public bool IsLocked { get; private set; }

    [HideInInspector] public Action UponUnlocked;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        IsLocked = true;
    }

    public void Unlock()
    {
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Unlock"))
            _animator.Play("Unlock");
    }

    public bool CanOpen(Unit unit)
    {
        foreach (var item in unit.Inventory.GetItems<Item>())
            if (item.Name == _itemToOpen.Name)
                return true;

        return false;
    }

    // Animation Events

    private void SetLocked() => IsLocked = true;

    private void SetUnlocked()
    {
        IsLocked = false;

        if (UponUnlocked != null)
        {
            UponUnlocked.Invoke();
            UponUnlocked = null;
        }
    }
}
