using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public enum DecalState { Idle, MovingLeft, MovingRight }

public class Decal : MonoBehaviour
{

    [SerializeField] private DecalState currentState = DecalState.Idle;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private Transform targetIK;
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Transform rightTarget;
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private LimbSolver2D limbSolver;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            onMove(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            onMove(true);
        }

    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case DecalState.Idle:
                snapToTarget(defaultTarget);
                break;
            case DecalState.MovingLeft:
                snapToTarget(leftTarget);
                break;
            case DecalState.MovingRight:
                snapToTarget(rightTarget);
                break;
        }
    }

    private void snapToTarget(Transform target)
    {
        targetIK.position = Vector3.Lerp(targetIK.position, target.position, movementSpeed * Time.fixedDeltaTime);
        float distance = Vector2.Distance(targetIK.position, target.position);

        if (distance <= 0.05f)
        {
            currentState = DecalState.Idle;
        }
    }

    public void onMove(bool isMovingRight)
    {
        currentState = isMovingRight ? DecalState.MovingRight : DecalState.MovingLeft;
        limbSolver.flip = !isMovingRight;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == gameObject.layer)
        {
            Debug.Log("Should move");
            onMove(BodyMovement.instance.isGoingRight);
        }
    }
}
