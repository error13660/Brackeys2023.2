using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : MonoBehaviour
{
    /// <summary>
    /// If true the object doesn't lock, instead it hovers in front of the player allowing manipulation
    /// </summary>
    [field: SerializeField] public bool isLink { get; private set; }
    [field: SerializeField] public float PickupTime { get; private set; }
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPhysics(bool enable)
    {
        rb.isKinematic = !enable;
    }
}
