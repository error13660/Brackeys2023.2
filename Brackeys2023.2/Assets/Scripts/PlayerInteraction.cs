using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles interactions between the player and other objects such as:
/// Pickung up and placeing objects.
/// Opening parcel bills and notes.
/// Using objects.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    private bool isHolding; //true is the player is currently holding something
    private bool isActive = true; //determines if any action is possible

    public Transform target; //the target used for picking up objects
    private Vector3 originalTarget; //the default position of the target
    private Holdable heldObject; //the object currently linked to the target

    [SerializeField] private float range; //the range in which interactions can be made with the enviroment

    void Update()
    {
        //Primary action
        if (Input.GetKeyDown(KeyCode.E)) //Later change this to holding down e for one/half a second
        {
            PrimaryAction();
        }
    }

    private void PrimaryAction()
    {

        if (isHolding)
        {
            Release(heldObject); //place the object
        }
        else
        {
            GameObject gameObject; //if the raycast hits a gameobject with a holdable script
            gameObject = GetTargetInRange();

            if (gameObject != null) //check for null
            {
                if (gameObject.GetComponent<Holdable>()) //chech if the raycast hit a HOLDABLE object
                {
                    Capture(gameObject.GetComponent<Holdable>()); //pick up the object
                }
            }
        }
    }

    #region Utility
    private GameObject GetTargetInRange()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
    #endregion

    #region Holdable object manupulation
    /// <summary>
    /// Attempts to place the linked holdable on a static surface.
    /// </summary>
    /// <param name="holdable"></param>
    private void Release(Holdable holdable)
    {

        if (holdable.isControlled) //if the placement method is being overridden by an inventory
        {
            Delink(holdable); //delink without placement procedure
        }
        else
        {
            Place(holdable); //delink with placement procedure
        }
    }

    /// <summary>
    /// Links the given holdable object to the target.
    /// </summary>
    /// <param name="holdable"></param>
    private void Capture(Holdable holdable)
    {

        if (holdable.isControlled)
        {
            Link(holdable); //link without pickup procedure
        }
        else
        {
            PickUp(holdable); //link with pickup procedure
        }
    }

    #region Place / Pick up
    /// <summary>
    /// Places and delinks the object.
    /// </summary>
    /// <param name="holdable"></param>
    private void Place(Holdable holdable)
    {
        if (holdable.FootprintClear())
        {
            Quaternion yaw = Quaternion.Euler(0, holdable.transform.eulerAngles.y, 0);
            float h = 0;
            int layerMask = 1 << 13; //bitwise shift operator (x13)

            //get the height above the static surface
            Ray ray = new Ray(holdable.transform.position, new Vector3(0, -1, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10/*max distance*/, layerMask/*layer mask*/)) //raycast to determine the height above the closest static surface (layer 13) (floors, shelves etc.)
            {
                h = hit.distance;
            }

            Vector3 target = (Vector3.up * (h - holdable.FromGround()) * -1)/*down to the ground +half height*/+ holdable.transform.position; //from the current position


            //placement method
            holdable.transform.rotation = yaw;
            Delink(holdable);
            StartCoroutine(LerpPlace(holdable, target));
        }
        else
        {
            //indicate that the object cannot be placed
        }
    }

    /// <summary>
    /// The version of Place() where the object's placement procedure is handled by an inventory.
    /// </summary>
    private void Delink(Holdable holdable)
    {
        target.position = originalTarget
            ;

        holdable.Delink();
        isHolding = false;
        heldObject = null;
    }

    /// <summary>
    /// Picks up and links the object.
    /// </summary>
    private void PickUp(Holdable holdable)
    {
        Link(holdable);
    }

    /// <summary>
    /// The version of PickUp() where the object's pickup procedure is handled by an inventory.
    /// </summary>
    private void Link(Holdable holdable)
    {
        originalTarget = target.position;
        target.position = holdable.transform.position;

        holdable.LinkTo(target);
        isHolding = true;
        heldObject = holdable;
    }
    #endregion

    #region Lerp
    /// <summary>
    /// Places lerps the object to place in a set amount of time and locks it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LerpPlace(Holdable holdable, Vector3 target)
    {
        float snapTime = 0.2f;
        float elapsed = 0f;
        float t = 0;
        Vector3 startPosition = holdable.transform.position;

        while (elapsed <= snapTime) //time
        {
            elapsed += Time.deltaTime;
            t = elapsed / snapTime;
            holdable.transform.position = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }
        holdable.transform.position = target;

        //reenable colliders
    }
    #endregion
    #endregion
}
