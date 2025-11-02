using UnityEngine;

public static class GridUtility
{
    public static bool HasLineOfSight(Vector3Int casterPosition, Vector3Int targetPositoin)
    {
        Vector3 start = GridManager.Instance.GridToWorld(casterPosition);
        Vector3 end = GridManager.Instance.GridToWorld(targetPositoin);

        Vector3 direction = (end - start).normalized;

        float distance = Vector3.Distance(start, end);
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");

        if (Physics2D.Raycast(start, direction, distance, obstacleLayer))
        {
            return false;
        }

        return true;
    }
}
