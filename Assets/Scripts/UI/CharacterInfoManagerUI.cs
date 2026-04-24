using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterInfoManagerUI : MonoBehaviour
{
    [SerializeField] private UnitStatsUI statsDisplay;
    [SerializeField] private AbilityInfoUI abilityStatsDisplay;
    private void Start()
    {
        if (statsDisplay == null)
        {
            Debug.LogError("UnitStatsDisplay reference is missing!");
            return;
        }

        GameInput.Instance.OnClickAction += GameInput_OnClickAction;
    }

    public void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnClickAction -= GameInput_OnClickAction;
        }
    }
    private void GameInput_OnClickAction(Vector2 mousePosition)
    {
        bool overUI = IsPointerOverUI(mousePosition);

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3Int gridPosition = GridManager.Instance.WorldToGrid(worldPoint);

        IGridObject clickedObject = GridObjectRegistry.Instance.GetObjectAt(gridPosition);

        Debug.Log($"Clicked at world position: {GameInput.Instance.GetMousePosition()}, grid position: {GridManager.Instance.WorldToGrid(GameInput.Instance.GetMousePosition())}");

        if (clickedObject != null && clickedObject is Unit unit && unit.UnitFaction == Faction.Enemy)
        {
            Debug.Log($"Clicked on unit: {unit.name}");
            statsDisplay.Initialize(unit.gameObject);
            statsDisplay.Hide();
            abilityStatsDisplay.Hide();
            statsDisplay.ShowForUnit(unit);
        }
        else if (!overUI)
        {
            Debug.Log("Clicked on empty space or non-unit object.");
            if (abilityStatsDisplay.IsVisible)
            {
                abilityStatsDisplay.Hide();
            }
            else
            {
                statsDisplay.Hide();
            }
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}