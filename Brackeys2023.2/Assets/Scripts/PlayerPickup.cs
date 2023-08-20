using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Transform linkTarget;
    [SerializeField] private Transform lockTarget;
    [SerializeField] private float rayDistance;
    private bool targetChanged = false;
    private Holdable target;
    private Holdable heldItem;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        targetChanged = false;

        //calculate if target has changed
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;
        Holdable newTarget;
        if (Physics.Raycast(ray, out hit, rayDistance)
            && hit.collider.gameObject.TryGetComponent<Holdable>(out newTarget))
        {

            if (target == null)
            {
                targetChanged = true;
                target = newTarget;
            }

            Debug.DrawRay(hit.point, hit.normal);
            if (newTarget != target)
            {
                targetChanged = true;
                target = newTarget;
            }
        }
        else
        {
            target = null;
        }

        if (Input.GetKeyDown(KeyCode.E)
            && heldItem != null)
        {
            DropHeldItem();
        }

        if (Input.GetKeyDown(KeyCode.E)
            && target != null)
        {
            TryPickup(target);
        }
    }

    private async Task<bool> TryPickup(Holdable holdable)
    {
        bool isSuccess = await IsTargetHeldFor(holdable.PickupTime, KeyCode.E);
        if (isSuccess)
        {
            if (holdable.isLink)
            {
                Link(holdable);
                return true;
            }
            else
            {
                Lock(holdable);
                return true;
            }
        }
        return false;
    }

    private void Lock(Holdable holdable)
    {
        holdable.gameObject.layer = 8; //no collisions
        holdable.transform.position = lockTarget.position;
        holdable.transform.rotation = lockTarget.transform.rotation;
        holdable.transform.parent = lockTarget;
        holdable.SetPhysics(false);

        heldItem = holdable;
    }

    private void Link(Holdable holdable)
    {
        holdable.SetPhysics(false);

        heldItem = holdable;
    }

    private void DropHeldItem()
    {
        heldItem.gameObject.layer = 0; //reenable collisions
        heldItem.SetPhysics(true);
        heldItem.transform.parent = null;

        heldItem = null;
    }

    /// <returns>True if the target doesn't change</returns>
    private async Task<bool> IsTargetedFor(float seconds)
    {
        float t0 = Time.time;

        while (!targetChanged)
        {
            await Task.Yield();
            if (Time.time - t0 >= seconds) return true;
        }
        return false;
    }

    /// <returns>True if the target doesn't change and the key isn't released</returns>
    private async Task<bool> IsTargetHeldFor(float seconds, KeyCode key)
    {
        float t0 = Time.time;

        while (!targetChanged
            && Input.GetKey(key))
        {
            await Task.Yield();
            if (Time.time - t0 >= seconds) return true;
        }
        return false;
    }
}