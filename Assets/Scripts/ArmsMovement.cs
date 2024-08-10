using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsMovement : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform targetArm;
    [SerializeField] private Transform weaponTarget;
    [SerializeField] private List<Transform> swingTargets;

    private int currentSwingTarget = 0;
    [SerializeField] private float armMoveSpeed = 15f;

    void Update()
    {
        if (weapon != null) holdWeapon();
        else snapArm();
    }

    private void holdWeapon()
    {
        targetArm.position = Vector3.Lerp(targetArm.position, weaponTarget.position, armMoveSpeed * Time.deltaTime);
    }

    private void snapArm()
    {
        Vector3 targetPosition = swingTargets[currentSwingTarget].position;
        targetArm.position = Vector3.Lerp(targetArm.position, targetPosition, armMoveSpeed * Time.deltaTime);

        float distance = Vector3.Distance(targetArm.position, targetPosition);
        if (distance < 0.1f)
        {
            currentSwingTarget++;
            if (currentSwingTarget >= swingTargets.Count)
            {
                currentSwingTarget = 0;
            }
        }
    }
}
