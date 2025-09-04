using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    public static List<RoadNode> FindPath(RoadNode start, RoadNode goal)
    {
        HashSet<RoadNode> closedSet = new HashSet<RoadNode>();
        HashSet<RoadNode> openSet = new HashSet<RoadNode> { start };

        Dictionary<RoadNode, RoadNode> cameFrom = new Dictionary<RoadNode, RoadNode>();

        Dictionary<RoadNode, float> gScore = new Dictionary<RoadNode, float>();
        Dictionary<RoadNode, float> fScore = new Dictionary<RoadNode, float>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            RoadNode current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.neighbors)
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentative_gScore = gScore[current] + Vector3.Distance(current.worldPos, neighbor.worldPos);

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
                else if (tentative_gScore >= (gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue))
                    continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
            }
        }

        return null;
    }

    private static float Heuristic(RoadNode a, RoadNode b)
    {
        return Vector3.Distance(a.worldPos, b.worldPos);
    }

    private static List<RoadNode> ReconstructPath(Dictionary<RoadNode, RoadNode> cameFrom, RoadNode current)
    {
        List<RoadNode> totalPath = new List<RoadNode> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
