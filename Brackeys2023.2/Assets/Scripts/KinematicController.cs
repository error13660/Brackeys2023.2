using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class KinematicController : MonoBehaviour
{
    public static bool isPaused = false;

    private float radius;
    private float height;
    [SerializeField] private float velocity = 0.5f;
    [SerializeField] private float maxClimbAngle = 45;
    private Vector3 inputInFixedUpdate = Vector3.zero;
    private Vector3 nextPosition;
    private Vector3 lastPosition;
    private float lastFixedUpdate;

    private float skinWidth = 0.02f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform head;
    [SerializeField] private float verticalLookMax;

    void Start()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        radius = capsule.radius;
        height = capsule.height;
        nextPosition = transform.position;
        lastPosition = transform.position;
        lastFixedUpdate = Time.time;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (isPaused) return;

        //check for teleport and don't overrie it
        Vector3 delta = transform.position - lastPosition;
        if (delta.magnitude > 1) return;

        //get input
        Vector3 movementVector = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")).normalized;
        inputInFixedUpdate += (transform.rotation * movementVector * Time.deltaTime);

        //interpolate motion
        float t = (Time.time - lastFixedUpdate) / Time.fixedDeltaTime; //where between fixed updates
        transform.position = Vector3.Lerp(lastPosition, nextPosition, t);

        /*
         * Camera cntrol
         */
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");


        transform.localRotation *= Quaternion.Euler(0, mouseX * mouseSensitivity, 0);
        //head.localRotation *= Quaternion.Euler(mouseY * mouseSensitivity * -1, 0, 0);
        //Debug.Log(head.localRotation.eulerAngles.x);

        Quaternion newRotation = head.localRotation * Quaternion.Euler(mouseY * mouseSensitivity * -1, 0, 0);
        if (Quaternion.Angle(newRotation, Quaternion.LookRotation(Vector3.forward)) <= verticalLookMax)
        {
            //Debug.Log(Quaternion.Angle(newRotation, Quaternion.LookRotation(Vector3.forward)));
            head.localRotation = newRotation;
        }
    }

    private void FixedUpdate()
    {
        Vector3 movementVector = inputInFixedUpdate * velocity * Time.fixedDeltaTime;
        Vector3 newPosition;

        newPosition = transform.position + CollideAndSlide(movementVector, transform.position, 0, false);
        newPosition = newPosition + CollideAndSlide(Vector3.down, newPosition, 0, true);

        lastPosition = nextPosition;
        nextPosition = newPosition;
        inputInFixedUpdate = Vector3.zero;
        lastFixedUpdate = Time.time;
    }

    private Vector3 CollideAndSlide(Vector3 velocity, Vector3 position, int depth, bool isGravity)
    {
        if (depth >= 10) return Vector3.zero;

        float dist = velocity.magnitude + skinWidth;

        RaycastHit hit;
        if (Physics.SphereCast(
            position,
            radius,
            velocity.normalized,
            out hit,
            dist,
            ~0,
            QueryTriggerInteraction.Ignore
            ))
        {
            Vector3 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
            Vector3 leftover = velocity - snapToSurface;
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            if (snapToSurface.magnitude <= skinWidth) //discard small movements from inaccuracy
            {
                snapToSurface = Vector3.zero;
            }

            if (angle <= 40)
            {
                if (isGravity)
                {
                    return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, hit.normal);
            }
            //wall or steep slope
            else
            {
                float scale = 1 - Vector3.Dot(
                    new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                    -new Vector3(velocity.x, 0, velocity.z).normalized);
                leftover = ProjectAndScale(leftover, hit.normal) * scale;
            }

            return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1, isGravity);
        }
        return velocity;

        Vector3 ProjectAndScale(Vector3 leftover, Vector3 normal)
        {
            float mag = leftover.magnitude;
            leftover = Vector3.ProjectOnPlane(leftover, normal).normalized;
            leftover *= mag;
            return leftover;
        }
    }
}
