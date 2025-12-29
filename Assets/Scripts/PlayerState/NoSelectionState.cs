using UnityEngine;

public class NoSelectionState : IPlayerState
{
    private readonly CharacterSelectionController _controller;

    public NoSelectionState(CharacterSelectionController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _controller.ClearSelection();
        AbilityTargetingVisualizer.Instance.HideAbilityRange();
        Debug.Log("Entering no selection state");
    }
    public void Exit()
    {

    }
    public void OnClick(Vector3Int gridPosition)
    {
        IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);

        if (clickedObject is Unit unit)
        {
            _controller.SelectUnit(unit);
        }
    }
}
