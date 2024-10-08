using System.Collections.Generic;
using UnityEngine;

public enum LegState { Idle, Lifted, Snapped, Jumping }

// This scripts is attached to the move target, so transform refers to where the leg should be!
public class LegMovement : MonoBehaviour
{
    public LegState currentState = LegState.Idle;
    [Header("Movement")]
    [SerializeField] private Transform targetLeg;
    [SerializeField] private Transform intermediatePosition;
    [SerializeField] private Transform stepTarget;
    [SerializeField] private List<Transform> jumpTargets;
    private int currentJumpTarget = 0;
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private float legLiftDistance = 0.1f;
    [SerializeField] private float transitionDistance = 0.1f;
    [SerializeField] private float legWalkSpeed = 2f;
    [SerializeField] private float legActionsSpeed = 2f;

    [Header("Ground checks")]
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform raycastBackup;
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
        if (feet != null) defaultFeetRotation = feet.rotation;
    }

    // TODO have static lift feet position
    // TODO have target snap feet position
    // TODO keep the snap feet position when idle

    void FixedUpdate()
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
                liftLeg(intermediatePosition);
                break;
            case LegState.Jumping:
                liftLeg(jumpTargets[currentJumpTarget]);
                break;
            case LegState.Snapped:
                Transform target = isForceSnapping ? transform : stepTarget;
                snapToTarget(!isForceSnapping, target);
                break;
            default:
                break;
        }
    }

    private void checkGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawLine(raycastOrigin.position, raycastOrigin.position + Vector3.down * groundCheckDistance, Color.green);

        if (hit.collider != null)
        {
            Vector3 point = hit.point;
            transform.position = point + new Vector3(0, 0.1f, 0);
        }
        else
        {
            // TODO not sure if that's a good idea, but looks cute
            RaycastHit2D hitBackup = Physics2D.Raycast(raycastBackup.position, Vector2.down, groundCheckDistance, groundLayer);
            Debug.DrawLine(raycastBackup.position, raycastBackup.position + Vector3.down * groundCheckDistance, Color.green);

            if (hitBackup.collider != null)
            {
                Vector3 point = hitBackup.point;
                transform.position = point + new Vector3(0, 0.1f, 0);
            }
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
            feet.rotation = Quaternion.Slerp(feet.rotation, targetRotation, Time.fixedDeltaTime * feetRotationSpeed);
        }
        else
        {
            feet.rotation = Quaternion.Slerp(feet.rotation, defaultFeetRotation, Time.fixedDeltaTime * feetRotationSpeed);
        }
    }

    public void updateActiveLeg(bool isActive)
    {
        isActiveLeg = isActive;
    }

    public void onFlip()
    {
        positionAnchor = transform.position;
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
        targetLeg.position = Vector3.Lerp(targetLeg.position, target.position, legWalkSpeed * Time.fixedDeltaTime);

        float distance = Vector2.Distance(targetLeg.position, target.position);
        // Debug.Log("Distance between target and current position " + distance);
        if (distance <= transitionDistance)
        {
            if (shouldUpdateActiveLeg)
            {
                // Debug.Log("ON LEG SNAP IS TRIGGERED from " + gameObject.name);
                BodyMovement.instance.onLegSnap();
            }
            currentState = LegState.Idle;
        }
    }

    public void onJump()
    {
        currentState = LegState.Jumping;
    }

    public void onLand()
    {
        currentState = LegState.Snapped;
    }

    public void liftLeg(Transform liftPosition)
    {
        targetLeg.position = Vector3.Lerp(targetLeg.position, liftPosition.position, legActionsSpeed * Time.fixedDeltaTime);

        // Jumping state is turned off from ground check, not dependent on distance
        if (currentState == LegState.Jumping && Vector2.Distance(targetLeg.position, jumpTargets[currentJumpTarget].position) < transitionDistance)
        {
            currentJumpTarget++;
            if (currentJumpTarget >= jumpTargets.Count) currentJumpTarget = 0;
        }

        if (currentState == LegState.Lifted && Vector2.Distance(positionAnchor, transform.position) >= snapDistance)
        {
            currentState = LegState.Snapped;
        }
    }
}
