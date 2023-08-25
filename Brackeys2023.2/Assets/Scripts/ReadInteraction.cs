using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadInteraction : ActionInteraction
{
    [SerializeField] private string text;

    public override void Interact(GameObject interactor)
    {
        UIManager.Instance.DisplayText(text);
    }
}
