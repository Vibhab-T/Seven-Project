using System.Collections.Generic;
using UnityEngine;

public class RoadNode
{
    public Vector2Int gridPos;
    public Vector3 worldPos;
    public List<RoadNode> neighbors = new List<RoadNode>();

    public RoadNode(Vector2Int grid, Vector3 world)
    {
        gridPos = grid;
        worldPos = world;
    }
}
