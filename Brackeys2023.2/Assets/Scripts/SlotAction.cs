using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlotAction : MonoBehaviour
{
    [SerializeField] private Slot linkedSlot;
    [SerializeField] private string requredAddInfo;

    protected virtual void Start()
    {
        linkedSlot.OnPlaced += OnPlaced;
    }

    private void OnPlaced(string addInfo)
    {
        if (!addInfo.Equals(requredAddInfo)) return;
        OnAction();
    }

    protected abstract void OnAction();
}
