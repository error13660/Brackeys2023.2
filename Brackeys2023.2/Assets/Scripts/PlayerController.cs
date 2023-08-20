using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float forwardRunForce;
    [SerializeField] private float sideRunForce;
    [SerializeField] private float jumpForce;

    [SerializeField] private float idleDrag; //the force that stops the palyer when it is not actively moving
    [SerializeField] private float burst; //how much faster to go when starting to run
    [SerializeField] private float burstTime;
    [SerializeField] private float velocityMax;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float inputInLastSec; //the patio of input vs non input in the last sec

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform head;
    [SerializeField] private float verticalLookMin;
    [SerializeField] private float verticalLookMax;

    private Vector3 movementVectorCache;
    float xRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        if (inputX != 0 || inputY != 0)
        { //there is input
            inputInLastSec = Mathf.Clamp01(inputInLastSec + Time.deltaTime / burstTime);
        }
        else //there is no input
        {
            inputInLastSec = Mathf.Clamp01(inputInLastSec - Time.deltaTime / burstTime);
        }

        float burstMultiplyer = burst * (1 - inputInLastSec);

        Vector3 moveForce = new Vector3(inputX, 0, inputY).normalized * Time.deltaTime; //sideways, 0, forward
        moveForce.Scale(new Vector3(sideRunForce + sideRunForce * burstMultiplyer, 0,
            forwardRunForce + forwardRunForce * burstMultiplyer)); //run slower sideways

        float dragX = 0;
        float dragY = 0;
        if (inputX == 0) dragX = Vector3.Dot(rb.velocity, transform.forward) * -1 * Mathf.Lerp(idleDrag, 1, 1 - inputInLastSec); //not actively moving sideways
        if (inputY == 0) dragY = Vector3.Dot(rb.velocity, transform.right) * -1 * Mathf.Lerp(idleDrag, 1, 1 - inputInLastSec); //forward

        Vector3 idleDragForce = new Vector3(dragX, 0, dragY) * Time.deltaTime;

        movementVectorCache += (transform.rotation * moveForce);
        movementVectorCache += (transform.rotation * idleDragForce);

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
        rb.AddForce(movementVectorCache);
        movementVectorCache = Vector3.zero;
    }
}
