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
    [SerializeField] private bool isPhysicsEnabledByDefault = false;
    [SerializeField] private float mass = 100;
    [field: SerializeField] public float PickupTime { get; private set; }
    private Rigidbody rb;
    private Transform target;
    [SerializeField] private Vector3 boundingBox;
    [HideInInspector] public Quaternion additionalRotation;
    private Quaternion lastTargetRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = !isPhysicsEnabledByDefault;
        rb.mass = mass;
    }

    public void SetPhysics(bool enable)
    {
        rb.isKinematic = !enable;
    }

    private void Update()
    {
        if (target == null) return;

        Quaternion rotationDelta = target.rotation * Quaternion.Inverse(lastTargetRotation);

        // var colliders = Physics.OverlapBox(transform.position, boundingBox * 0.5f, rotationDelta * additionalRotation * transform.rotation);
        //  if (colliders.Length <= 1)
        //  {
        transform.rotation = rotationDelta * additionalRotation * transform.rotation;
        //  }
        additionalRotation = Quaternion.identity;
        lastTargetRotation = target.rotation;

        //  Vector3 direction = target.position - transform.position;
        //  if (!Physics.BoxCast(transform.position, boundingBox * 0.5f, direction.normalized, target.rotation, direction.magnitude))
        // { //if the path is clear
        transform.position = target.transform.position;
        //  }
    }

    public void LinkTo(Transform target)
    {
        this.target = target;
        lastTargetRotation = target.rotation;
    }

    public void Unlink()
    {
        target = null;
    }
}
