using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    public float speed = 5f;
    private List<RoadNode> path;
    private int currentIndex = 0;

    public void SetPath(List<RoadNode> newPath)
    {
        path = newPath;
        currentIndex = 0;
    }

    void Update()
    {
        if (path == null || currentIndex >= path.Count) return;

        Vector3 target = path[currentIndex].worldPos;
        target.y = transform.position.y; // stay on ground

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.forward = dir;

        if (Vector3.Distance(transform.position, target) < 0.1f)
            currentIndex++;
    }
}
