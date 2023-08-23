using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class Crystal : MonoBehaviour
{
    private bool isActive;
    public float brightness { get; private set; }

    private Holdable holdable;

    public void Activate()
    {
        if (isActive) return;

        CrystalManager.Instance.RegisterCrystal(this);
        isActive = true;
    }

    private void Start()
    {
        holdable = GetComponent<Holdable>();
        holdable.OnPickup += Activate;
    }

    /// <summary>
    /// Returns the crystal with the highest light level in sight
    /// </summary>
    private Crystal GetUpstream()
    {
        CrystalManager.Instance.GetUpstreamFrom(this);
    }
}
