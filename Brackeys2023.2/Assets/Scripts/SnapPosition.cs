using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPosition
{
    /// <summary>
    /// The block of grid cells this snap position occupies
    /// </summary>
    public Rect gridRect;
    /// <summary>
    /// The center of the occupied area in the object's local space
    /// </summary>
    public Vector3 position;
    /// <summary>
    /// The parent object
    /// </summary>
    private GameObject parent;
    /// <summary>
    /// The parent inventory this snap position is a part of
    /// </summary>
    private TetrisInventory_v2 inventory;

    /// <summary>
    /// The gameObject spawned to indicate a snapping point to the player
    /// </summary>
    private GameObject indicator;

    public SnapPosition(Rect gridRect, TetrisInventory_v2 inventory)
    {
        this.gridRect = gridRect;
        this.position = inventory.RectCenter(gridRect);
        this.parent = inventory.gameObject;
        this.inventory = inventory;
    }

    public void Spawn()
    {
        indicator = inventory.snappointIndicator;
        //Resources.Load("Indicator", typeof(GameObject)) as GameObject;
        indicator = Object.Instantiate(indicator, parent.transform); //swap the indicator field from a reference to the instanciated indicator object
        indicator.transform.localPosition = position;
        //Debug.Log("Grid rect:\n" +
        //    "x=" + x + " y=" + y + " width=" + width + " height=" + height);
    }

    public void Destroy()
    {
        GameObject.Destroy(indicator);
    }

    /// <summary>
    /// Marks the footprint area as occupied in the inventory grid, thereby confirming the snapping of the object.
    /// </summary>
    public void Apply()
    {
        inventory.ApplySnapPosition(gridRect);
    }

    public void Free()
    {
        inventory.FreeSnapPosition(gridRect);
    }
}
