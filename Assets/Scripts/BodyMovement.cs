using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    [SerializeField] private List<LegMovement> legs;
    [SerializeField] private float speed = 3f;
    public float lastMoveTime = 0f;
    public bool forceSnap = false;
    public float timeToSnap = 0.35f;
    [SerializeField] private int activeLegIdx = 0;
    public float movementMagnitude = 0f;

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
        // float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        movementMagnitude = movement.magnitude;

        if (movement.magnitude > 0.1f)
        {
            lastMoveTime = Time.time;
            transform.position += movement * speed * Time.deltaTime;
            forceSnap = false;
        }

        if (Time.time - lastMoveTime > timeToSnap && !forceSnap) forceSnap = true;

        springFloat();
    }

    public void onLegSnap(LegMovement leg)
    {
        leg.updateActiveLeg(false);
        activeLegIdx++;
        if (activeLegIdx >= legs.Count) activeLegIdx = 0;
        legs[activeLegIdx].updateActiveLeg(true);
    }

    private void springFloat()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, heightCheckDistance, springCheckLayer);

        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            Vector2 velocity = rb.velocity;
            Vector2 rayDirection = Vector2.down;

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
