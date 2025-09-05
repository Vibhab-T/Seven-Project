using UnityEngine;
using TMPro;

public class WaypointIndicator : MonoBehaviour
{
    public TextMeshPro numberText;
    public GameObject arrowObject;
    public int pointsValue = 10;
    public bool isCollected = false;

    private Collider waypointCollider;
    private Renderer waypointRenderer;

    void Start()
    {
        // Get components
        waypointCollider = GetComponent<Collider>();
        waypointRenderer = GetComponent<Renderer>();

        // Ensure collider is set up correctly
        if (waypointCollider != null)
        {
            waypointCollider.isTrigger = true;
            Debug.Log($"Waypoint collider is trigger: {waypointCollider.isTrigger}");
        }
        else
        {
            Debug.LogError("No collider found on waypoint! Please add a collider component.");
        }

        // Log for debugging
        Debug.Log($"Waypoint initialized at position: {transform.position}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collision detected with: {other.name}, Tag: {other.tag}");

        // Check if the collider is the player car, if waypoint hasn't been collected
        if (!isCollected && other.CompareTag("Player"))
        {
            Debug.Log("Player detected, checking timer...");

            // Check if UIManager exists and timer is running
            if (UIManager.Instance != null)
            {
                if (UIManager.Instance.IsTimerRunning())
                {
                    CollectWaypoint();
                }
                else
                {
                    Debug.Log("Timer is not running, waypoint not collected");
                }
            }
            else
            {
                Debug.LogError("UIManager instance is null!");
            }
        }
    }

    void CollectWaypoint()
    {
        isCollected = true;
        Debug.Log($"Collecting waypoint worth {pointsValue} points");

        // Add score through UIManager
        UIManager.Instance.AddScore(pointsValue);

        // Visual feedback - disable or hide the waypoint
        if (arrowObject != null)
        {
            arrowObject.SetActive(false);
            Debug.Log("Arrow object disabled");
        }

        if (numberText != null)
        {
            numberText.gameObject.SetActive(false);
            Debug.Log("Number text disabled");
        }

        if (waypointCollider != null)
        {
            waypointCollider.enabled = false;
            Debug.Log("Collider disabled");
        }

        if (waypointRenderer != null)
        {
            waypointRenderer.enabled = false;
            Debug.Log("Renderer disabled");
        }
    }

    public void SetOrderNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number.ToString();
        }
    }

    // Visual indicator in editor to see trigger bounds
    void OnDrawGizmos()
    {
        if (waypointCollider != null && !isCollected)
        {
            Gizmos.color = Color.green;
            if (waypointCollider is SphereCollider)
            {
                SphereCollider sphere = waypointCollider as SphereCollider;
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
            else if (waypointCollider is BoxCollider)
            {
                BoxCollider box = waypointCollider as BoxCollider;
                Gizmos.DrawWireCube(transform.position + box.center, box.size);
            }
        }
    }
}