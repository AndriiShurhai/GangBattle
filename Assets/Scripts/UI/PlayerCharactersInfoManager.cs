using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerCharacterInfoManager : MonoBehaviour
{
    [SerializeField] private GameObject unitIconsPanel;
    [SerializeField] private UnitStatsUI unitStats;
    [SerializeField] private List<GameObject> teamIconsUI;
    [SerializeField] private List<GameObject> unitIconsPlaceHolders;

    private void Start()
    {
        TurnManager.Instance.OnUnitsInitialized += SetupUnits;
    }

    public void SetupUnits()
    {
        List<Unit> playerUnits = TurnManager.Instance.GetPlayerUnits();

        for (int i = 0; i < teamIconsUI.Count; i++)
        {
            GameObject teamUnitIcon = Instantiate(playerUnits[i].ClassIcon, teamIconsUI[i].transform.parent);
            teamUnitIcon.transform.localPosition = teamIconsUI[i].transform.localPosition;
            teamUnitIcon.transform.localScale = teamIconsUI[i].transform.localScale;
            teamUnitIcon.transform.localRotation = teamIconsUI[i].transform.localRotation;
            teamIconsUI[i].SetActive(false);

            GameObject unitIcon = Instantiate(playerUnits[i].ClassIcon, unitIconsPlaceHolders[i].transform.parent);
            unitIcon.transform.localPosition = unitIconsPlaceHolders[i].transform.localPosition;
            unitIcon.transform.localScale = unitIconsPlaceHolders[i].transform.localScale;
            unitIcon.transform.localRotation = unitIconsPlaceHolders[i].transform.localRotation;
            unitIcon.gameObject.AddComponent<UnitIconInfoUI>();
            unitIcon.GetComponent<UnitIconInfoUI>().Initialize(playerUnits[i], unitStats);
            unitIconsPlaceHolders[i].SetActive(false);
        }

        unitIconsPanel.SetActive(false);
    }

    public void Toggle()
    {
        unitIconsPanel.SetActive(!unitIconsPanel.activeSelf);
    }

    public void ShowUnitInfo(Unit unit)
    {
        unitStats.Initialize(unit.gameObject);
        unitStats.ShowForUnit(unit);
    }
}