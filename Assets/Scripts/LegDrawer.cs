using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
public class LegDrawer : MonoBehaviour
{
    [SerializeField] private List<Transform> legBones;
    private LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = legBones.Count;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        drawLeg();

        if (lineRenderer.positionCount != legBones.Count)
        {
            lineRenderer.positionCount = legBones.Count;
        }
    }

    private void drawLeg()
    {
        for (int i = 0; i < legBones.Count; i++)
        {
            lineRenderer.SetPosition(i, legBones[i].position);
        }
    }
}
