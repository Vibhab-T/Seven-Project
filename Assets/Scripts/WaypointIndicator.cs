using UnityEngine;
using TMPro;

public class WaypointIndicator : MonoBehaviour
{
    public TextMeshPro numberText;
    public GameObject arrowObject;
    public int pointsValue = 10;
    public bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player car, if waypoint hasn't been collected, and if timer is running
        if (!isCollected && other.CompareTag("Player") && UIManager.Instance.IsTimerRunning())
        {
            isCollected = true;

            // Add score through UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddScore(pointsValue);
            }

            // Visual feedback - disable or hide the waypoint
            if (arrowObject != null) arrowObject.SetActive(false);
            if (numberText != null) numberText.gameObject.SetActive(false);

            Debug.Log($"Waypoint collected! +{pointsValue} points");
        }
    }

    public void SetOrderNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number.ToString();
        }
    }
}