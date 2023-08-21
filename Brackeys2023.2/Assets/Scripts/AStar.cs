using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    /// <summary>
    /// The starting point of the pathfinding
    /// </summary>
    private Vector3 start;
    /// <summary>
    /// The end point of the pathfinding
    /// </summary>
    private Vector3 end;
    /// <summary>
    /// The maximum number of nodes to travel trough before
    /// the end is determined unreachable
    /// </summary>
    private int maxDistance;
    /// <summary>
    /// The gap between two nodes in 3d space
    /// </summary>
    private float nodeGap;

    //object bounding box properties
    private Vector3 halfExtents;
    private Quaternion rotation;

    /// <summary>
    /// If true, the pathfind will return a simplified path.
    /// </summary>
    private bool simplify = false;
    public bool Simplify
    {
        set { simplify = value; }
    }

    private AStarNode closest = null; //the closest node found to the target during the pathfinding process

    /// <summary>
    /// The ordewr in which to expoand the nodes
    /// </summary>
    private List<AStarNode> priorityQueue = new List<AStarNode>();

    /// <summary>
    /// Creates an instance of the A* pathfinding algorithm to find a route around
    /// obstacles in 3d space.
    /// </summary>
    /// <param name="start">The starting point</param>
    /// <param name="end">The end point to reach</param>
    /// <param name="maxDistance">The maximum number of nodes to travel trough before
    /// the end is determined unreachable</param>
    public AStar(Vector3 start, Vector3 end, int maxDistance, float nodeGap, Vector3 halfExtents, Quaternion rotation)
    {
        this.start = start;
        this.end = end;
        this.maxDistance = maxDistance;
        this.nodeGap = nodeGap;

        this.halfExtents = halfExtents;
        this.rotation = rotation;

        priorityQueue.Add(new AStarNode(start, end, nodeGap));
    }

    /// <summary>
    /// Adds a new node to the 'expandQueue' in the way
    /// as dictated by the A* algorythm.
    /// </summary>
    /// <param name="node"></param>
    private void AddToQueue(AStarNode node)
    {
        for (int i = 0; i < priorityQueue.Count; i++)
        {
            //node is first
            if (priorityQueue[0].weight >= node.weight)
            {
                priorityQueue.Insert(0, node);
                return;
            }

            //put the nod in an ordered list
            if (priorityQueue[i].weight >= node.weight && priorityQueue[i - 1].weight <= node.weight)
            {
                priorityQueue.Insert(i, node);
                return;
            }
        }
        //add at last place
        priorityQueue.Add(node);
    }

    /// <summary>
    /// Returns the fastest route with the given parameters.
    /// </summary>
    public AStarNode[] Pathfind(out bool isComplete)
    {
        AStarNode[] path = Pathfind(0, out isComplete);

        //path shouldn't be simplified
        if (!simplify)
        {
            return path;
        }

        if (path == null)
        {
            return null;
        }

        //simplify
        List<AStarNode> pathList = new List<AStarNode>();
        pathList.AddRange(path);
        return SimplifyRoute(pathList).ToArray();
    }

    /// <param name="isComplete">True if the route reached the target</param>
    private AStarNode[] Pathfind(int distance, out bool isComplete)
    {
        isComplete = false;

        //can't expand more
        if (priorityQueue.Count == 0)
        {
            return null;
        }

        //target is reached
        if (priorityQueue[0].IsEnd())
        {
            isComplete = true;
            return Trace(priorityQueue[0]);
        }

        //max distance limit rached
        if (distance >= maxDistance)
        {
            //return best aproximation
            return Trace(closest);
        }

        AStarNode[] expandables;
        AStarNode firstInQueue = priorityQueue[0];

        priorityQueue.RemoveAt(0);
        //expand queue
        firstInQueue.Expand(halfExtents, rotation, out expandables);

        for (int i = 0; i < expandables.Length; i++)
        {
            //add to priority queue
            AddToQueue(expandables[i]);

            //compare with known closest
            if (closest == null ||
                expandables[i].weight <= closest.weight)
            {
                closest = expandables[i];
            }
        }

        distance++;
        return Pathfind(distance, out isComplete); //recurse
    }

    /// <summary>
    /// Returns the ancestor chain of the given node.
    /// </summary>
    private AStarNode[] Trace(AStarNode node)
    {
        List<AStarNode> ancestors = new List<AStarNode>();
        AStarNode current = node;

        do
        {
            ancestors.Add(current);
            current = current.ancestor;
        } while (current != null);

        return ancestors.ToArray();
    }

    /// <summary>
    /// Removes unnecessary nodes in a route
    /// </summary>
    /// <param name="route">first element = last node in route</param>
    private List<AStarNode> SimplifyRoute(List<AStarNode> route)
    {
        for (int i = route.Count - 1; i >= 0; i--)
        {
            if (!route[i].IsEdge() && !route[i].IsEnd()) //not edge and not end
            {
                route.RemoveAt(i);
            }
        }

        for (int i = route.Count - 1; i >= 2; i--)
        {
            if (Vector3.Distance(route[i].position, route[i - 2].position) <= 1.5 * nodeGap)
            {
                route.RemoveAt(i - 1);
            }
        }

        return route;
    }

    /// <summary>
    /// Returns Vector3 points along the path.
    /// </summary>
    public static Vector3[] PathPoints(AStarNode[] path)
    {
        if (path == null)
        {
            return null;
        }

        Vector3[] points = new Vector3[path.Length];

        for (int i = path.Length - 1; i >= 0; i--)
        {
            points[i] = path[path.Length - (i + 1)].position;
        }
        return points;
    }
}
