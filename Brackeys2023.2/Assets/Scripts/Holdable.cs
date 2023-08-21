using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshHelper))]
public class Holdable : MonoBehaviour
{
    //general status flags
    [HideInInspector] public bool isHeld; //true if the object is held by the player
    [HideInInspector] public bool isControlled; //true if the object is controlled in a way that is preventing the normal placement method

    //tetris inventory
    [HideInInspector] public bool isSnapping; //true is the object is currently choosing a snapping place on a tetris inventory
    [HideInInspector] public SnapPosition snapPosition;

    //essential references
    [HideInInspector] public Transform target;
    private MeshHelper meshHelper;

    //actions
    public Action<Holdable> Linked = OnLinked;
    public Action<Holdable> Delinked = OnDelinked;

    private void Awake()
    {
        meshHelper = GetComponent<MeshHelper>();
    }

    public void LinkTo(Transform target)
    {
        this.target = target;
        isHeld = true;

        Linked.Invoke(this);
    }

    public void Delink()
    {
        target = null;
        isHeld = false;
        Delinked.Invoke(this);
    }

    #region Placing
    /// <summary>
    /// Returns true if the properly rotated rectangular area below this object is clear.
    /// </summary>
    /// <returns></returns>
    public bool FootprintClear()
    {
        Quaternion yaw = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        Rect footprint = Footprint();
        float h;
        int staticSurface = 1 << 13; //bitwise shift operator (x13)
        //static surface layermask
        int ignoreInFootprint = 1 << 15; //ignore layermask
        //ignore in footprint and exclude in pathfind are now the same

        Ray ray = new Ray(transform.position, new Vector3(0, -1, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10/*max distance*/, staticSurface/*layer mask*/)) //raycast to determine the height above the closest static surface (layer 13) (floors, shelves etc.)
        {
            h = hit.distance;
        }
        else
        {
            return false;
        }

        Vector3 halfExtents = new Vector3(footprint.width / 2, h / 2, footprint.height / 2);
        Vector3 center = transform.position + Vector3.up * (h / -2); //half height below this object
        Collider[] inWay = Physics.OverlapBox(center, halfExtents, yaw, ~ignoreInFootprint, QueryTriggerInteraction.Ignore); //the rectangle below this object considering its yaw

        //exclude this object and static surfaces
        int overlaps = 0;

        for (int i = 0; i < inWay.Length; i++)
        {
            if (inWay[i].gameObject != this.gameObject & inWay[i].gameObject.layer != 13 & inWay[i].gameObject.layer != 14) //not this object and not a static surface or an ignored object
            {
                overlaps++;
            }
        }

        //Debug.Log("Objects preventing placement: " + overlaps);

        if (overlaps == 0) //overlaps: the number of overlaps found
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Tetris Inventory
    /// <summary>
    /// Returns a rect that this object can stand in perfeclty.
    /// Note: The position values of the rect are not set.
    /// </summary>
    /// <returns></returns>
    public Rect Footprint()
    {
        Vector3[] vertices = meshHelper.TransformedVertices;
        float minX = 0;
        float maxX = 0;
        float minZ = 0;
        float maxZ = 0;

        for (int i = 0; i < vertices.Length; i++) //get the extremes of the mesh
        {
            if (vertices[i].x < minX)
            {
                minX = vertices[i].x;
            }

            if (vertices[i].x > maxX)
            {
                maxX = vertices[i].x;
            }

            if (vertices[i].z < minZ) //that was z, i changed it to debug
            {
                minZ = vertices[i].z;
            }

            if (vertices[i].z > maxZ)
            {
                maxZ = vertices[i].z;
            }
        }
        float width = maxX - minX;
        float height = maxZ - minZ;
        Rect footprint = new Rect(0, 0, width, height);
        return footprint;
    }
    #endregion

    #region Mesh Utility
    /// <summary>
    /// Returns the height of the mesh collider connected to this game object.
    /// </summary>
    /// <returns></returns>
    public float BodyHeight()
    {
        Vector3[] vertices = meshHelper.TransformedVertices;
        float maxY = 0;
        float minY = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < minY)
            {
                minY = vertices[i].y;
            }

            if (vertices[i].y > maxY)
            {
                maxY = vertices[i].y;
            }
        }
        float height = maxY - minY;
        return Mathf.Abs(height);
    }

    /// <summary>
    /// Returns the height difference between the lowest point of the object and the origin of the object.
    /// </summary>
    /// <returns></returns>
    public float FromGround()
    {
        Vector3[] vertices = meshHelper.TransformedVertices;
        float minY = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < minY)
            {
                minY = vertices[i].y;
            }
        }
        return Math.Abs(minY);
    }
    #endregion

    #region Maths Utility
    private Vector3 sqrtVector(Vector3 vector)
    {

        if (vector.x >= 0)
        {
            vector.x = (float)Math.Sqrt(vector.x);
        }
        else
        {
            vector.x *= -1f;
            vector.x = (float)Math.Sqrt(vector.x);
            vector.x *= -1f;
        }

        if (vector.y >= 0)
        {
            vector.y = (float)Math.Sqrt(vector.y);
        }
        else
        {
            vector.y *= -1f;
            vector.y = (float)Math.Sqrt(vector.y);
            vector.y *= -1f;
        }

        if (vector.z >= 0)
        {
            vector.z = (float)Math.Sqrt(vector.z);
        }
        else
        {
            vector.z *= -1f;
            vector.z = (float)Math.Sqrt(vector.z);
            vector.z *= -1f;
        }

        return vector;
    }
    #endregion

    #region Events
    private static void OnLinked(Holdable holdable)
    {

    }
    private static void OnDelinked(Holdable holdable)
    {

    }
    #endregion
}
