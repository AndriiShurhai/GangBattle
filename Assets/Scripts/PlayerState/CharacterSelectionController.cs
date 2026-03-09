using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectionController : MonoBehaviour
{
    private Camera mainCamera;
    private Unit selectedUnit;

    private IPlayerState currentState;

    public NoSelectionState noSelectionState;

    private void Awake()
    {
        mainCamera = Camera.main;
        noSelectionState = new NoSelectionState(this);
    }
    private void Start()
    {
        ChangeState(noSelectionState);
    }

    private void Update()
    {
        currentState?.Update();
    }
    private void OnEnable()
    {
        GameInput.Instance.OnClickAction += GameInput_OnClickAction;
    }
    private void OnDisable()
    {
        GameInput.Instance.OnClickAction -= GameInput_OnClickAction;
    }
    private void GameInput_OnClickAction(Vector2 mousePosition)
    {
        Debug.Log("Clicked action");
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector3Int gridPosition = GridManager.Instance.WorldToGrid(worldPoint);

        currentState?.OnClick(gridPosition);
    }
    public void ChangeState(IPlayerState state)
    {
        currentState?.Exit();
        currentState = state;
        currentState?.Enter();
    }

    public void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        ChangeState(new UnitSelectedState(this, unit));
    }


    public void ClearSelection()
    {
        GridVisualizer.Instance.ClearHighlights();
        CharacterActionPanelUI.Instance.HideAbilityPanel();
        selectedUnit = null;
    }
}
