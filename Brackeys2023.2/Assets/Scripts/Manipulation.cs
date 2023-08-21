using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manipulation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera camera;
    [SerializeField] private Transform target;

    [Header("Interaction")]
    [SerializeField] private float range; //the range in which interactions can be made with the enviroment
    [SerializeField] private float rotationInputStrength = 30f;

    [Header("Pathfinding parameters")]
    [SerializeField] private float pathfindingPrecision; //node gap
    [SerializeField] private int giveUpDistance;
    [SerializeField] private float subjectTimeToTarget = 0.1f; //the time it takes for the subject to travel to the end of its path

    //pathfinding
    private Holdable subject = null;
    private Path subjectPath;
    private float updateTime;
    //rotation
    private Vector3 targetLastForward; //rotation of the target at the path generation
    private Quaternion combinedStartRotation;
    private Quaternion combinedEndRotation; //the final calculated rotation of the subject at the next path generation
    private Quaternion rotationInput; //input by the player per path calculation

    //flags
    private bool isHolding = false; //the player is holding an object

    private void Awake()
    {
        targetLastForward = target.forward;
    }

    private void Update()
    {
        //perform primary action on keypress -- if the inputmanager agrees
        if (Input.GetKeyDown(KeyCode.E))
        {
            PrimaryAction();
        }

        //manipulate held object
        if (subject != null && !subject.isControlled)
        {
            rotationInput *= GetRotationInput();

            subject.transform.position = CalculateSubjectPosition(subjectTimeToTarget); //pos
            subject.transform.rotation = CalculateSubjectRotation(subjectTimeToTarget); //rot
        }
    }

    private void FixedUpdate()
    {
        if (subject != null)
        {
            int layer = subject.gameObject.layer;
            subject.gameObject.layer = 15;//exclude inj pathfind

            UpdateRotationValues(); //get rotation used in pathfinding and lerp

            subjectPath = GetSubjectPath();
            updateTime = Time.time;

            subject.gameObject.layer = layer;
        }
    }

    /// <summary>
    /// The player's primary action, by default bound to E
    /// </summary>
    private void PrimaryAction()
    {
        //if already holding an item
        if (isHolding)
        {
            Release(subject);
            return;
        }

        GameObject targeted = GetTargeted();

        //if there is no targeted object found
        if (targeted == null)
        {
            return;
        }

        //holdable object found
        Holdable holdable;
        if ((holdable = targeted.GetComponent<Holdable>()))
        {
            Link(holdable);
            return;
        }
    }

    private GameObject GetTargeted()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    #region Rotation

    /// <summary>
    /// Creates the used values for rotation in a pathfinding cycle
    /// </summary>
    private void UpdateRotationValues()
    {
        Vector3 targetCurrentForward = new Vector3(target.forward.x, 0, target.forward.z).normalized; //only keep euler y component of rotation
        Quaternion targetRotationChange = Quaternion.FromToRotation(targetLastForward, targetCurrentForward);

        targetLastForward = targetCurrentForward;

        combinedStartRotation = combinedEndRotation;

        combinedEndRotation = rotationInput * targetRotationChange * combinedStartRotation;
        rotationInput = Quaternion.identity;

        float d = 0.02f / subjectTimeToTarget * subjectPath.TotalLength;
        if (!IsRotationValid(subjectPath.PointAtDistance(d), combinedEndRotation))
        { //if rotation is invalid at that point
            combinedEndRotation = combinedStartRotation;
        }
    }

    private Quaternion CalculateSubjectRotation(float time)
    {
        float elapsedSinceUpdate = Time.time - updateTime;
        float t = elapsedSinceUpdate / time;

        return Quaternion.Lerp(combinedStartRotation, combinedEndRotation, t);
    }

    /// <summary>
    /// Collects manual rotation input from the player
    /// </summary>
    private Quaternion GetRotationInput()
    {
        //get input for manual rotation
        float input = Input.GetAxis("Mouse ScrollWheel");
        float inputStrength = rotationInputStrength;
        Quaternion rotation;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 direction = target.right;
            Debug.DrawLine(target.position, target.position + direction);
            //Quaternion.Inverse(Quaternion.Euler(0, subject.transform.rotation.eulerAngles.y, 0)) * Vector3.right;

            rotation = Quaternion.AngleAxis(input * inputStrength, direction);
        }
        else
        {
            Debug.DrawLine(target.position, target.position + Vector3.up);
            rotation = Quaternion.Euler(Vector3.up * input * inputStrength);
        }
        return rotation;
    }

    /// <summary>
    /// Returns true if the subject can be placed at the given position with the given rotation.
    /// </summary>
    private bool IsRotationValid(Vector3 position, Quaternion rotation)
    {
        Rect footprint = subject.Footprint();
        Vector3 halfExtents = new Vector3(footprint.width / 2, subject.BodyHeight() / 2, footprint.height / 2) * 1.1f;
        Collider[] colliders = Physics.OverlapBox(position, halfExtents, rotation, ~0, QueryTriggerInteraction.Ignore);
        int overlaps = 0;
        int layer = subject.gameObject.layer;
        subject.gameObject.layer = 15;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.layer != 15) //layer 15 is excluded
            { //layer 15 is self
                overlaps++;
                break;
            }
        }

        subject.gameObject.layer = layer;

        //if colliders are obstructing
        if (overlaps > 0)
        {
            return false;
        }
        return true;
    }

    #endregion
    #region Pathfinding
    /// <summary>
    /// Returns the subject's position on the precalculated path based on the speed and the elapsed time.
    /// <param name="time">The time to reach the target.</param>
    /// </summary>
    private Vector3 CalculateSubjectPosition(float time)
    {
        float elapsedSinceUpdate = Time.time - updateTime;
        float t = elapsedSinceUpdate / time;
        float distance = subjectPath.TotalLength * t;

        if (distance <= 0.01)
        {
            distance = 0;
        }

        return subjectPath.PointAtDistance(distance);
    }

    /// <summary>
    /// Calculates the path of the subject to the target.
    /// </summary>
    /// <returns></returns>
    private Path GetSubjectPath()
    {
        Vector3 start = subject.transform.position;
        Vector3 end = target.position;

        Rect footprint = subject.Footprint();
        Vector3 halfExtents = new Vector3(footprint.width / 2, subject.BodyHeight() / 2, footprint.height / 2);

        AStar astar = new AStar(start, end, giveUpDistance, pathfindingPrecision, halfExtents, combinedEndRotation);
        //using the subject's rotation at the end of its path ^^
        astar.Simplify = true;

        bool isComplete;
        //account for children in case of inventories
        int[] childLayers = new int[subject.transform.childCount];
        for (int i = 0; i < subject.transform.childCount; i++) //set layer of children to 15 (don't include in pathfind)
        {
            GameObject child = subject.transform.GetChild(i).gameObject;
            childLayers[i] = child.layer;
            child.layer = 15;
        }

        Vector3[] pathPoints = AStar.PathPoints(astar.Pathfind(out isComplete));
        if (pathPoints == null)
        {
            pathPoints = new Vector3[] { start, end };
        }

        for (int i = 0; i < subject.transform.childCount; i++) //reset layers
        {
            GameObject child = subject.transform.GetChild(i).gameObject;
            child.layer = childLayers[i];
        }

        Path path = new Path(pathPoints);

        return path;
    }
    #endregion

    #region Holdable object manupulation
    /// <summary>
    /// Attempts to place the linked holdable on a static surface.
    /// </summary>
    /// <param name="holdable"></param>
    private void Release(Holdable holdable)
    {
        if (holdable.isControlled) //if the placement method is being overridden by an inventory
        {
            Delink(holdable); //delink without placement procedure
        }
        else
        {
            Place(holdable); //delink with placement procedure
        }
    }

    #region Place / Pick up
    /// <summary>
    /// Places and delinks the object.
    /// </summary>
    /// <param name="holdable"></param>
    private void Place(Holdable holdable)
    {
        if (holdable.FootprintClear())
        {
            Quaternion yaw = Quaternion.Euler(0, holdable.transform.eulerAngles.y, 0);
            float h = 0;
            int layerMask = 1 << 13; //bitwise shift operator (x13)

            //get the height above the static surface
            Ray ray = new Ray(holdable.transform.position, new Vector3(0, -1, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10/*max distance*/, layerMask/*layer mask*/)) //raycast to determine the height above the closest static surface (layer 13) (floors, shelves etc.)
            {
                h = hit.distance;
            }

            Vector3 target = (Vector3.up * (h - holdable.FromGround()) * -1)/*down to the ground +half height*/+ holdable.transform.position; //from the current position


            //placement method
            holdable.transform.rotation = yaw;
            Delink(holdable);
            StartCoroutine(LerpPlace(holdable, target));
        }
        else
        {
            //indicate that the object cannot be placed
        }
    }

    /// <summary>
    /// The version of Place() where the object's placement procedure is handled by an inventory.
    /// </summary>
    private void Delink(Holdable holdable)
    {
        holdable.Delink();
        isHolding = false;
        subject = null;
    }

    /// <summary>
    /// The version of PickUp() where the object's pickup procedure is handled by an inventory.
    /// </summary>
    private void Link(Holdable holdable)
    {
        target.position = holdable.transform.position;
        subject = holdable;
        subjectPath = GetSubjectPath();

        holdable.LinkTo(target);
        isHolding = true;

        targetLastForward = target.forward;

        combinedStartRotation = subject.transform.rotation;
        combinedEndRotation = subject.transform.rotation;

        rotationInput = Quaternion.identity;
    }
    #endregion

    #region Lerp
    /// <summary>
    /// Places lerps the object to place in a set amount of time and locks it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LerpPlace(Holdable holdable, Vector3 target)
    {
        float snapTime = 0.2f;
        float elapsed = 0f;
        float t = 0;
        Vector3 startPosition = holdable.transform.position;

        while (elapsed <= snapTime) //time
        {
            elapsed += Time.deltaTime;
            t = elapsed / snapTime;
            holdable.transform.position = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }
        holdable.transform.position = target;
    }
    #endregion
    #endregion
}
