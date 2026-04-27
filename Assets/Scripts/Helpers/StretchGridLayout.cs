using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class StretchGridLayout : MonoBehaviour
{
    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void RecalculateStretch()
    {
        // 1. Count active children to find row count
        int activeChildren = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) activeChildren++;
        }

        // Divide by 2 because you have 2 columns
        int rowCount = Mathf.CeilToInt((float)activeChildren / 2f);
        if (rowCount == 0) return;

        // 2. Calculate available vertical space
        float containerHeight = rectTransform.rect.height;
        float topBottomPadding = grid.padding.top + grid.padding.bottom;
        float totalSpacing = grid.spacing.y * (rowCount - 1);

        float availableHeight = containerHeight - topBottomPadding - totalSpacing;

        // 3. Set the new cell height
        float rowHeight = availableHeight / rowCount;
        grid.cellSize = new Vector2(grid.cellSize.x, rowHeight);
    }
}