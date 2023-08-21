using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisInventory_v2 : MonoBehaviour
{
    [Header("Inventory grid")]
    [SerializeField] private int cellsX;
    [SerializeField] private int cellsZ;
    private bool[,] grid;

    [Header("Inventory area")]
    [SerializeField] private BoxCollider operationArea;
    [SerializeField] private float baseHeight;
    private float unitX;
    private float unitZ;
    private Vector3 gridOrigin;

    [Header("Snapping")]
    [SerializeField] private float snapTime;
    [SerializeField] protected float escapeDistance;

    [Header("Visuals")]
    public GameObject snappointIndicator;


    //Snapping
    protected SnapPosition[] snapPositions;
    private Holdable snappingObject;

    //contents
    [HideInInspector] public List<Holdable> contents = new List<Holdable>(); //mainly used for saving

    private void Awake()
    {
        unitX = operationArea.size.x / cellsX;
        unitZ = operationArea.size.z / cellsZ;

        grid = new bool[cellsX, cellsZ];

        float originX = operationArea.size.x / -2 + operationArea.center.x;
        float originZ = operationArea.size.z / -2 + operationArea.center.z;
        gridOrigin = new Vector3(originX, baseHeight, originZ);
    }

    #region Events
    protected virtual void OnTriggerEnter(Collider other)
    {
        Holdable holdable;
        if ((holdable = other.GetComponent<Holdable>()) != null)
        {
            if (holdable.isHeld && !holdable.isSnapping)
            {
                /*Debug.Log("Holdable object confirmed");*/

                Rect footprint = holdable.Footprint();
                snapPositions = FreePositions(footprint);
                ShowSnappingPoints();

                if (snapPositions.Length > 0)
                {
                    Capture(holdable);
                    StartCoroutine(Manage(holdable));
                }
            }
        }
    }

    public void OnLinked(Holdable holdable)
    {
        Unlock(holdable);
        StartCoroutine(LerpEscape(holdable));
    }

    private void OnDelinked(Holdable holdable)
    {
        Lock(holdable);
        StopCoroutine(Manage(holdable));
    }
    #endregion

    #region Utility (presnap)
    /// <summary>
    /// Returns an integer sided rectangular area this input rectangle would take up in this inventory grid.
    /// </summary>
    /// <returns></returns>
    protected virtual Rect GridFootprint(Rect rect)
    {
        Rect gridRect = new Rect();
        gridRect.width = (int)(rect.width / unitX) + 1;
        gridRect.height = (int)(rect.height / unitZ) + 1;

        return gridRect;
    }

    /// <summary>
    /// Returns the position of the given rectangular area if grid cells' center in local space.
    /// </summary>
    /// <returns></returns>
    public Vector3 RectCenter(Rect gridRect)
    {
        float x = gridRect.x * unitX;
        float z = gridRect.y * unitZ;

        float width = gridRect.width * unitX;
        float height = gridRect.height * unitZ;

        Vector3 center = new Vector3(x + width / 2, 0, z + height / 2); //the base height is already included in the grid origin
        return center + gridOrigin;
    }

    /// <summary>
    /// Returns if the specified rectangular area of grid cells is completely free.
    /// </summary>
    /// <param name="gridRect"></param>
    /// <returns></returns>
    private bool IsFreeSpace(Rect gridRect)
    {
        try
        {
            //scan the given area for "true" cells
            for (int x = (int)gridRect.x; x < gridRect.x + gridRect.width; x++) //go from minimum x(inc) to maximum x(exc)
            {
                for (int y = (int)gridRect.y; y < gridRect.y + gridRect.height; y++) //go from minimum y(inc) to maximum y(exc)
                {
                    if (grid[x, y])
                    {
                        return false;
                    }
                }
            }
            return true; //no "true" cells found

        }
        catch (System.IndexOutOfRangeException)
        {
            return false; //if the given area sticks out of the inventory grid, return false
        }
    }

    /// <summary>
    /// Returns an array of snap positions sufficient for the given footprint (units).
    /// </summary>
    /// <returns></returns>
    protected SnapPosition[] FreePositions(Rect footprint)
    {
        Rect gridFootprint = GridFootprint(footprint);
        /*Debug.Log("Grid footprint: " + gridFootprint);*/
        List<SnapPosition> snapPositions = new List<SnapPosition>();

        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                gridFootprint.x = x;
                gridFootprint.y = z;
                if (IsFreeSpace(gridFootprint))
                {
                    //if the given block of grtid cells is free, add it to the list of possible snap locations
                    SnapPosition snapPosition = new SnapPosition(gridFootprint, this);
                    snapPositions.Add(snapPosition);
                }
            }
        }
        return snapPositions.ToArray();
    }
    #endregion
    #region Utility (snap)
    /// <summary>
    /// Marks an area as occupied in the inventory grid
    /// </summary>
    public void ApplySnapPosition(Rect gridRect)
    {
        for (int x = (int)gridRect.x; x < gridRect.x + gridRect.width; x++) //mark all ccells inside this rect
        {
            for (int z = (int)gridRect.y; z < gridRect.y + gridRect.height; z++)
            {
                grid[x, z] = true; //true means occupied
            }
        }
    }

    /// <summary>
    /// Marks an area as free in the inventory grid
    /// </summary>
    public void FreeSnapPosition(Rect gridRect)
    {
        for (int x = (int)gridRect.x; x < gridRect.x + gridRect.width; x++) //mark all ccells inside this rect
        {
            for (int z = (int)gridRect.y; z < gridRect.y + gridRect.height; z++)
            {
                grid[x, z] = false; //false means free
            }
        }
    }

    /// <summary>
    /// Returns the closest snapping point to the given position.
    /// </summary>
    /// <returns></returns>
    protected SnapPosition ClosestSnappingPoint(Vector3 pos)
    {
        int index = 0;
        float minDistance = 10;

        for (int i = 0; i < snapPositions.Length; i++)
        {
            float distance = Vector3.Distance(snapPositions[i].position, pos);

            if ((distance = Vector3.Distance(snapPositions[i].position, pos)) < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }
        return snapPositions[index];
    }

    /// <summary>
    /// Returns the closest snapping point to the given position, suitable for the given footprint (units).
    /// </summary>
    /// <returns></returns>
    public SnapPosition ClosestSnappingPoint(Vector3 pos, Rect footprint)
    {
        int index = 0;
        float minDistance = 10;
        SnapPosition[] snapPositions = FreePositions(footprint);

        for (int i = 0; i < snapPositions.Length; i++)
        {
            float distance = Vector3.Distance(snapPositions[i].position, pos);

            if ((distance = Vector3.Distance(snapPositions[i].position, pos)) < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }
        return snapPositions[index];
    }
    #endregion

    #region Snapping
    //corutines
    #region #1
    /// <summary>
    /// Controls the behaviour of a holdable object while it's being placed in the inventory.
    /// </summary>
    /// <param name="holdable"></param>
    /// <returns></returns>
    protected IEnumerator Manage(Holdable holdable)
    {
        SnapPosition snappingPoint = null;

        while (holdable.target != null) //manage the object until it's target doesn't "pull" it out of the operation area
        {
            //additional while conditions
            if ((Vector3.Distance(holdable.transform.position, holdable.target.position) > escapeDistance) &
                (Vector3.Angle(Vector3.up, holdable.target.position - holdable.transform.position) < 100f))
            {
                break;
            }


            SnapPosition newSnappingPoint = ClosestSnappingPoint(Quaternion.Inverse(transform.rotation) * (holdable.target.position - transform.position)); //the target's position relative to this
            if (snappingPoint != newSnappingPoint) //if there is a closest snapping point than the current one
            {
                Vector3 target = newSnappingPoint.position + transform.up * holdable.FromGround();
                holdable.snapPosition = newSnappingPoint; //save the snapping point in the holdable object
                snappingPoint = newSnappingPoint;
                StartCoroutine(LerpSnap(holdable, target));
            }
            yield return null;
        }

        //execute escape procedure
        StartCoroutine(LerpEscape(holdable)); //the LerpEscape method calls the Release method when it finishes
    }

    /// <summary>
    /// Lerps an object to the target location in a set amount of time. (uses l ocal space)
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    protected IEnumerator LerpSnap(Holdable holdable, Vector3 target)
    {
        float elapsed = 0f;
        float t = 0;
        Vector3 startPosition = holdable.transform.localPosition;

        while (elapsed <= snapTime) //time
        {
            elapsed += Time.deltaTime;
            t = elapsed / snapTime;
            holdable.transform.localPosition = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }
        holdable.transform.localPosition = target;
    }

    /// <summary>
    /// Lerps a holdable object back into it's target position. (uses local space)
    /// </summary>
    /// <param name="holdable"></param>
    /// <returns></returns>
    protected IEnumerator LerpEscape(Holdable holdable)
    {
        if (holdable.target != null)
        {
            float elapsed = 0f;
            float t = 0;
            Vector3 startPosition = holdable.transform.localPosition;
            Vector3 target = Quaternion.Inverse(transform.localRotation) * (holdable.target.position - this.transform.position); //convert to local space

            while (elapsed <= snapTime) //time
            {
                target = Quaternion.Inverse(transform.localRotation) * (holdable.target.position - this.transform.position); //convert to local space
                elapsed += Time.deltaTime;
                t = elapsed / snapTime;
                holdable.transform.localPosition = Vector3.Lerp(startPosition, target, t);
                yield return null;
            }
            holdable.transform.localPosition = target;

            //finish escape procedure
            Release(holdable);
        }
    }
    #endregion
    //other
    #region #2
    /// <summary>
    /// Parents this object to the inventory and disables its physics.
    /// </summary>
    /// <param name="holdable"></param>
    public void Capture(Holdable holdable)
    {
        snappingObject = holdable; //set up reference (is this needed?)
        holdable.isSnapping = true;
        holdable.isControlled = true;

        holdable.transform.parent = this.transform;

        float random = Random.Range(-2.5f, 2.5f);
        holdable.transform.localRotation = Quaternion.Euler(0, random, 0); //sets the object's rotation to vertical
        //i can not really change the rotation because the mesh used to calculate the footprint wouldn't follow it, creating false footprints
        //the small randomness isn't interfeering with the rotation enough to cause problems in most instances but still elevates the visulas

        holdable.Linked += OnLinked; //set up connections trough events
        holdable.Delinked += OnDelinked;
    }

    /// <summary>
    /// Reenables physics and breaks any connection to the inventory.
    /// </summary>
    /// <param name="holdable"></param>
    public void Release(Holdable holdable)
    {
        snappingObject = null; //breaking reference
        holdable.isSnapping = false;
        holdable.isControlled = false;

        holdable.transform.parent = null;

        holdable.Linked -= OnLinked; //breaking reference trough events
        holdable.Delinked -= OnDelinked;

        HideSnappingPoints();
    }

    /// <summary>
    /// Stops the managing of this object by the inventory, leaving it locked in place.
    /// </summary>
    /// <param name="holdable"></param>
    public virtual void Lock(Holdable holdable)
    {
        holdable.snapPosition.Apply(); //mark the occupied area

        holdable.gameObject.layer = 11;

        contents.Add(holdable);

        HideSnappingPoints();
    }

    /// <summary>
    /// Separates this object from the inventory.
    /// </summary>
    /// <param name="holdable"></param>
    protected virtual void Unlock(Holdable holdable)
    {
        holdable.snapPosition.Free();

        holdable.gameObject.layer = 0;

        contents.Remove(holdable);
    }
    #endregion
    #endregion

    #region Indication
    /// <summary>
    /// Spawn visual indicators for the current snapping points.
    /// </summary>
    protected void ShowSnappingPoints()
    {
        for (int i = 0; i < snapPositions.Length; i++)
        {
            snapPositions[i].Spawn();
        }
    }

    /// <summary>
    /// Destros the spawned visual indicators of the current snapping points.
    /// </summary>
    private void HideSnappingPoints()
    {
        try
        {
            for (int i = 0; i < snapPositions.Length; i++)
            {
                snapPositions[i].Destroy();
            }
        }
        catch (System.NullReferenceException)
        {
        }
    }
    #endregion

}
