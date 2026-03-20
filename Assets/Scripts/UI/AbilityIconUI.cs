using UnityEngine;
using UnityEngine.EventSystems;


public class AbilityIconUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AbilityBaseSO ability;
    [SerializeField] private AbilityInfoUI infoDisplay;
    public void Initialize(AbilityBaseSO ability, AbilityInfoUI infoDisplay)
    {
        this.ability = ability;
        this.infoDisplay = infoDisplay;

        Debug.Log($"AbilityIconUI: Initialized for ability '{ability.AbilityName}'");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ability == null || infoDisplay == null) return;

        Debug.Log($"AbilityIconUI: Clicked on ability '{ability.AbilityName}'");
        infoDisplay.ShowForAbility(ability);
    }
}