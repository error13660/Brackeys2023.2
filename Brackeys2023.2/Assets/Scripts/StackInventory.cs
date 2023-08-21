using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackInventory : TetrisInventory_v2
{
    [HideInInspector] public bool isOnStack; //if the object is on a stack, it can't function as an inventory
    [HideInInspector] public bool isStackBase; //if the object is the base of a stack it can't be put on a stack
    /// <summary>
    /// The key that shows that two stackable objects are compatible
    /// </summary>
    public string objectSignature; 
    //ex: cement.objectSignature == bag
    //    instantConcrete.objectSignature == bag
    //so the two objects can be stacked

    #region Events
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<StackInventory>()) //if there is a stack inventory with the same signature as this
        {
            if (!isOnStack & other.GetComponent<StackInventory>().objectSignature == this.objectSignature & !other.GetComponent<StackInventory>().isStackBase)
            { //yepp, it also can't be a base
                base.OnTriggerEnter(other);
            }
        }
    }

    public override void Lock(Holdable holdable)
    {
        base.Lock(holdable);

        holdable.GetComponent<StackInventory>().isOnStack = true;
        this.isStackBase = true;
        holdable.gameObject.layer = 12;
    }

    protected override void Unlock(Holdable holdable)
    {
        base.Unlock(holdable);

        holdable.GetComponent<StackInventory>().isOnStack = false;
        this.isStackBase = false;
        holdable.gameObject.layer = 0;
    }

    protected override Rect GridFootprint(Rect rect)
    {
        return new Rect(0, 0, 1, 1);
    }
    #endregion

}
