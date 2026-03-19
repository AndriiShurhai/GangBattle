using UnityEngine;
using System.Collections;
using System;

public class CharacterInfoManagerUI : MonoBehaviour
{
    [SerializeField] private UnitStatsUI statsDisplay;
    private void Start()
    {
        if (statsDisplay == null)
        {
            Debug.LogError("UnitStatsDisplay reference is missing!");
            return;
        }

        GameInput.Instance.OnClickAction += GameInput_OnClickAction;
    }

    private void GameInput_OnClickAction(Vector2 mousePosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3Int gridPosition = GridManager.Instance.WorldToGrid(worldPoint);

        IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);

        Debug.Log($"Clicked at world position: {GameInput.Instance.GetMousePosition()}, grid position: {GridManager.Instance.WorldToGrid(GameInput.Instance.GetMousePosition())}");
        if (clickedObject != null && clickedObject is Unit unit && unit.UnitFaction == Faction.Enemy)
        {
            Debug.Log($"Clicked on unit: {unit.name}");
            statsDisplay.Initialize(unit.gameObject);
            statsDisplay.Hide();
            statsDisplay.ShowForUnit(unit); 
        }
        else
        {
            Debug.Log("Clicked on empty space or non-unit object.");
            statsDisplay.Hide();
        }
    }

    private void OnDestroy()
    {
    }
}