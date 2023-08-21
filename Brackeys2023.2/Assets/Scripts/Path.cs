using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A path made of Vector3 points
/// </summary>
public class Path
{
    private Vector3[] points;
    /// <summary>
    /// Each points' distance from the next one.
    /// </summary>
    private float[] segmentLengths;
    /// <summary>
    /// The total length of the path.
    /// </summary>
    private float totalLength = 0;
    public float TotalLength
    {
        get { return totalLength; }
    }

    public Path(Vector3[] points)
    {
        this.points = points;

        segmentLengths = new float[points.Length - 1];

        for (int i = 0; i < points.Length - 1; i++)
        {
            segmentLengths[i] = Vector3.Distance(points[i], points[i + 1]);
            totalLength += segmentLengths[i];
        }
    }

    /// <summary>
    /// Returns a point at the given distance from the start of the path.
    /// </summary>
    public Vector3 PointAtDistance(float distance)
    {
        float coveredDistance = 0;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            //The point is between two path points
            if (coveredDistance + segmentLengths[i] >= distance)
            {
                float t = (distance - coveredDistance) / segmentLengths[i];
                Vector3 destination = Vector3.Lerp(points[i], points[i + 1], t);
                return destination;
            }
            coveredDistance += segmentLengths[i];
        }
        //the distance is greater than the length of the path
        return points[points.Length - 1];
    }
}
