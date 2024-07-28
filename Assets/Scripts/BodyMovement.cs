using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    // TODO add logic to check distance to the ground and spring back to it like in very very vale game
    // https://www.youtube.com/watch?v=qdskE8PJy6Q&ab_channel=ToyfulGames

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        // float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        transform.position += movement * speed * Time.deltaTime;
    }
}
