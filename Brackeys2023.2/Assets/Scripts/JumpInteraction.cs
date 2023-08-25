using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpInteraction : ActionInteraction
{
    [SerializeField] private Transform destination;

    public override void Interact(GameObject interactor)
    {
        interactor.transform.position = destination.position;
        interactor.transform.rotation = destination.rotation;
    }
}
