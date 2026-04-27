using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;


public class AbilityIconUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AbilityBaseSO ability;
    [SerializeField] private AbilityInfoUI infoDisplay;
    [SerializeField] private CanvasGroup infoCanvasGroup;

    private Unit currentUnit;
    public void Initialize(AbilityBaseSO ability, AbilityInfoUI infoDisplay, Unit currentUnit)
    {
        this.ability = ability;
        this.infoDisplay = infoDisplay;
        this.currentUnit = currentUnit;

        Debug.Log($"AbilityIconUI: Initialized for ability '{ability.AbilityName}'");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ability == null || infoDisplay == null) return;

        Debug.Log($"AbilityIconUI: Clicked on ability '{ability.AbilityName}'");
        infoDisplay.ShowForAbility(ability, currentUnit);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ability == null || infoDisplay == null) return;
        CanvasGroup cg = transform.parent.transform.parent.GetComponentInChildren<CanvasGroup>();
        Debug.Log(cg != null
            ? $"AbilityIconUI: Found CanvasGroup for ability '{cg.name}' info display."
            : $"AbilityIconUI: No CanvasGroup found for ability '{ability.AbilityName}' info display.");
        if (cg != null)
        {
            Debug.Log($"AbilityIconUI: Pointer entered on ability '{ability.AbilityName}', fading in info display.");
            cg.DOFade(1f, 0.4f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ability == null || infoDisplay == null) return;
        CanvasGroup cg = transform.parent.transform.parent.GetComponentInChildren<CanvasGroup>();
        if (cg != null)
        {
            Debug.Log($"AbilityIconUI: Pointer exited from ability '{ability.AbilityName}', fading out info display.");
            cg.DOFade(0f, 0.4f);
        }
    }
}