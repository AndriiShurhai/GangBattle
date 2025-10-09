using System.Collections.Generic;
using UnityEngine;

public static class RangeFinder
{
    public static List<Vector3Int> GetSquareRange(Vector3Int center, int range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                tiles.Add(center + new Vector3Int(x, y, 0));
            }
        }
        return tiles;
    }

    public static List<Vector3Int> GetDiamondRange(Vector3Int center, int range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    tiles.Add(center + new Vector3Int(x, y, 0));
                }
            }
        }
        return tiles;
    }

    public static List<Vector3Int> GetCrossRange(Vector3Int center, int range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        for (int i = -range; i <= range; i++)
        {
            if (i == 0) continue;
            tiles.Add(center + new Vector3Int(i, 0, 0));
            tiles.Add(center + new Vector3Int(0, i, 0));
        }
        tiles.Add(center);
        return tiles;
    }

    public static List<Vector3Int> GetLineRange(Vector3Int center, int range)
    {
        // For now, Line is the same as Cross. This can be expanded later.
        return GetCrossRange(center, range);
    }

    public static List<Vector3Int> GetCircleRange(Vector3Int center, int range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        float rangeSqr = range * range;
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x * x + y * y <= rangeSqr)
                {
                    tiles.Add(center + new Vector3Int(x, y, 0));
                }
            }
        }
        return tiles;
    }
}