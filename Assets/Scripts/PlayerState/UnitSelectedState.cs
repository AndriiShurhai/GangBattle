using UnityEngine;

public class UnitSelectedState : IPlayerState
{
    private readonly CharacterSelectionController _controller;
    private readonly Unit _selectedUnit;

    public UnitSelectedState(CharacterSelectionController controller, Unit selectedUnit)
    {
        _controller = controller;
        _selectedUnit = selectedUnit;
    }
    public void Enter()
    {
        Debug.Log($"Entering unit selected state for {_selectedUnit.name}");

        if (_selectedUnit.UnitFaction == Faction.Player)
        {
            GridVisualizer.Instance.ShowMovementRange(_selectedUnit.GridPosition, _selectedUnit.MovementRange, GridManager.Instance.IsValidPosition);
            CharacterActionPanelUI.Instance.ShowAbilitiesForUnit(_selectedUnit);
        }
        else
        {
            Debug.Log("Enemy has been selected");
            GridVisualizer.Instance.ClearHighlights();
            CharacterActionPanelUI.Instance.HideAbilityPanel();
        }

    }

    public void Exit()
    {

    }

    public void OnClick(Vector3Int gridPosition)
    {
        if (_selectedUnit.CanMoveTo(gridPosition) && _selectedUnit.UnitFaction == Faction.Player)
        {
            _selectedUnit.MoveTo(gridPosition);
            _controller.ChangeState(_controller.noSelectionState);
        }

        else if (GridObjectRegistry.Instance.GetObjectAt(gridPosition) is Unit otherUnit && otherUnit != _selectedUnit)
        {
            _controller.SelectUnit(otherUnit);
        }

        else
        {
            _controller.ChangeState(_controller.noSelectionState);
        }

    }
}
