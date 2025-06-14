using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPlacementScript : MonoBehaviour
{
    [Header("Anchor Placement")]
    public Transform pageAnchorLeft;
    public Transform pageAnchorRight;


    private GameObject currentAnchor;
    private GameObject selectedDrawingPrefab;
    private GameObject lastSpawnedPrefab;

    private Vector3 lastDrawingScale = Vector3.zero;
    private float lastDrawingOpacity = 1.0f;

    private bool isMoving = false;
    private bool isScaling = false;
    private bool selectedDrawingChanged = false;

    private Vector3 moveStartPos;
    private Vector3 anchorStartPos;
    private float scaleStartY;
    private Vector3 anchorStartScale;
    private int currentStepIndex = 0;
    private bool isVisible = true;

    [SerializeField] private GameObject courseDrawingPrefab;
    [SerializeField] private GameObject referencesPanel;
    [SerializeField] private List<GameObject> summaryPanels = new List<GameObject>();

    [Header("Course Management")]
    [SerializeField] private List<GameObject> currentCoursePrefabs; // List of prefabs for the active course
    private bool courseStarted = false;


    [Header("Courses")]
    [SerializeField] private List<GameObject> headFrontCoursePrefabs;
    [SerializeField] private List<GameObject> headSideCoursePrefabs;
    [SerializeField] private List<GameObject> headQuarterCoursePrefabs;
    [SerializeField] private GameObject stopButton;

void Update()
{
    // Drawing placement - ONLY if a course is active
    if (courseStarted)
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            CreateSpatialAnchor(pageAnchorRight);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            CreateSpatialAnchor(pageAnchorLeft);
    }

    // Toggle drawing visibility
    if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
    OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
    {
        ToggleDrawing();
    }

    // Movement and scaling
    if (currentAnchor != null)
    {
        HandleMovement();
        HandleScaling();
    }

    if (courseStarted)
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch) ||
        OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))
    {
        NextStep();
    }

    if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch) ||
        OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))
    {
        PreviousStep();
    }
    }
}


private void HandleMovement()
{
    if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
    {
        isMoving = true;
        moveStartPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        anchorStartPos = currentAnchor.transform.position;
    }
    else if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
    {
        isMoving = true;
        moveStartPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        anchorStartPos = currentAnchor.transform.position;
    }

    if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) ||
        OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch))
    {
        isMoving = false;
    }

    if (isMoving)
    {
        // Use the most recent hand that started movement
        OVRInput.Controller activeController = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch) ? 
                                               OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

        Vector3 currentPos = OVRInput.GetLocalControllerPosition(activeController);
        Vector3 delta = currentPos - moveStartPos;
        Vector3 worldDelta = Camera.main.transform.TransformDirection(delta);
        currentAnchor.transform.position = anchorStartPos + worldDelta;
    }
}

