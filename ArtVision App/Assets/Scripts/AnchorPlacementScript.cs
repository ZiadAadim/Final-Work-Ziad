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
    [SerializeField] private List<GameObject> headBackCoursePrefabs;
    [SerializeField] private List<GameObject> torsoFrontCoursePrefabs;
    [SerializeField] private List<GameObject> torsoQuarterCoursePrefabs;
    [SerializeField] private List<GameObject> torsoSideCoursePrefabs;
    [SerializeField] private GameObject CourseCanvas;

    [Header("Course UI")]
    [SerializeField] private TMPro.TextMeshProUGUI courseNameText;
    [SerializeField] private TMPro.TextMeshProUGUI stepCounterText;
    [SerializeField] private TMPro.TextMeshProUGUI stepExplanationText;
    [SerializeField] private List<string> currentCourseExplanations = new List<string>();
    [SerializeField] private List<string> headFrontExplanations;
    [SerializeField] private List<string> headSideExplanations;
    [SerializeField] private List<string> headQuarterExplanations;
    [SerializeField] private List<string> headBackExplanations;
    [SerializeField] private List<string> torsoFrontExplanations;
    [SerializeField] private List<string> torsoQuarterExplanations;
    [SerializeField] private List<string> torsoSideExplanations;




    [Header("Music")]
    [SerializeField] private AudioSource musicSource;   // drag the AudioSource on SFXManager
    [SerializeField] private AudioClip defaultMusic;    // the track that plays at app launch
    [SerializeField] private AudioClip headFrontMusic;
    [SerializeField] private AudioClip headSideMusic;
    [SerializeField] private AudioClip headQuarterMusic;
    [SerializeField] private AudioClip headBackMusic;
    [SerializeField] private AudioClip torsoFrontMusic;
    [SerializeField] private AudioClip torsoQuarterMusic;
    [SerializeField] private AudioClip torsoSideMusic;

    [Header("Reference Canvases")]
    [SerializeField] private List<GameObject> referencePanels = new List<GameObject>(); // put ALL canvases here
    private GameObject previousReferencePanel;  // runtime only – don’t expose in Inspector



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

    public void StartCourse(List<GameObject> coursePrefabs, List<string> courseExplanations, GameObject referencePanel, string courseName, AudioClip courseClip)
    {

            /* ---------- remember what was showing before ---------- */
    previousReferencePanel = null;
    foreach (GameObject panel in referencePanels)
        if (panel != null && panel.activeSelf)
            previousReferencePanel = panel; 

                /* ---------- hide every panel except the chosen one ----- */
    foreach (GameObject panel in referencePanels)
        if (panel != null && panel != referencePanel)
            panel.SetActive(false);
            

    // make sure the chosen panel is ON
    if (referencePanel != null)
        referencePanel.SetActive(true);

        foreach (GameObject panel in summaryPanels)
            if (panel != null)
                panel.SetActive(false);

        if (CourseCanvas != null)
            CourseCanvas.SetActive(true);

        currentCoursePrefabs = new List<GameObject>(coursePrefabs);
        currentStepIndex = 0;
        courseStarted = true;

        if (currentCoursePrefabs.Count > 0)
        {
            selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        }

        if (musicSource != null && courseClip != null)
        {
            musicSource.Stop();
            musicSource.clip = courseClip;
            musicSource.loop = true;      // most BG loops; keep if desired
            musicSource.Play();
        }

        // UPDATE UI
        if (courseNameText != null)
            courseNameText.text = courseName;
        // UPDATE TEXT EXPLANATION
        currentCoursePrefabs = new List<GameObject>(coursePrefabs);
        currentCourseExplanations = new List<string>(courseExplanations);
        
        UpdateStepExplanation();
        UpdateStepCounter();
        
    }

    private void UpdateStepCounter()
    {
        if (stepCounterText != null && currentCoursePrefabs.Count > 0)
        {
            stepCounterText.text = $"{currentStepIndex + 1}/{currentCoursePrefabs.Count}";
        }
    }

    private void UpdateStepExplanation()
{
    if (stepExplanationText != null && currentCourseExplanations.Count > currentStepIndex)
    {
        stepExplanationText.text = currentCourseExplanations[currentStepIndex];
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

    public void NextStep()
    {
        if (!courseStarted || currentCoursePrefabs.Count == 0) return;

        currentStepIndex = Mathf.Min(currentStepIndex + 1, currentCoursePrefabs.Count - 1);
        selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        SwapToCurrentStepPrefab();
        UpdateStepCounter();
        UpdateStepExplanation();
    }

    public void PreviousStep()
    {
        if (!courseStarted || currentCoursePrefabs.Count == 0) return;

        currentStepIndex = Mathf.Max(currentStepIndex - 1, 0);
        selectedDrawingPrefab = currentCoursePrefabs[currentStepIndex];
        SwapToCurrentStepPrefab();
        UpdateStepCounter();
        UpdateStepExplanation();
    }


    public void StartHeadFrontCourse()
    {
        StartCourse(headFrontCoursePrefabs, headFrontExplanations, referencesPanel, "Front Face", headFrontMusic);
    }

    public void StartHeadSideCourse()
    {
        StartCourse(headSideCoursePrefabs, headSideExplanations, referencesPanel, "Side Face", headSideMusic);
    }

    public void StartHeadQuarterCourse()
    {
        StartCourse(headQuarterCoursePrefabs, headQuarterExplanations, referencesPanel, "Quarter Face", headQuarterMusic);
    }

    public void StartHeadBackCourse()
    {
        StartCourse(headBackCoursePrefabs, headBackExplanations, referencesPanel, "Back Face", headBackMusic);
    }
    
        public void StartTorsoFrontCourse()
    {
        StartCourse(torsoFrontCoursePrefabs, torsoFrontExplanations, referencesPanel, "Torso Front", torsoFrontMusic);
    }

            public void StartTorsoQuarterCourse()
    {
        StartCourse(torsoQuarterCoursePrefabs, torsoQuarterExplanations, referencesPanel, "Torso 3/4", torsoQuarterMusic);
    }

            public void StartTorsoSideCourse()
    {
        StartCourse(torsoSideCoursePrefabs, torsoSideExplanations, referencesPanel, "Torso Side", torsoSideMusic);
    }


    public void StopCourse()
{
    /* ---------- hide the panel used during the course ------- */
    foreach (GameObject panel in referencePanels)
        if (panel != null)
            panel.SetActive(false);

    /* ---------- restore the one that was visible before ------ */
    if (previousReferencePanel != null)
        previousReferencePanel.SetActive(true);

    /* ---------- your existing cleanup ------------------------ */
    if (CourseCanvas) CourseCanvas.SetActive(false);

    courseStarted = false;
    currentCoursePrefabs.Clear();
    currentStepIndex = 0;

    if (currentAnchor)
    {
        Destroy(currentAnchor);
        currentAnchor = null;
    }

    if (musicSource && defaultMusic)
    {
        musicSource.Stop();
        musicSource.clip = defaultMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}

    
    



}
