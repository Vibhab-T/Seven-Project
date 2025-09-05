using UnityEngine;

public class PlayerCollisionDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Player car initialized at: {transform.position}");

        // Check if player has required components
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("Player car has no collider! Please add a collider component.");
        }
        else
        {
            Debug.Log($"Player collider type: {col.GetType().Name}, isTrigger: {col.isTrigger}");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Player car has no Rigidbody! Please add a Rigidbody component.");
        }
        else
        {
            Debug.Log($"Player Rigidbody found, isKinematic: {rb.isKinematic}");
        }

        // Verify tag
        Debug.Log($"Player tag: {gameObject.tag}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Player triggered with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Waypoint"))
        {
            Debug.Log("Player hit a waypoint!");
        }
    }
}