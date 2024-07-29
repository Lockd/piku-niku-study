using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private List<LegMovement> legs;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int activeLegIdx = 0;
    [SerializeField] private float bodyRotationSpeed = 5f;
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
    private Rigidbody2D rb;

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

        if (movement.magnitude > 0.1f)
        {
            lastMoveTime = Time.time;
            forceSnap = false;
        }
        rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);

        if (Time.time - lastMoveTime > timeToSnap && !forceSnap) forceSnap = true;

        springFloat();
        rotateBody();
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

        float angle = Mathf.Atan2(vectorToAlignTo.y, vectorToAlignTo.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
    }

    private void springFloat()
    {

        Vector2 rayDirection = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, heightCheckDistance, springCheckLayer);

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
