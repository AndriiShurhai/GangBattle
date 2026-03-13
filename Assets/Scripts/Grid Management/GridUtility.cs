using UnityEngine;

public static class GridUtility
{
    private static readonly LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");
    public static bool HasLineOfSight(Vector3Int casterPosition, Vector3Int targetPosition)
    {
        Vector3 start = GridManager.Instance.GridToWorld(casterPosition);
        Vector3 end = GridManager.Instance.GridToWorld(targetPosition);

        Vector3 direction = (end - start).normalized;

        float distance = Vector3.Distance(start, end);

        if (Physics2D.Raycast(start, direction, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }
}
