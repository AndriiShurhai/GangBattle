using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Canvas canvas;

    private AbilityBaseSO ability;
    private Unit caster;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;

    private GameObject dragVisual;
    private bool isDragging = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (canvas == null)
        {
            canvas = gameObject.GetComponentInParent<Canvas>();
        }
    }

    public void Setup(AbilityBaseSO abilityData, Unit unitCaster)
    {
        ability = abilityData;
        caster = unitCaster;
        abilityIcon.sprite = abilityData.abilityIcon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ability == null || caster == null) return;

        Debug.Log("Start dragging");

        isDragging = true;
        originalPosition = rectTransform.position;
        originalParent = rectTransform.parent;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;


    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        rectTransform.position = eventData.position;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector3Int gridPosition = GridManager.Instance.WorldToGrid(worldPoint);


    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        rectTransform.position = originalPosition;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector3Int targetPosition = GridManager.Instance.WorldToGrid(worldPoint);

        if (ability.IsValidTarget(caster.GridPosition, targetPosition, caster))
        {
            caster.UseAbility(ability, targetPosition);
        }
        else
        {
            Debug.Log("Invalid target position for ability");
        }

    }
}
