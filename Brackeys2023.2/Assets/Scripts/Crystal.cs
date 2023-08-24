using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class Crystal : MonoBehaviour
{
    private bool isActive;
    [field: SerializeField] public float brightness { get; private set; } = 1;
    private Holdable holdable;
    private Crystal upstream = null; //the crystal this one gets its light from
    [SerializeField] private float brightnessLoss = 0.05f;
    public Action OnUpdate = () => { };

    public bool IsHeld { get; private set; }

    private void Start()
    {
        holdable = GetComponent<Holdable>();
        holdable.OnPickup += OnPickUp;
        holdable.OnDrop += OnDrop;
    }

    private void Update()
    {
        if (upstream != null)
        {
            Debug.DrawLine(transform.position, upstream.transform.position);
            Debug.DrawRay(transform.position, Vector3.up);
        }
    }

    private void OnPickUp()
    {
        IsHeld = true;
        upstream = null;
        OnUpdate.Invoke();

        //activate
        if (isActive) return;
        CrystalManager.Instance.RegisterCrystal(this);
        isActive = true;
    }

    private void OnDrop()
    {
        IsHeld = false;
        OnUpdtaeTriggered();
    }

    private void OnUpdtaeTriggered()
    {
        /*   if (upstream != null) upstream.OnUpdate -= OnUpdtaeTriggered;
           upstream = null;

           var updated = GetUpstream();
           upstream = updated;
           if (upstream != null) upstream.OnUpdate += OnUpdtaeTriggered;

           brightness = GetBrightness(upstream);
           OnUpdate.Invoke();
        */
        if (upstream != null) upstream.OnUpdate -= OnUpdtaeTriggered;
        upstream = null;
        brightness = GetBrightness(null); //reset the brightness to default

        upstream = GetUpstream();
        if (upstream != null) upstream.OnUpdate += OnUpdtaeTriggered;

        float newBrightness = GetBrightness(upstream);
        if (newBrightness != brightness) //if the brightness updates
        {
            brightness = newBrightness;
            OnUpdate.Invoke();
        }
    }

    private void UpdateVisuals()
    {

    }

    /// <summary>
    /// Returns the crystal with the highest light level in sight
    /// </summary>
    private Crystal GetUpstream()
    {
        return CrystalManager.Instance.GetUpstreamFrom(this);
    }

    protected virtual float GetBrightness(Crystal upstream)
    {
        if (upstream == null)
        {
            return 0;
        }
        return Mathf.Max(upstream.brightness - brightnessLoss, 0);
    }

    public bool IsLineOfSight(Crystal other)
    {
        Vector3 delta = other.transform.position - transform.position;
        Ray ray = new Ray(transform.position, delta.normalized);
        int layermask = ~(1 << 9);

        if (!Physics.Raycast(ray, delta.magnitude, layermask))
        {
            return true;
        }
        return false;
    }
}
