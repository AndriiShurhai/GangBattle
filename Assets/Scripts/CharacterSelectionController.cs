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

    private void OnDestroy()
    {
        GameInput.Instance.OnClickAction -= GameInput_OnClickAction;    
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
                ClearSelection();
            }
            else if (selectedUnit != null && !selectedUnit.CanMoveTo(gridPosition))
            {
                IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);
                if (clickedObject is Unit unit)
                {
                    SelectUnit(unit);
                }
                else
                {
                    ClearSelection();
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
            ClearSelection();
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (selectedUnit != null)
        {
            ClearSelection();
        }

        selectedUnit = unit;
        selectedUnit.Select();

        GridVisualizer.Instance.ShowMovementRange(unit.GridPosition, unit.MovementRange, GridManager.Instance.IsValidPosition);

        CharacterActionPanelUI.Instance.ShowAbilitiesForUnit(unit); 
    }


    private void ClearSelection()
    {
        GridVisualizer.Instance.ClearHighlights();
        CharacterActionPanelUI.Instance.HideAbilityPanel();
        selectedUnit = null;
    }
}
