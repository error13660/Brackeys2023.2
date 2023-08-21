using UnityEngine;

public class Mirror : MonoBehaviour
{
    public RenderTexture reflectionTexture;
    public int visibleMirrorCount;

    private Camera reflectionCamera;

    private void Start()
    {
        // Create and configure a reflection camera
        GameObject reflectionCameraObject = new GameObject("ReflectionCamera");
        reflectionCamera = reflectionCameraObject.AddComponent<Camera>();
        reflectionCamera.targetTexture = reflectionTexture;
        reflectionCamera.cullingMask = LayerMask.GetMask("MirrorReflection"); // Set the appropriate reflection layer
    }

    private void Update()
    {
        // Count the number of visible mirrors in the reflection
        RaycastHit hit;
        visibleMirrorCount = 0;

        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, LayerMask.GetMask("MirrorReflection")))
        {
            Mirror reflectedMirror = hit.collider.GetComponent<Mirror>();
            if (reflectedMirror != null)
            {
                visibleMirrorCount++;
            }
        }
    }
}
