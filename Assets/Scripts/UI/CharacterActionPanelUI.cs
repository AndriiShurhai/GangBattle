using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharacterActionPanelUI : MonoBehaviour
{
    public static CharacterActionPanelUI Instance { get; private set; }

    [SerializeField] private GameObject abilityButtonPrefab;
    [SerializeField] private Transform actionButtonsContainer;

    private List<GameObject> activeButtons = new List<GameObject>();
    private Unit currentUnit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowAbilitiesForUnit(Unit unit)
    {
        ClearAbilityButtons();
        currentUnit = unit;

        if (unit == null || unit.Abilities.Count == 0) return;

        foreach (AbilityBaseSO ability in unit.Abilities)
        {
            CreateAbilityButton(ability, unit);
        }
    }

    public void HideAbilityPanel()
    {
        ClearAbilityButtons();
        currentUnit = null;
    }

    private void CreateAbilityButton(AbilityBaseSO ability, Unit caster)
    {
        GameObject buttonObj = Instantiate(abilityButtonPrefab, actionButtonsContainer);

        AbilityButton abilityButton = buttonObj.GetComponent<AbilityButton>();

        if (abilityButton != null)
        {
            abilityButton.Setup(ability, caster);
        }

        buttonObj.SetActive(true);
        activeButtons.Add(buttonObj);
    }

    private void ClearAbilityButtons()
    {
        foreach (GameObject button in activeButtons)
        {
            if (button != null) Destroy(button);
        }
        activeButtons.Clear();
    }
}
