using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : Targetable
{
    [SerializeField] private string acceptableIdentifier;
    private Holdable placed;
    [SerializeField] private Transform placedPosition;
    [SerializeField] private bool overrideRotation;
    [SerializeField] private Quaternion rotation;
    public Action OnPlaced = () => { };
    public Action OnActivated = () => { };
    public Sprite icon;

    public bool IsEligible(Holdable holdable)
    {
        return placed == null
            && holdable.Identifier.Equals(acceptableIdentifier);
    }

    public void Place(Holdable holdable)
    {
        placed = holdable;
        holdable.OnPickup += OnPlacedObjectPickedUp;
        holdable.transform.position = placedPosition.position;
        holdable.SetPhysics(false);

        if (overrideRotation)
        {
            holdable.transform.rotation = rotation;
        }
        OnPlaced.Invoke();
    }

    public string LookatMessage()
    {
        if (placed != null)
        {
            return "Full";
        }
        return "Place";
    }

    private void OnPlacedObjectPickedUp()
    {
        placed.OnPickup -= OnPlacedObjectPickedUp;
        placed = null;
    }

    public string GetPlacedAdditionalInfo()
    {
        return placed?.AdditionalInfo;
    }

    public void SealPlaced()
    {
        Destroy(placed);
        Destroy(this);
    }
}
