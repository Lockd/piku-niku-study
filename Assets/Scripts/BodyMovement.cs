using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;

public class BodyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private List<LegMovement> legs;
    [SerializeField] private float speed = 3f;
    public bool isGoingRight = true;
    [SerializeField] private int activeLegIdx = 0;
    [SerializeField] private float bodyRotationSpeed = 5f;
    [SerializeField] private float rotationAngleDivider = 2f;
    [SerializeField] private Transform boxCastStartPoint;
    [SerializeField] private List<Transform> flipTargets;
    [SerializeField] private List<LimbSolver2D> limbSolvers;
    // TODO make this private
    public float lastMoveTime = 0f;
    // TODO make this private
    public bool forceSnap = false;
    // TODO make this private
    public float timeToSnap = 0.35f;

    [Header("Spring force")]
    [SerializeField] private float floatHeight = 1.4f;
    [SerializeField] private float floatSpringStrength = 30f;
    [SerializeField] private float floatSpringDamper = 5f;
    [SerializeField] private float heightCheckDistance = 2f;
    [SerializeField] private LayerMask springCheckLayer;
    [SerializeField] private List<Transform> rayOrigins;
    private Rigidbody2D rb;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    public static BodyMovement instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // TODO add logic to check distance to the ground and spring back to it like in very very vale game
    // https://www.youtube.com/watch?v=qdskE8PJy6Q&ab_channel=ToyfulGames

    private void Start()
    {
        if (activeLegIdx >= legs.Count) activeLegIdx = 0;
        legs[activeLegIdx].updateActiveLeg(true);

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        bool isMoving = movement.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            lastMoveTime = Time.time;
            forceSnap = false;
            bool newIsGoingRight = movement.x > 0;

            animator.SetBool("isMovingRight", newIsGoingRight);
            if (newIsGoingRight != isGoingRight) flip();
        }
        rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);

        if (Time.time - lastMoveTime > timeToSnap && !forceSnap) forceSnap = true;

        springFloat();
        rotateBody();
    }

    private void flip()
    {
        isGoingRight = !isGoingRight;
        foreach (Transform target in flipTargets)
        {
            target.localScale = new Vector3(target.localScale.x * -1, target.localScale.y, target.localScale.z);
        }

        foreach (LimbSolver2D limbSolver in limbSolvers)
        {
            limbSolver.flip = !limbSolver.flip;
        }
    }

    public void onLegSnap(LegMovement leg)
    {
        leg.updateActiveLeg(false);
        activeLegIdx++;
        if (activeLegIdx >= legs.Count) activeLegIdx = 0;
        legs[activeLegIdx].updateActiveLeg(true);
    }

    private void rotateBody()
    {
        // TODO make this work for more than 2 legs
        Vector2 vectorToAlignTo = legs[1].transform.position - legs[0].transform.position;
        Debug.DrawLine(legs[0].transform.position, legs[1].transform.position, Color.green);

        float angle = Mathf.Atan2(vectorToAlignTo.y, vectorToAlignTo.x) * Mathf.Rad2Deg / rotationAngleDivider;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
    }

    private void springFloat()
    {

        Vector2 rayDirection = Vector2.down;
        RaycastHit2D hit = Physics2D.BoxCast(boxCastStartPoint.position, new Vector2(0.5f, 0.5f), 0f, rayDirection, heightCheckDistance, springCheckLayer);
        if (hit.collider != null)
        {
            Vector2 velocity = rb.velocity;

            Vector2 hitBodyVel = Vector2.zero;
            Rigidbody2D hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                hitBodyVel = hitBody.velocity;
            }

            float rayDirVelocity = Vector2.Dot(rayDirection, velocity);
            float hitBodyDirVelocity = Vector2.Dot(rayDirection, hitBodyVel);

            float relVel = rayDirVelocity - hitBodyDirVelocity;

            float x = hit.distance - floatHeight;

            float springForce = (x * floatSpringStrength) - (relVel * floatSpringDamper);

            Vector3 debugLine = transform.position + new Vector3(rayDirection.x, rayDirection.y, 0f) * springForce;
            Debug.DrawLine(transform.position, debugLine, Color.red);

            rb.AddForce(rayDirection * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(-rayDirection * springForce, hit.point);
            }
        }
        else
        {
            Debug.Log("No hit");
        }
    }
}
