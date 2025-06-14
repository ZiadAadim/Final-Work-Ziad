using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryPositioner : MonoBehaviour
{
    [SerializeField] private Transform headCanvasTransform;
    [SerializeField] private Transform summaryCanvasTransform;
    [SerializeField] private float zOffset = 0.4f;

    // Call this when you want to show the summary panel
    public void ShowSummaryPanel()
    {
        if (headCanvasTransform == null || summaryCanvasTransform == null)
        {
            Debug.LogWarning("Please assign the Head & Neck and Summary canvases.");
            return;
        }

        // Position the Summary Canvas in front of the Head Canvas
        Vector3 newPosition = headCanvasTransform.position + headCanvasTransform.forward * zOffset;
        summaryCanvasTransform.position = newPosition;

        // Match the rotation (optional but usually recommended)
        summaryCanvasTransform.rotation = headCanvasTransform.rotation;

        // Enable the Summary Canvas
        summaryCanvasTransform.gameObject.SetActive(true);
    
            // Start following
    SummaryFollower follower = summaryCanvasTransform.GetComponent<SummaryFollower>();
    if (follower != null)
    {
        follower.StartFollowing();
    }
}

    // Call this to hide the summary panel
    public void HideSummaryPanel()
    {
        if (summaryCanvasTransform != null)
        {
            summaryCanvasTransform.gameObject.SetActive(false);

            // Stop following
        SummaryFollower follower = summaryCanvasTransform.GetComponent<SummaryFollower>();
        if (follower != null)
        {
            follower.StopFollowing();
        }

        summaryCanvasTransform.gameObject.SetActive(false);
    
        }
    }
}

