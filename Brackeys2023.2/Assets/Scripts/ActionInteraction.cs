using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place this on objects the player can do a custom interaction with
/// </summary>
public abstract class ActionInteraction : MonoBehaviour
{
    public Sprite icon;

    /// <summary>
    /// Performs the custom action
    /// </summary>
    /// <param name="interactor">The player</param>
    public abstract void Interact(GameObject interactor);
}
