using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum LegState { Idle, Lifted, Snapped }

// This scripts is attached to the move target, so transform refers to where the leg should be!
public class LegMovement : MonoBehaviour
{
    public LegState currentState = LegState.Idle;
    [SerializeField] private Transform targetLeg;
    [SerializeField] private Transform intermediatePosition;
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private float legLiftDistance = 0.1f;
    [SerializeField] private float transitionDistance = 0.1f;
    [SerializeField] private float legMoveSpeed = 2f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;
    public bool isActiveLeg = false;
    private Vector2 positionAnchor;

    private void Start()
    {
        positionAnchor = transform.position;
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
                snapToTarget(!isForceSnapping);
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            // Bad, that's a nono
            Vector3 point = hit.point;
            transform.position = point + new Vector3(0, 0.1f, 0);
        }
    }

    public void updateActiveLeg(bool isActive)
    {
        isActiveLeg = isActive;
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

    public void snapToTarget(bool shouldUpdateActiveLeg)
    {
        targetLeg.position = Vector3.Lerp(targetLeg.position, transform.position, legMoveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(targetLeg.position, transform.position);
        // Debug.Log("Distance between target and current position " + distance);
        if (distance <= transitionDistance)
        {
            if (shouldUpdateActiveLeg)
            {
                Debug.Log("ON LEG SNAP IS TRIGGERED from " + gameObject.name);
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
