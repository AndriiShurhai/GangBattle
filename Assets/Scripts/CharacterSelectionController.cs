using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectionController : MonoBehaviour
{
    private Camera mainCamera;
    private Unit selectedUnit;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void Start()
    {
        GameInput.Instance.OnClickAction += GameInput_OnClickAction;
    }

    private void GameInput_OnClickAction(Vector2 mousePosition)
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(worldPoint);

        if (hit)
        {
            Vector3Int gridPosition = GridManager.Instance.WorldToGrid(worldPoint);

            if (selectedUnit != null && selectedUnit.CanMoveTo(gridPosition))
            {
                selectedUnit.MoveTo(gridPosition);
                GridVisualizer.Instance.ClearHighlights();
                selectedUnit = null;
            }
            else if (selectedUnit != null && !selectedUnit.CanMoveTo(gridPosition))
            {
                Debug.Log("Clearing highlights");
                GridVisualizer.Instance.ClearHighlights();
                IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);
                if (clickedObject is Unit unit)
                {
                    SelectUnit(unit);
                }
            }
            else
            {
                IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);
                if (clickedObject is Unit unit)
                {
                    SelectUnit(unit);
                }
            }
        }
        else
        {
            GridVisualizer.Instance.ClearHighlights();
        }
    }

    private void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        selectedUnit.Select();
        GridVisualizer.Instance.ShowMovementRange(unit.GridPosition, unit.MovementRange, GridManager.Instance.IsValidPosition);

        CharacterActionPanelUI.Instance.SetCharacterActionsPanel(selectedUnit.ActionsSO);
    }

}
