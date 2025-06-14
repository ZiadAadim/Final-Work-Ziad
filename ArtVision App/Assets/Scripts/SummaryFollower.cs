using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryFollower : MonoBehaviour
{
    [SerializeField] private Transform headCanvasTransform;
    [SerializeField] private float zOffset = 0.4f;

    private bool isFollowing = false;

    public void StartFollowing()
    {
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }

    private void Update()
    {
        if (isFollowing && headCanvasTransform != null)
        {
            // Keep position in front of the Head Canvas
            transform.position = headCanvasTransform.position + headCanvasTransform.forward * zOffset;
            // Keep rotation aligned with the Head Canvas
            transform.rotation = headCanvasTransform.rotation;
        }
    }
}

