using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    [SerializeField] private List<LegMovement> legs;
    [SerializeField] private float speed = 3f;
    public float lastMoveTime = 0f;
    private bool isSnapped = false;
    public float timeToSnap = 0.35f;
    [SerializeField] private int activeLegIdx = 0;
    public float movementMagnitude = 0f;

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
            isSnapped = false;
        }

        // if (Time.time - lastMoveTime > timeToSnap && !isSnapped) snapAllLegs();
    }

    public void onLegSnap(LegMovement leg)
    {
        leg.updateActiveLeg(false);
        activeLegIdx++;
        if (activeLegIdx >= legs.Count) activeLegIdx = 0;
        legs[activeLegIdx].updateActiveLeg(true);
    }

    private void snapAllLegs()
    {
        Debug.Log("should snap based on time");
        foreach (LegMovement leg in legs)
        {
            leg.snapToTarget(false);
        }
        isSnapped = true;
    }
}
