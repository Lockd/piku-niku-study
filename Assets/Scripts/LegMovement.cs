using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovement : MonoBehaviour
{
    // This scripts is attached to the move target, so transform refers to where the leg should be!
    [SerializeField] private Transform targetLeg;
    [SerializeField] private float maxDistanceToTarget = 1.0f;
    [SerializeField] private float legMoveSpeed = 2f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    void Update()
    {
        checkGround();
        float distance = Vector2.Distance(transform.position, targetLeg.position);
        Debug.Log("Distance between leg and it's target" + distance);
        if (distance > maxDistanceToTarget)
        {
            snapToTarget();
        }
    }

    public void snapToTarget()
    {
        Debug.Log("Should snap leg");
        targetLeg.position = transform.position;
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
}
