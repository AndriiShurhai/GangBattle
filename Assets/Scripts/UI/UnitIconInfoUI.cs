using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class UnitIconInfoUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UnitStatsUI statsDisplay;
    [SerializeField] private Unit unit;

    public void Initialize(Unit unit, UnitStatsUI statsDisplay)
    {
        this.unit = unit;
        this.statsDisplay = statsDisplay;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        statsDisplay.Initialize(unit.gameObject);
        statsDisplay.ShowForUnit(unit);
    }
}