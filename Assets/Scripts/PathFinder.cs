using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }

    public class PathNode
    {
        public Vector3Int position;
        public int gCost; // Cost from the start node.
        public int hCost; // Heuristic cost to the end node.
        public PathNode parent; // The node that came before this one.

        public int FCost => gCost + hCost;

        public PathNode(Vector3Int position)
        {
            this.position = position;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Vector3Int> GetReachableTiles(Vector3Int startPosition, int movementRange, System.Func<Vector3Int, bool> IsValidPosition)
    {
        List<Vector3Int> reachableTiles = new List<Vector3Int>();
        Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();

        frontier.Enqueue(startPosition);
        costSoFar[startPosition] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            foreach (var neighbor in GetNeighbours(current))
            {
                int newCost = costSoFar[current] + 1; // 1 cost per tile regardless of direction
                if (newCost <= movementRange && IsValidPosition(neighbor))
                {
                    if (!costSoFar.ContainsKey(neighbor))
                    {
                        costSoFar[neighbor] = newCost;
                        frontier.Enqueue(neighbor);
                        reachableTiles.Add(neighbor);
                    }
                }
            }
        }
        return reachableTiles;
    }

    public List<Vector3Int> GetPath(Vector3Int startPosition, Vector3Int endPosition, System.Func<Vector3Int, bool> IsValidPosition)
    {
        PathNode startNode = new PathNode(startPosition);
        PathNode endNode = new PathNode(endPosition);

        List<PathNode> openSet = new List<PathNode> { startNode };
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode.position, endNode.position);

        while (openSet.Count > 0)
        {
            // Find the node with the lowest F-cost in the open set.
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                   (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);

            // Path found
            if (currentNode.position == endNode.position)
            {
                return ReconstructPath(currentNode);
            }

            foreach (Vector3Int neighbourPosition in GetNeighbours(currentNode.position))
            {
                if (closedSet.Contains(neighbourPosition) || !IsValidPosition(neighbourPosition))
                {
                    continue;
                }

                // FIXED: Use cost of 1 for all movements (orthogonal and diagonal)
                int tentativeGCost = currentNode.gCost + 1;

                PathNode neighbourNode = openSet.Find(n => n.position == neighbourPosition);
                if (neighbourNode == null || tentativeGCost < neighbourNode.gCost)
                {
                    if (neighbourNode == null)
                    {
                        neighbourNode = new PathNode(neighbourPosition);
                        openSet.Add(neighbourNode);
                    }

                    neighbourNode.parent = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode.position, endNode.position);
                }
            }
        }

        // Path not found
        return new List<Vector3Int>();
    }

    private IEnumerable<Vector3Int> GetNeighbours(Vector3Int node)
    {
        // 4-directional movement (orthogonal)
        yield return node + Vector3Int.right;
        yield return node + Vector3Int.left;
        yield return node + Vector3Int.up;
        yield return node + Vector3Int.down;

        // 4-directional diagonal movement
        yield return node + new Vector3Int(1, 1, 0);
        yield return node + new Vector3Int(-1, -1, 0);
        yield return node + new Vector3Int(-1, 1, 0);
        yield return node + new Vector3Int(1, -1, 0);
    }

    private List<Vector3Int> ReconstructPath(PathNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
    private int CalculateDistanceCost(Vector3Int a, Vector3Int b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        // Chebyshev distance: max of horizontal and vertical distance
        // This is admissible when diagonal movement costs the same as orthogonal
        return Mathf.Max(xDistance, yDistance);
    }
}