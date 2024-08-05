using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BodyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private List<LegMovement> legs;
    [SerializeField] private float speed = 3f;
    public bool isGoingRight = true;
    [SerializeField] private int activeLegIdx = 0;
    [SerializeField] private float bodyRotationSpeed = 5f;
    [SerializeField] private float rotationAngleDivider = 2f;
    private Vector2 vectorToAlignTo = Vector2.right;
    [SerializeField] private Transform boxCastStartPoint;
    [SerializeField] private List<Transform> flipTargets;
    [SerializeField] private List<LimbSolver2D> limbSolvers;
    // TODO make this private
    public float lastMoveTime = 0f;
    // TODO make this private
    public bool forceSnap = false;
    // TODO make this private
    public float timeToSnap = 0.35f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 50f;
    public bool isJumping = false;
    [SerializeField] private float startCheckingForGroundAfter = 0.15f;
    [SerializeField] private float jumpingCheckHeightDistance = 1f;
    private float checkGroundAfter = 0f;

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

    private void Start()
    {
        if (activeLegIdx >= legs.Count) activeLegIdx = 0;
        legs[activeLegIdx].updateActiveLeg(true);

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            jump();
        }

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

        if (Time.time - lastMoveTime > timeToSnap && !forceSnap && !isJumping) forceSnap = true;

        // Spring forces are affecting jump force > jumps are different every time, not cool
        bool canCheckForGround = Time.time > checkGroundAfter;
        if (canCheckForGround) springFloat();

        rotateBody();
    }

    private void jump()
    {
        // zeroing out velocity so that the jump height is always consistent
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        isJumping = true;
        forceSnap = false;
        checkGroundAfter = Time.time + startCheckingForGroundAfter;

        foreach (LegMovement leg in legs)
        {
            leg.onJump();
        }
    }

    public void onLand()
    {
        isJumping = false;
        foreach (LegMovement leg in legs)
        {
            leg.onLand();
        }
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
        float angle = Mathf.Atan2(vectorToAlignTo.y, vectorToAlignTo.x) * Mathf.Rad2Deg / rotationAngleDivider;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
    }

    private void springFloat()
    {
        Vector2 rayDirection = Vector2.down;
        float checkDistance = isJumping ? jumpingCheckHeightDistance : heightCheckDistance;

        RaycastHit2D hit = Physics2D.Raycast(boxCastStartPoint.position, rayDirection, checkDistance, springCheckLayer);
        Debug.DrawLine(
            boxCastStartPoint.position,
            new Vector2(boxCastStartPoint.position.x, boxCastStartPoint.position.y) + rayDirection * checkDistance,
            Color.red
        );

        if (hit.collider != null)
        {
            vectorToAlignTo = Quaternion.Euler(0f, 0f, -90f) * hit.normal;
            Debug.DrawRay(hit.point, vectorToAlignTo, Color.green);

            // stop jumping
            if (isJumping) onLand();

            Debug.DrawLine(
                boxCastStartPoint.position,
                hit.point,
                Color.blue
            );

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
    }

    // TODO explore that?
    // as an option you can hide the legs under the body, as piku niku does and wait for the body to hit the ground
    private void OnCollisionEnter2D(Collision2D other)
    {
        // TODO this is not the best idea since you can hit something with your head, and the the jump will be over
        // Combine that with raycast check under the character, to check if the ground is under the character
        // if (isJumping) onLand();
    }
}
