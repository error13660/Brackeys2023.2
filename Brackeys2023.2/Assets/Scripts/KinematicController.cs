using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class KinematicController : MonoBehaviour
{
    private float radius;
    private float height;
    [SerializeField] private float velocity = 0.5f;
    [SerializeField] private float maxClimbAngle = 45;
    private Vector3 inputInFixedUpdate = Vector3.zero;
    private Vector3 nextPosition;
    private Vector3 lastPosition;
    private float lastFixedUpdate;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform head;
    [SerializeField] private float verticalLookMin;
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
        head.localRotation *= Quaternion.Euler(Mathf.Clamp(mouseY * mouseSensitivity, verticalLookMin, verticalLookMax) * -1,
            0, 0);
    }

    private void FixedUpdate()
    {
        Vector3 movementVector = inputInFixedUpdate * velocity * Time.fixedDeltaTime;
        Vector3 newPosition;

        GetMove(
            out newPosition,
            transform.position,
            movementVector,
            velocity * Time.deltaTime);

        lastPosition = nextPosition;
        nextPosition = newPosition;
        inputInFixedUpdate = Vector3.zero;
        lastFixedUpdate = Time.time;
    }

    /// <summary>
    /// Recursively calculates the new position of the player
    /// </summary>
    /// <param name="endPosition">The position where the palyer should be moved</param>
    /// <returns>True if the calculation reached it's end</returns>
    private bool GetMove(out Vector3 endPosition, Vector3 startPosition, Vector3 direction, float DistanceRemaining)
    {
        RaycastHit hit1;

        //raycast along direction
        bool collision = Physics.CapsuleCast(
            startPosition - Vector3.up * (height / 2),
            startPosition + Vector3.up * (height / 2),
            radius,
            direction,
            out hit1,
            DistanceRemaining
            );

        Vector3 point1;
        if (collision)
        {
            if (hit1.distance > 0.01f)
            {
                point1 = startPosition + direction * hit1.distance;
            }
            else
            {
                point1 = startPosition;
            }
            DistanceRemaining -= hit1.distance;
        }
        else
        {
            point1 = startPosition + direction * velocity /* Time.fixedDeltaTime*/;
            DistanceRemaining -= velocity/* * Time.fixedDeltaTime*/;
        }

        RaycastHit hit2;
        Vector3 point2 = point1;
        Debug.DrawRay(hit1.point, Vector3.up, Color.red);

        //gravity
        //gravity always happens (it might not do anything)
        if (Physics.CapsuleCast(
            point1 - Vector3.up * (height / 2),
            point1 + Vector3.up * (height / 2),
            radius,
            Vector3.down,
            out hit2
            ))
        {
            point2 = point1 + Vector3.down * hit2.distance;
        }

        if (collision) //if a wall was hit
        {
            endPosition = point2;
            return false;
        }

        if (DistanceRemaining <= 0) //if the distance was reached
        {
            endPosition = startPosition + direction * (velocity/*Time.fixedDeltaTime*/);
            return false;
        }

        endPosition = startPosition;
        return true;
    }
}
