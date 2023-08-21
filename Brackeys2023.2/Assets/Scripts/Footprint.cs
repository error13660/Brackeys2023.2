using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Providing an easy solution for arranging Storables on a 2d plane.
/// </summary>
/// <remarks>
/// Associates a Rect with a gameObject and provides function for placing,
/// rotating and tracking side lengths
/// </remarks>
public class Footprint
{
    public enum Axis
    {
        X, Y, Z
    }

    public enum Side
    {
        NORTH, EAST, SOUTH, WEST
    }

    private GameObject gameObject;
    public Rect rect;
    private float fromGround;

    public Footprint(Holdable holdable)
    {
        this.gameObject = holdable.gameObject;
        //quite resource intensive calculations
        fromGround = holdable.FromGround();
        this.rect = holdable.Footprint();
    }

    public Footprint(Rect rect)
    {
        this.rect = rect;
    }



    /// <remarks>
    /// Rotates this footprint such a way that the given edge
    /// is interfacing with a side that's shorter than it, but is closer to it's lengt
    /// than the other side.
    /// </remarks>
    /// <param name="edge">The edge to fit this footprint to</param>
    public void DominoRotate(Vector2 edge)
    {
        Axis edgeOrientation;
        float edgeLength;

        //determine the edge orientation to fit the rect to
        //and get edge length
        if (edge.x == 0)
        {
            edgeOrientation = Axis.Z;
            edgeLength = edge.y;
        }
        else
        {
            edgeOrientation = Axis.X;
            edgeLength = edge.x;
        }

        //main algorithm
        if (rect.width <= edgeLength && rect.height <= edgeLength)
        {
            //get larger side rotated parallel to edge
            //(width > height) XNOR (edge == X)
            if (!((rect.width >= rect.height) == (edgeOrientation == Axis.X)))
            {
                //rotate 90
                gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                //swap width and height
                rect.width += rect.height;
                rect.height = rect.width - rect.height;
                rect.width -= rect.height;
            }
        }
        else
        {
            //get larger side rotated parallel to edge
            //(width < height) XNOR (edge == X)
            if (!((rect.width <= rect.height) == (edgeOrientation == Axis.X)))
            {
                //rotate 90
                gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                //swap width and height
                rect.width += rect.height;
                rect.height = rect.width - rect.height;
                rect.width -= rect.height;
            }
        }
    }

    /// <summary>
    /// Returns the longer side of the footprint rect.
    /// </summary>
    public float GetLongerSide()
    {
        return Mathf.Max(rect.width, rect.height);
    }

    /// <summary>
    /// Sets the position of the enclosed gameObject.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        Vector3 transformPosition = new Vector3(position.x, fromGround, position.y);

        if (gameObject != null)
        {
            gameObject.transform.position = transformPosition;
        }

        rect.x = position.x;
        rect.y = position.y;
    }

    /// <summary>
    /// Returns the area of the footprint rect.
    /// </summary>
    public float Area()
    {
        return rect.width * rect.height;
    }

    /// <summary>
    /// Returns the edge on the given side.
    /// </summary>
    public Vector2 GetEdge(Side side)
    {
        if (side == Side.NORTH || side == Side.SOUTH) //horizontal
        {
            return new Vector2(rect.width, 0);
        }
        else //vertical
        {
            return new Vector2(0, rect.height);
        }
    }

    public float GetEdgeLength(Side side)
    {
        return GetEdge(side).magnitude;
    }

    /// <summary>
    /// Aligns this Footprint to the left side of a chosen edge of another footprint.
    /// </summary>
    /// <remarks>
    /// Takes care of domino rotation of the alignee.
    /// </remarks>
    /// <param name="datum">The original footprint to align to</param>
    /// <param name="side">The side to place on</param>
    /// <param name="padding">The space to leave between footprints</param>
    public void AlignLeftOn(Footprint datum, Side side, float padding)
    {
        //get the edge to align to
        Vector2 edge = datum.GetEdge(side);

        //dominorotate
        this.DominoRotate(edge);

        //the distance perpendicular to the edge
        float normalOffset = SignedNormalOffset(datum, this, side, padding);
        //the distance between the centers projected to the edge
        float centerOffset = SignedCenterOffset(edge, this.GetEdge(side), side);
        Vector2 offset = new Vector2(); //the offset vector

        if (edge.x == 0) //the edge is vertical
        {
            offset.x = normalOffset;
            offset.y = centerOffset;
        }
        else
        {
            offset.x = centerOffset;
            offset.y = normalOffset;
        }

        this.SetPosition(datum.rect.position + offset);
    }

    public void AlignCenterOn(Footprint datum, Side side, float padding)
    {
        //get the edge to align to
        Vector2 edge = datum.GetEdge(side);

        //dominorotate
        this.DominoRotate(edge);

        //the distance perpendicular to the edge
        float normalOffset = SignedNormalOffset(datum, this, side, padding);
        Vector2 offset = new Vector2(); //the offset vector

        if (edge.x == 0) //the edge is vertical
        {
            offset.x = normalOffset;
            offset.y = 0;
        }
        else
        {
            offset.x = 0;
            offset.y = normalOffset;
        }

        this.SetPosition(datum.rect.position + offset);
    }

    /// <summary>
    /// Returns the signed distance between the centers of f1 and f2. (perpendicular to edge)
    /// </summary>
    /// <param name="f1">Footprint one</param>
    /// <param name="f2">Footprint two</param>
    /// <param name="side">The side of f1 f2 is placed on</param>
    /// <param name="padding">The padding left between the two</param>
    /// <returns>Signed float distance</returns>
    public static float SignedNormalOffset(Footprint f1, Footprint f2, Side side, float padding)
    {
        //unsigned distances
        float xDistance = (f1.rect.width / 2) + (f2.rect.width / 2) + padding; //side lengths /2 +padding
        float yDistance = (f1.rect.height / 2) + (f2.rect.height / 2) + padding;

        switch (side)
        {
            case Side.NORTH:
                return yDistance;
            case Side.EAST:
                return xDistance;
            case Side.SOUTH:
                return yDistance * -1f;
            case Side.WEST:
                return xDistance * -1f;
        }
        return -1f; //never
    }

    /// <summary>
    /// Offset along edge.
    /// </summary>
    public static float SignedCenterOffset(Vector2 edge1, Vector2 edge2, Side side)
    {
        float length1; //edge length 1
        float length2; //edge length 2
        float offset; //unsigned

        //get edge lengths as float
        if (edge1.x == 0)
        {
            length1 = edge1.y;
        }
        else
        {
            length1 = edge1.x;
        }

        if (edge2.x == 0)
        {
            length2 = edge2.y;
        }
        else
        {
            length2 = edge2.x;
        }

        offset = length1 - length2;

        //determine sign
        switch (side)
        {
            case Side.NORTH:
                return offset * -1f;
            case Side.EAST:
                return offset;
            case Side.SOUTH:
                return offset;
            case Side.WEST:
                return offset * -1f;
        }
        return -1f;
    }

    /// <summary>
    /// Returns the side corresponding to the given index.
    /// </summary>
    /// <remarks>
    /// NORTH - 0 |
    /// EAST - 1 ...
    /// </remarks>
    public static Side ToSide(int index)
    {
        switch (index)
        {
            case 0:
                return Side.NORTH;
            case 1:
                return Side.EAST;
            case 2:
                return Side.SOUTH;
            case 3:
                return Side.WEST;

            default:
                return Side.NORTH;
        }
    }
}
