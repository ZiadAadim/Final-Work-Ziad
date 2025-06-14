using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject smallCanvas;
    [SerializeField] private GameObject bigCanvas;

    public void SwitchToBigCanvas()
    {
        if (smallCanvas == null || bigCanvas == null) return;

        // Copy position, rotation, and scale from small canvas
        bigCanvas.transform.position = smallCanvas.transform.position;
        bigCanvas.transform.rotation = smallCanvas.transform.rotation;

        // Activate big canvas and deactivate small one
        bigCanvas.SetActive(true);
        smallCanvas.SetActive(false);
    }
}
