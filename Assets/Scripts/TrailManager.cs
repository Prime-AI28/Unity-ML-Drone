using UnityEngine;

public class TrailManager : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailDuration = 1000f;    // How long the trail persists
    [SerializeField] private float startWidth = 0.5f;       // Starting width of the trail
    [SerializeField] private float endWidth = 0.1f;         // Ending width of the trail
    [SerializeField] private Color trailColor = Color.red;  // Trail color

    [Header("Reset Settings")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;  // Key to manually reset trail
    [SerializeField] private bool resetOnCollision = true;  // Reset trail on collision

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;        // Enable debug logs

    private TrailRenderer trailRenderer;
    private bool isInitialized = false;

    void Start()
    {
        InitializeTrail();
    }

    void Update()
    {
        if (!isInitialized) return;

        // Manual reset via key press
        if (Input.GetKeyDown(resetKey))
        {
            ResetTrail();
        }
    }

    private void InitializeTrail()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            if (debugMode) Debug.LogWarning("No TrailRenderer found on " + gameObject.name + ". TrailManager will not run.");
            return;
        }

        // Configure trail settings
        trailRenderer.time = trailDuration;
        trailRenderer.startWidth = startWidth;
        trailRenderer.endWidth = endWidth;

        // Use HDRP material if available, fallback to default otherwise
        Material trailMaterial = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
        trailMaterial.color = trailColor;
        trailRenderer.material = trailMaterial;

        isInitialized = true;
        if (debugMode) Debug.Log("TrailManager initialized on " + gameObject.name);
    }

    public void ResetTrail()
    {
        if (!isInitialized) return;

        trailRenderer.Clear();
        if (debugMode) Debug.Log("Trail reset on " + gameObject.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isInitialized || !resetOnCollision) return;

        // ResetTrail();
        if (debugMode) Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
}