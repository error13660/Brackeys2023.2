using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : Targetable
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
    [field: SerializeField] public string Identifier { get; private set; } //ex: vase, or floor tile
    [field: SerializeField] public string AdditionalInfo { get; private set; } //ex: a glyph painted on a vase
    public Action OnPickup = () => { }; //triggered when this object is picked up
    public Action OnDrop = () => { };
    [HideInInspector] public int defaultLayer = 0;
    [SerializeField] private bool isSnapToGround;
    [SerializeField] private Sprite pickUpIcon;
    [SerializeField] private Sprite pryIcon;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = !isPhysicsEnabledByDefault;
        rb.mass = mass;
        defaultLayer = gameObject.layer;
        OnDrop += OnDropped;
    }

    public void SetPhysics(bool enable)
    {
        rb.isKinematic = !enable;
    }

    protected virtual void Update()
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

    private void OnDropped()
    {
        if (!isSnapToGround) return;

        SetPhysics(false);

        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.BoxCast(transform.position,
            boundingBox / 2,
            Vector3.down,
            out hit,
            transform.rotation
            ))
        {
            transform.position = transform.position + Vector3.down * hit.distance;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    public Sprite GetIcon()
    {
        if (rb.isKinematic)
        {
            return pryIcon;
        }
        else
        {
            return pickUpIcon;
        }
    }
}
