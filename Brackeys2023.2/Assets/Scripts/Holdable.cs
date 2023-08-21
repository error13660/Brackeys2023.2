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
    [SerializeField] private bool IsPhysicsEnabledByDefault = false;
    [SerializeField] private float mass = 100;
    [field: SerializeField] public float PickupTime { get; private set; }
    [field: SerializeField] public string InteractMessage { get; private set; }
    private Rigidbody rb;
    private Transform target;
    [SerializeField] private Vector3 boundingBox;
    [HideInInspector] public Quaternion additionalRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = !IsPhysicsEnabledByDefault;
    }

    private void Update()
    {
        if (target == null) return;

        var colliders = Physics.OverlapBox(transform.position, boundingBox * 0.5f, transform.rotation * additionalRotation);
        if (colliders.Length <= 1)
        {
            transform.rotation = transform.rotation * additionalRotation;
        }
        additionalRotation = Quaternion.identity;

        Vector3 direction = target.position - transform.position;
        if (!Physics.BoxCast(transform.position, boundingBox * 0.5f, direction.normalized, target.rotation, direction.magnitude))
        { //if the path is clear
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
        }
    }

    public void SetPhysics(bool enable)
    {
        rb.isKinematic = !enable;
    }

    public void LinkTo(Transform target)
    {
        this.target = target;
    }

    public void Unlink()
    {
        target = null;
    }
}
