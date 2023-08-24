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
    [SerializeField] private float scrollSensitivity = 3;
    private bool targetChanged = false;
    private Targetable target;
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
        Targetable newTarget;
        if (Physics.Raycast(ray, out hit, rayDistance)
            && hit.collider.gameObject.TryGetComponent<Targetable>(out newTarget))
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

        if (target != null
            && target.GetType() == typeof(Slot) //place in slot
            && Input.GetKeyDown(KeyCode.E)
            && heldItem != null)
        {
            var slot = (Slot)target;
            if (slot.IsEligible(heldItem))
            {
                var item = heldItem;
                DropHeldItem();
                slot.Place(item);
            }
        }

        else if (target != null
            && target.GetType() == typeof(Holdable) //pick up
            && Input.GetKeyDown(KeyCode.E)
            && target != null)
        {
            TryPickup((Holdable)target);
        }

        else if (Input.GetKeyDown(KeyCode.E) //drop
            && heldItem != null)
        {
            DropHeldItem();
        }


        if (heldItem != null
            && heldItem.isLink) //linked held item
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            Quaternion addRoatation = Quaternion.AngleAxis(scroll * scrollSensitivity, linkTarget.right);
            heldItem.additionalRotation = addRoatation;
        }


        if (heldItem != null
            && heldItem.isLink) //linked held item
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            Quaternion addRoatation = Quaternion.AngleAxis(scroll * scrollSensitivity, linkTarget.right);
            heldItem.additionalRotation = addRoatation;
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
                holdable.OnPickup.Invoke();
                return true;
            }
            else
            {
                Lock(holdable);
                holdable.OnPickup.Invoke();
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

        holdable.gameObject.layer = 8; //no collisions
        holdable.SetPhysics(false);
        holdable.LinkTo(linkTarget);

        heldItem = holdable;
    }

    private void DropHeldItem()
    {
        heldItem.transform.position = linkTarget.position;
        heldItem.gameObject.layer = heldItem.defaultLayer; //reenable collisions
        heldItem.SetPhysics(true);
        heldItem.transform.parent = null;
        heldItem.Unlink();
        heldItem.OnDrop.Invoke();

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