private void HandleScaling()
{
    // Handle Button.Two from both controllers
    bool isButtonTwoDown = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch) ||
                           OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch);
    bool isButtonTwoUp = OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch) ||
                         OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.LTouch);

    if (isButtonTwoDown)
    {
        isScaling = true;

        // Prefer right hand for scale origin, fallback to left
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            scaleStartY = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch).y;
        else
            scaleStartY = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch).y;

        anchorStartScale = currentAnchor.transform.localScale;
    }

    if (isButtonTwoUp)
        isScaling = false;

    if (isScaling)
    {
        float currentY = 0;

        // Use whichever controller is still scaling
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            currentY = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch).y;
        else if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            currentY = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch).y;

        float deltaY = currentY - scaleStartY;
        float sensitivity = 2f;
        float scaleFactor = 1 + deltaY * sensitivity;
        scaleFactor = Mathf.Clamp(scaleFactor, 0.2f, 5f);
        currentAnchor.transform.localScale = anchorStartScale * scaleFactor;
    }
}

    public void CreateSpatialAnchor(Transform targetAnchor)
    {
        // Save existing drawing state if needed
        if (currentAnchor != null)
        {
            lastDrawingScale = currentAnchor.transform.localScale;

            Renderer renderer = currentAnchor.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                lastDrawingOpacity = renderer.material.color.a;
            }

            Destroy(currentAnchor);
        }

        if (selectedDrawingPrefab == null)
{
    Debug.LogWarning("No selected drawing prefab assigned!");
    return;
}
GameObject prefabToSpawn = selectedDrawingPrefab;
 
        Quaternion originalRotation = targetAnchor.rotation;
        Quaternion flatRotation = Quaternion.Euler(0, originalRotation.eulerAngles.y, 0);

        bool isNewPrefab = prefabToSpawn != lastSpawnedPrefab;
        lastSpawnedPrefab = prefabToSpawn;

        currentAnchor = Instantiate(prefabToSpawn, targetAnchor.position, flatRotation);
        currentAnchor.AddComponent<OVRSpatialAnchor>();

        // Apply scale if same prefab as before
        if (!isNewPrefab && lastDrawingScale != Vector3.zero)
        {
            currentAnchor.transform.localScale = lastDrawingScale;
        }

        // Apply opacity
        Renderer newRenderer = currentAnchor.GetComponent<Renderer>();
        if (newRenderer != null && newRenderer.material.HasProperty("_Color"))
        {
            Color color = newRenderer.material.color;
            color.a = lastDrawingOpacity;
            newRenderer.material.color = color;
        }
    }

    public void ToggleDrawing()
    {
        if (currentAnchor == null) return;

        isVisible = !isVisible;
        currentAnchor.SetActive(isVisible);
    }

    public void SetDrawingOpacity(float opacity)
    {
        if (currentAnchor == null) return;

        Renderer renderer = currentAnchor.GetComponent<Renderer>();
        if (renderer == null) return;

        Color color = renderer.material.color;
        color.a = Mathf.Clamp01(opacity);
        renderer.material.color = color;
    }

    public void SetSelectedDrawingPrefab(GameObject drawingPrefab)
    {
        selectedDrawingPrefab = drawingPrefab;
        selectedDrawingChanged = true;
    }

    public void StartCourse(List<GameObject> coursePrefabs, GameObject referencesPanel)
    {
        if (referencesPanel != null)
            referencesPanel.SetActive(false);

        foreach (GameObject panel in summaryPanels)
      {
        if (panel != null)
             panel.SetActive(false);
      }


        if (stopButton != null)
            stopButton.SetActive(true);

        currentCoursePrefabs = new List<GameObject>(coursePrefabs);
        currentStepIndex = 0;
        courseStarted = true;

        if (currentCoursePrefabs.Count > 0)
        {
            selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        }
    }

    private void SwapToCurrentStepPrefab()
    {
        if (currentAnchor == null) return;

        // Save position, rotation, and scale
        Vector3 savedPosition = currentAnchor.transform.position;
        Quaternion savedRotation = currentAnchor.transform.rotation;
        Vector3 savedScale = currentAnchor.transform.localScale;

        // Save opacity
        Renderer oldRenderer = currentAnchor.GetComponent<Renderer>();
        float savedOpacity = 1f;
        if (oldRenderer != null && oldRenderer.material.HasProperty("_Color"))
        {
            savedOpacity = oldRenderer.material.color.a;
        }

        // Destroy old anchor
        Destroy(currentAnchor);

        // Spawn the new step prefab
        GameObject newPrefab = currentCoursePrefabs[currentStepIndex];
        currentAnchor = Instantiate(newPrefab, savedPosition, savedRotation);
        currentAnchor.AddComponent<OVRSpatialAnchor>();
        currentAnchor.transform.localScale = savedScale;

        // Apply opacity
        Renderer newRenderer = currentAnchor.GetComponent<Renderer>();
        if (newRenderer != null && newRenderer.material.HasProperty("_Color"))
        {
            Color color = newRenderer.material.color;
            color.a = savedOpacity;
            newRenderer.material.color = color;
        }
    }

    private void NextStep()
    {
        if (!courseStarted || currentCoursePrefabs.Count == 0) return;

        currentStepIndex = Mathf.Min(currentStepIndex + 1, currentCoursePrefabs.Count - 1);
        selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        SwapToCurrentStepPrefab();
    }

    private void PreviousStep()
    {
        if (!courseStarted || currentCoursePrefabs.Count == 0) return;

        currentStepIndex = Mathf.Max(currentStepIndex - 1, 0);
        selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        SwapToCurrentStepPrefab();
    }


    public void StartHeadFrontCourse()
    {
        StartCourse(headFrontCoursePrefabs, referencesPanel);
    }

    public void StartHeadSideCourse()
    {
        StartCourse(headSideCoursePrefabs, referencesPanel);
    }

    public void StartHeadQuarterCourse()
    {
        StartCourse(headQuarterCoursePrefabs, referencesPanel);
    }
    
    public void StopCourse()
{
    // Hide stop button
    if (stopButton != null)
        stopButton.SetActive(false);

    // Reactivate panels
    if (referencesPanel != null)
        referencesPanel.SetActive(true);


    // Reset course state
    courseStarted = false;
    currentCoursePrefabs.Clear();
    currentStepIndex = 0;

    // Remove current anchor if desired
    if (currentAnchor != null)
    {
        Destroy(currentAnchor);
        currentAnchor = null;
    }
}



}
