using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHelper : MonoBehaviour
{
    [SerializeField] private string meshObjectName = "boundingBox";
    [SerializeField] private Mesh mesh;
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Vector3[] transformedVertices;
    public Vector3[] TransformedVertices
    {
        get
        {
            if (transformedVertices == null)
            {
                return GetTransformedVertices();
            }
            else
            {
                return transformedVertices;
            }
        }
    }

    //basic precalculated information about the mesh
    private Vector3 extents;
    public Vector3 Extents { get { return extents; } }
    private Vector2 footprint;
    public Vector2 Footprint { get { return footprint; } }
    private float fromGround;
    public float FromGround { get { return fromGround; } }
    private float height;
    public float Height { get { return height; } }

    //Minimum and maximum values for transformed vertex coordinates
    private float minX;
    private float minY;
    private float minZ;

    private float maxX;
    private float maxY;
    private float maxZ;

    private void Awake()
    {
        //FindMesh();
        //Bake();
    }

    private Vector3[] GetTransformedVertices()
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] transformed = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = (meshTransform.localRotation * vertices[i]); //rotation
            vertex.x *= meshTransform.localScale.x; //scale
            vertex.y *= meshTransform.localScale.y;
            vertex.z *= meshTransform.localScale.z;
            vertex += meshTransform.localPosition; //position

            transformed[i] = vertex;
        }
        return transformed;
    }

    /// <summary>
    /// Searches for a GameObject with a MeshFilter attached and assigns it's mesh as the mesh used by this object.
    /// </summary>
    public void FindMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.name == meshObjectName)
            {
                mesh = meshFilters[i].sharedMesh;
                meshTransform = meshFilters[i].transform;
                return;
            }
        }
        Debug.LogError("Mesh not found!");
    }

    /// <summary>
    /// Bakes the transformed vertices.
    /// </summary>
    public void Bake()
    {
        if (mesh == null || meshTransform == null)
        {
            Debug.LogError("Mesh or transform not set!");
            return;
        }
        transformedVertices = GetTransformedVertices();

        ExtractMeshCaracteristics(transformedVertices);
    }

    /// <summary>
    /// Returns a triangle on the mesh that faces the player.
    /// </summary>
    /// <returns>The 3 verticies of the tri</returns>
    public Vector3[] TriFacingPlayer()
    {
        float minAngle = 180;
        int index = 0;

        Vector3 playerDirection = Camera.main.transform.position - transform.position;
        playerDirection = Quaternion.Inverse(transform.rotation) * playerDirection;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            //the vector pointing at the triangle's center point from the object's origin
            Vector3 faceDirection = (
                transformedVertices[mesh.triangles[i + 0]]
                + transformedVertices[mesh.triangles[i + 1]]
                + transformedVertices[mesh.triangles[i + 2]]) / 3;
            float angle = Vector3.Angle(playerDirection, faceDirection);
            if (angle < minAngle)
            {
                minAngle = angle;
                index = i;
            }
        }

        //the triangle in the position with the smallest deviation from the direction of the player
        Vector3[] tri = {
            transformedVertices[mesh.triangles[index + 0]],
            transformedVertices[mesh.triangles[index + 1]],
            transformedVertices[mesh.triangles[index + 2]] };
        return tri;
    }

    /// <summary>
    /// Returns the 3 neighbouring triangles of the original triangle.
    /// </summary>
    /// <param name="tri">array with a length of 3</param>
    /// <returns>An array containing the verticies of the three neighbouring tri-s</returns>
    public Vector3[] AdjacentTris(Vector3[] tri)
    {
        List<Vector3[]> tris = new List<Vector3[]>();
        Vector3[] verticies = new Vector3[9]; //the 9 verticies of the 3 triangles

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int matches = 0;
            for (int j = 0; j < tri.Length; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (tri[j] == transformedVertices[mesh.triangles[i + k]]) //if this triangle has a matching vertex with the original tri
                    {
                        matches++;
                        break;
                    }
                }
            }
            if (matches == 2) //if there are exactly 2 matches the tri is adjacent
            {
                tris.Add(new Vector3[] {
                    transformedVertices[mesh.triangles[i + 0]],
                    transformedVertices[mesh.triangles[i + 1]],
                    transformedVertices[mesh.triangles[i + 2]] });
                if (tris.Count == 3)
                {
                    break;
                }
            }
        }
        //create vertex array
        for (int i = 0; i < tris.Count; i++)
        {
            verticies[3 * i + 0] = tris[i][0];
            verticies[3 * i + 1] = tris[i][1];
            verticies[3 * i + 2] = tris[i][2];
        }
        return verticies;
    }

    /// <summary>
    /// Sets the min and max values of this object according to the given vertices.
    /// </summary>
    /// <param name="vertices">The vertices to get the extremes from</param>
    private void ExtractMeshCaracteristics(Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            //min
            if (vertex.x < minX) { minX = vertex.x; }
            if (vertex.y < minY) { minY = vertex.y; }
            if (vertex.z < minZ) { minZ = vertex.z; }

            //max
            if (vertex.x > maxX) { maxX = vertex.x; }
            if (vertex.y > maxY) { maxY = vertex.y; }
            if (vertex.z > maxZ) { maxZ = vertex.z; }
        }

        extents = new Vector3(
            Mathf.Abs(maxX - minX),
            Mathf.Abs(maxY - minY),
            Mathf.Abs(maxZ - minZ));

        footprint = new Vector2(
            Mathf.Abs(maxX - minX),
            Mathf.Abs(maxZ - minZ));

        fromGround = minY;

        height = Mathf.Abs(maxY - minY);
    }
}
