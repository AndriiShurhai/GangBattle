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
                else if (!IsValidPosition(neighbor))
                {
                    Debug.LogWarning("The position is invalid");
                }
            }
        }
        return reachableTiles;
    }

    public List<Vector3Int> GetPath(Vector3Int startPosition, Vector3Int endPosition, System.Func<Vector3Int, bool> IsValidPosition)
    {
        PathNode startNode = new PathNode(startPosition);
        PathNode endNode = new PathNode(endPosition);

        PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, PathNode> openSetNodes = new Dictionary<Vector3Int, PathNode>();

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode.position, endNode.position);

        openSet.Enqueue(startNode, startNode.FCost);
        openSetNodes[startNode.position] = startNode;

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet.Dequeue();
            openSetNodes.Remove(currentNode.position);

            // Path found
            if (currentNode.position == endNode.position)
            {
                return ReconstructPath(currentNode);
            }

            closedSet.Add(currentNode.position);


            foreach (Vector3Int neighbourPosition in GetNeighbours(currentNode.position))
            {
                if (closedSet.Contains(neighbourPosition) || !IsValidPosition(neighbourPosition))
                {
                    continue;
                }

                int tentativeGCost = currentNode.gCost + 1;

                if (openSetNodes.TryGetValue(neighbourPosition, out PathNode neighbourNode))
                {
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.parent = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode.position, endNode.position);

                        openSet.Enqueue(neighbourNode, neighbourNode.FCost);
                    }
                }
                else
                {
                    neighbourNode = new PathNode(neighbourPosition);
                    neighbourNode.parent = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode.position, endNode.position);
                    openSet.Enqueue(neighbourNode, neighbourNode.FCost);
                    openSetNodes[neighbourPosition] = neighbourNode;
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