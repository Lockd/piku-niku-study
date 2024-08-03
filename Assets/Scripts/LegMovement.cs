using UnityEngine;

public enum LegState { Idle, Lifted, Snapped }

// This scripts is attached to the move target, so transform refers to where the leg should be!
public class LegMovement : MonoBehaviour
{
    public LegState currentState = LegState.Idle;
    [Header("Movement")]
    [SerializeField] private Transform targetLeg;
    [SerializeField] private Transform intermediatePosition;
    [SerializeField] private Transform stepTarget;
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private float legLiftDistance = 0.1f;
    [SerializeField] private float transitionDistance = 0.1f;
    [SerializeField] private float legMoveSpeed = 2f;

    [Header("Ground checks")]
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;
    public bool isActiveLeg = false;
    public bool canMoveLegs = true;

    [Header("Feet rotation")]
    [SerializeField] private float feetRotationSpeed = 5f;
    [SerializeField] private Transform feet;
    private Quaternion defaultFeetRotation;
    private Vector2 positionAnchor;

    private void Start()
    {
        positionAnchor = transform.position;
        defaultFeetRotation = feet.rotation;
    }

    void Update()
    {
        checkGround();

        // Snap legs if the character is not moving for a certain time period
        bool isForceSnapping = BodyMovement.instance.forceSnap;
        if (isForceSnapping)
        {
            currentState = LegState.Snapped;
        }

        switch (currentState)
        {
            case LegState.Idle:
                checkIfShouldLift();
                break;
            case LegState.Lifted:
                liftLeg();
                break;
            case LegState.Snapped:
                Transform target = isForceSnapping ? transform : stepTarget;
                snapToTarget(!isForceSnapping, target);
                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentState = LegState.Lifted;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentState = LegState.Snapped;
        }
    }

    private void checkGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            Vector3 point = hit.point;
            // TODO replace this constant so that the leg placement looks better? 
            transform.position = point + new Vector3(0, 0.1f, 0);
        }

        rotateFeet(hit);
    }

    private void rotateFeet(RaycastHit2D hit)
    {
        if (feet == null) return;

        if (hit.collider != null && currentState != LegState.Lifted)
        {
            Vector2 normal = hit.normal;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            feet.rotation = Quaternion.Slerp(feet.rotation, targetRotation, Time.deltaTime * feetRotationSpeed);
        }
        else
        {
            feet.rotation = Quaternion.Slerp(feet.rotation, defaultFeetRotation, Time.deltaTime * feetRotationSpeed);
        }
    }

    public void updateActiveLeg(bool isActive)
    {
        isActiveLeg = isActive;
    }

    public void onLegsFlip()
    {
        turnOffMovement();
    }

    private void turnOffMovement()
    {
        canMoveLegs = false;
    }

    private void turnOnMovement()
    {
        canMoveLegs = true;
    }

    private void checkIfShouldLift()
    {
        if (!isActiveLeg) return;

        float distance = Vector2.Distance(transform.position, positionAnchor);

        if (distance > legLiftDistance && isActiveLeg)
        {
            positionAnchor = transform.position;
            currentState = LegState.Lifted;
        }
    }

    public void snapToTarget(bool shouldUpdateActiveLeg, Transform target)
    {
        targetLeg.position = Vector3.Lerp(targetLeg.position, target.position, legMoveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(targetLeg.position, target.position);
        // Debug.Log("Distance between target and current position " + distance);
        if (distance <= transitionDistance)
        {
            if (shouldUpdateActiveLeg)
            {
                // Debug.Log("ON LEG SNAP IS TRIGGERED from " + gameObject.name);
                BodyMovement.instance.onLegSnap(this);
            }
            currentState = LegState.Idle;
        }
    }

    public void liftLeg()
    {
        targetLeg.position = Vector3.Lerp(targetLeg.position, intermediatePosition.position, legMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(positionAnchor, transform.position) >= snapDistance)
        {
            currentState = LegState.Snapped;
        }
    }
}
