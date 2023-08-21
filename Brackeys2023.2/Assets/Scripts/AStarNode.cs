using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    obstructed,
    /// <summary>
    /// Neighbouring an obstructed node
    /// </summary>
    edge,
    free,
    unknown
}

public class AStarNode
{
    /// <summary>
    /// The position of this node in 3d space
    /// </summary>
    public Vector3 position;
    /// <summary>
    /// The endpoint of this pathfinding
    /// </summary>
    public Vector3 end;
    /// <summary>
    /// The node this is expanded from.
    /// Used to get the shortest path when the end is reached
    /// </summary>
    public AStarNode ancestor;
    /// <summary>
    /// A list of connected nodes
    /// </summary>
    public List<AStarNode> connected = new List<AStarNode>();
    /// <summary>
    /// The obstruction status of this node
    /// </summary>
    private Status status = Status.unknown;
    /// <summary>
    /// Since this implementation is based on a grid, the weight
    /// is solely determined by the euclidian distance from the end point
    /// </summary>
    public float weight;
    /// <summary>
    /// The gap between two nodes in 3d space
    /// </summary>
    public float nodeGap;

    /// <summary>
    /// The pattern in which a node expands (aka the offset
    /// positions of adjacent nodes)
    /// </summary>
    private Vector3[] expansionPattern;

    ///<summary>Use when expanding an existing node network.</summary>
    /// <param name="position">The position of this node</param>
    /// <param name="ancestor">The node this is expanded from</param>
    public AStarNode(Vector3 position, AStarNode ancestor)
    {
        this.position = position;
        weight = Vector3.Distance(position, ancestor.end); //calculate weight
        ancestor.connected.Add(this); //link to ancestor
        this.connected.Add(ancestor);

        this.nodeGap = ancestor.nodeGap;
        this.expansionPattern = ancestor.expansionPattern;
        this.end = ancestor.end;
        this.ancestor = ancestor;
    }

    ///<summary>Use when creating a new node network.</summary>
    /// <param name="position">The position of this node</param>
    /// <param name="end">The endpoint of this pathfinding</param>
    /// <param name="ancestor">The node this is expanded from</param>
    public AStarNode(Vector3 position, Vector3 end, float nodeGap)
    {
        this.position = position;
        weight = Vector3.Distance(position, end); //calculate weight
        this.nodeGap = nodeGap;
        this.end = end;
        this.ancestor = null;

        this.expansionPattern = new Vector3[]
        {
            new Vector3(1,0,0),
            new Vector3(-1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,-1,0),
            new Vector3(0,0,1),
            new Vector3(0,0,-1)
        };
    }

    /// <summary>
    /// Returns the node with the given position. If there is no such node
    /// within the specified distance, this function returns null.
    /// </summary>
    /// <param name="position">The position of the wanted node</param>
    /// <param name="distance">The maximum search distance in nodes</param>
    /// <param name="last">The last node examined</param>
    public AStarNode FindWithPosition(Vector3 position, int distance, AStarNode last)
    {
        if (this.position == position) //if this node has a matching position
        {
            return this;
        }

        if (distance == 0) //if the search distance is reached
        {
            return null;
        }

        for (int i = 0; i < connected.Count; i++) //search connected
        {
            AStarNode node;
            //only examine if not previously* examined (cut down on unnecessary, duplicate examination)
            if ((connected[i] != this) && (node = connected[i].FindWithPosition(position, distance - 1, this)) != null)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Expands this node creating new nodes or linking existing ones.
    /// </summary>
    /// <param name="created">The nodes created during this expansion</param>
    /// <returns>The created or linked nodes</returns>
    public AStarNode[] Expand(Vector3 halfExtents, Quaternion rotation, out AStarNode[] expandables)
    {
        AStarNode[] adjacent = new AStarNode[6];
        List<AStarNode> created = new List<AStarNode>(); //only created nodes have to be expanded further

        for (int i = 0; i < adjacent.Length; i++)
        {
            Vector3 adjacentPosition = this.position + expansionPattern[i] * nodeGap;
            AStarNode node = FindWithPosition(adjacentPosition, 3, this);

            if (node == null) //node doesn't already exist
            {
                node = new AStarNode(adjacentPosition, this);
                created.Add(node);
            }
            adjacent[i] = node;
        }

        //link nodes
        for (int i = 0; i < adjacent.Length; i++)
        {
            //link this to adjacent
            if (!connected.Contains(adjacent[i]))
            {
                connected.Add(adjacent[i]);
                adjacent[i].connected.Add(this);
            }
        }


        //remove obstructed nodes from 'created' to get expandables
        for (int i = created.Count - 1; i >= 0; i--)
        {
            if (created[i].GetStatus(halfExtents, rotation) == Status.obstructed)
            {
                created.RemoveAt(i);
            }
        }
        expandables = created.ToArray();
        return adjacent;
    }

    /// <summary>
    /// Returns the obstruction status of this node.
    /// </summary>
    /// <param name="halfExtents">Describes the box used to evaluate the obstruction status</param>
    /// <param name="rotation">The Rotation of the box used to evaluate the obstruction status</param>
    /// <returns>Obstructed/Edge/Free</returns>
    public Status GetStatus(Vector3 halfExtents, Quaternion rotation)
    {
        Collider[] colliders;
        int overlaps = 0;

        //if the status is already known
        if (status != Status.unknown)
        {
            return status;
        }

        colliders = Physics.OverlapBox(position, halfExtents, rotation, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.layer != 15) //layer 15 is excluded
            { //layer 15 is self
                overlaps++;
            }
        }

        //if colliders are obstructing
        if (overlaps > 0)
        {
            status = Status.obstructed;
            return Status.obstructed;
        }
        status = Status.free;
        return Status.free;
    }

    /// <summary>
    /// Returns true if this is the possible closest position to the end point.
    /// </summary>
    /// <returns></returns>
    public bool IsEnd()
    {
        if (Vector3.Distance(end, position) < nodeGap) //sqrt(2)/2
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if this node neighbours an obstructed one.
    /// </summary>
    public bool IsEdge()
    {
        //if it's the starting point -> don't remove
        if (ancestor == null)
        {
            return true;
        }

        //if conneted to an obstructed node -don't remove
        for (int i = 0; i < connected.Count; i++)
        {
            if (connected[i].status == Status.obstructed)
            {
                status = Status.edge;
                return true;
            }
        }
        return false;
    }
}
