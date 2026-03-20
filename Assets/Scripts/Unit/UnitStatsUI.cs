using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UnitStatsUI : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject unitObject;

    [Header("Panel References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject headerBackgroundPanel;
    [SerializeField] private GameObject statsBackgroundPanel;
    [SerializeField] private GameObject abilitiesPanel;

    [Header("Unit Identity")]
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private GameObject unitIconBackgroundImage;
    [SerializeField] private GameObject unitIconPrefab;
    [SerializeField] private Transform unitIconParentTransform;
    [SerializeField] private Transform unitIconTemplateTransform;

    [Header("Stat Labels")]
    [SerializeField] private TextMeshProUGUI intelligenceTextStat;
    [SerializeField] private TextMeshProUGUI strengthTextStat;
    [SerializeField] private TextMeshProUGUI agilityTextStat;
    [SerializeField] private TextMeshProUGUI behaviourTypeTextStat;

    [Header("Stat Values")]
    [SerializeField] private Image hpBarFillImage;
    [SerializeField] private TextMeshProUGUI hpAmountText;
    [SerializeField] private TextMeshProUGUI intelligenceAmountText;
    [SerializeField] private TextMeshProUGUI strengthAmountText;
    [SerializeField] private TextMeshProUGUI agilityAmountText;
    [SerializeField] private TextMeshProUGUI behaviourTypeText;

    [Header("Abilities")]
    [SerializeField] private List<GameObject> abilityIcons;
    [SerializeField] private AbilityInfoUI abilityInfoDisplay;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hideDuration = 0.2f;
    [SerializeField] private float hpBarAnimDuration = 0.6f;
    [SerializeField] private float staggerDelay = 0.06f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hpBarCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool IsVisible => isVisible; 

    private int currentHealth;
    private int maxHealth;
    private int intelligence;
    private int strength;
    private int agility;
    private string behaviourType;
    private List<AbilityBaseSO> abilities;
    private GameObject currentUnitIcon;

    private CanvasGroup mainCanvasGroup;
    private Coroutine visibilityCoroutine;
    private Coroutine hpBarCoroutine;
    private bool isVisible;

    private GameObject[] animatedPanels;

    private void Awake()
    {
        mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
            mainCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

        animatedPanels = new[] { headerBackgroundPanel, statsBackgroundPanel, abilitiesPanel };

        foreach (var panel in animatedPanels)
            if (panel != null && panel.GetComponent<CanvasGroup>() == null)
                panel.AddComponent<CanvasGroup>();

        mainPanel.SetActive(false);
        isVisible = false;
    }

    private void OnEnable()
    {
        if (unit != null)
            SubscribeToUnitEvents();
    }

    private void OnDisable()
    {
        if (unit != null)
            UnsubscribeFromUnitEvents();
    }

    public void ShowForUnit(Unit targetUnit, GameObject targetObject = null)
    {
        if (unit != null)
            UnsubscribeFromUnitEvents();

        abilityInfoDisplay?.ForceHide();

        unit = targetUnit;
        unitObject = targetObject != null ? targetObject : targetUnit.gameObject;

        SubscribeToUnitEvents();
        RefreshAllUI();
        Show();
    }

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        mainPanel.SetActive(true);

        if (visibilityCoroutine != null) StopCoroutine(visibilityCoroutine);
        visibilityCoroutine = StartCoroutine(AnimateShow());
    }

    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;

        abilityInfoDisplay?.ForceHide();

        if (visibilityCoroutine != null) StopCoroutine(visibilityCoroutine);
        visibilityCoroutine = StartCoroutine(AnimateHide());
    }

    public void Toggle() { if (isVisible) Hide(); else Show(); }

    public void UpdateHealth(int newCurrent, int newMax)
    {
        currentHealth = newCurrent;
        maxHealth = newMax;
        AnimateHPBar(currentHealth, maxHealth);
        hpAmountText.text = $"{currentHealth} / {maxHealth}";
    }

    private void InitializeFromUnit()
    {
        currentHealth = unit.CurrentHealth;
        maxHealth = unit.MaxHealth;
        intelligence = unit.Intelligence;
        strength = unit.Strength;
        agility = unit.Agility;
        abilities = unit.Abilities;
        unitIconPrefab = unit.ClassIcon;
        Debug.Log("UnitStatsUI: Initialized unit icon prefab from unit class icon.");
        Debug.Log($"UnitStatsUI: Unit has {abilities.Count} abilities.");
        Debug.Log($"UnitStastUIl Unit currentHealth is {unit.CurrentHealth}");
        Debug.Log($"UnitStatsUI: Unit maxHealth is {unit.MaxHealth}");
        Debug.Log($"UnitStatsUI: Unit intelligence is {unit.Intelligence}");
        Debug.Log($"UnitStatsUI: Unit strength is {unit.Strength}");
        Debug.Log($"UnitStatsUI: Unit agility is {unit.Agility}");


        var brain = unitObject.GetComponent<AIBrain>();
        behaviourType = brain != null && brain.Personality != null ? brain.Personality?.ToString() : "SIMPLE";
    }

    public void Initialize(GameObject unit)
    {
        this.unitObject = unit;
        this.unit = unit.GetComponent<Unit>();
        if (this.unit == null)
        {
            Debug.LogError($"UnitStatsUI on {gameObject.name} was given an object without a Unit component.");
            return;
        }
        InitializeFromUnit();
    }

    private void RefreshAllUI()
    {
        if (unitNameText != null)
            unitNameText.text = unit.UnitName;

        SetStatText(intelligenceAmountText, intelligence);
        SetStatText(strengthAmountText, strength);
        SetStatText(agilityAmountText, agility);
        if (behaviourTypeText != null)
            behaviourTypeText.text = behaviourType;

        float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        hpBarFillImage.fillAmount = ratio;
        hpAmountText.text = $"{currentHealth} / {maxHealth}";

        RefreshAbilityIcons();

        RefreshUnitIcon();
    }

    private void RefreshUnitIcon()
    {
        if (unitIconPrefab == null || unitIconParentTransform == null || unitIconTemplateTransform == null) return;

        unitIconTemplateTransform.gameObject.SetActive(true);

        Destroy(currentUnitIcon);
        currentUnitIcon = Instantiate(unitIconPrefab, unitIconParentTransform);

        currentUnitIcon.transform.localPosition = unitIconTemplateTransform.localPosition;
        currentUnitIcon.transform.localScale = unitIconTemplateTransform.localScale;
        currentUnitIcon.transform.localRotation = unitIconTemplateTransform.localRotation;

        unitIconTemplateTransform.gameObject.SetActive(false);
    }

    private void RefreshAbilityIcons()
    {
        if (abilityIcons == null || abilities == null) return;

        for (int i = 0; i < abilityIcons.Count; i++)
        {
            bool hasAbility = i < abilities.Count && abilities[i] != null;
            abilityIcons[i].SetActive(hasAbility);

            if (!hasAbility) continue;

            var img = abilityIcons[i].GetComponentInChildren<Image>();
            if (img != null && abilities[i].AbilityIcon != null)
                img.sprite = abilities[i].AbilityIcon;

            var label = abilityIcons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = abilities[i].AbilityName;

            if (abilityInfoDisplay == null) continue;

            var clickHandler = abilityIcons[i].GetComponent<AbilityIconUI>();
            if (clickHandler == null)
                clickHandler = abilityIcons[i].AddComponent<AbilityIconUI>();

            clickHandler.Initialize(abilities[i], abilityInfoDisplay);
        }
    }
    private void SubscribeToUnitEvents()
    {
        Unit.OnAnyUnitTookDamage += OnHealthChanged;
        Unit.OnAnyUnitHealed += OnHealthChanged;
        Unit.OnAnyUnitDied += OnUnitDied;
    }

    private void UnsubscribeFromUnitEvents()
    {
        Unit.OnAnyUnitTookDamage -= OnHealthChanged;
        Unit.OnAnyUnitHealed -= OnHealthChanged;
        Unit.OnAnyUnitDied -= OnUnitDied;
    }

    private void OnHealthChanged(Unit unit, int newCurrent, int newMax)
    {
        if (unit == null || unit != this.unit) return;
        UpdateHealth(newCurrent, newMax);
    }

    private void OnUnitDied(Unit unit)
    {
        if (unit == null || unit != this.unit) return;
        UpdateHealth(0, maxHealth);
        StartCoroutine(HideAfterDelay(1.2f));
    }

    private IEnumerator AnimateShow()
    {
        mainCanvasGroup.alpha = 0f;
        foreach (var panel in animatedPanels)
        {
            if (panel == null) continue;
            var cg = panel.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            panel.transform.localScale = new Vector3(1f, 0.85f, 1f);
        }

        yield return FadeCanvasGroup(mainCanvasGroup, 0f, 1f, showDuration * 0.4f);

        for (int i = 0; i < animatedPanels.Length; i++)
        {
            if (animatedPanels[i] == null) continue;

            if (i > 0) yield return new WaitForSeconds(staggerDelay);

            StartCoroutine(RevealPanel(animatedPanels[i], showDuration));
        }

        yield return new WaitForSeconds(showDuration * 0.5f);
        float targetRatio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        AnimateHPBar(targetRatio);
    }

    private IEnumerator AnimateHide()
    {
        for (int i = animatedPanels.Length - 1; i >= 0; i--)
        {
            if (animatedPanels[i] == null) continue;
            StartCoroutine(CollapsePanel(animatedPanels[i], hideDuration * 0.8f));
            yield return new WaitForSeconds(staggerDelay * 0.5f);
        }

        yield return FadeCanvasGroup(mainCanvasGroup, 1f, 0f, hideDuration);
        mainPanel.SetActive(false);
    }

    private IEnumerator RevealPanel(GameObject panel, float duration)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        var t = panel.transform;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(1f, 0.85f, 1f);
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = showCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            cg.alpha = progress;
            t.localScale = Vector3.LerpUnclamped(startScale, endScale, progress);
            yield return null;
        }

        cg.alpha = 1f;
        t.localScale = Vector3.one;
    }

    private IEnumerator CollapsePanel(GameObject panel, float duration)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        var t = panel.transform;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            cg.alpha = 1f - progress;
            t.localScale = Vector3.LerpUnclamped(Vector3.one, new Vector3(1f, 0.85f, 1f), progress);
            yield return null;
        }

        cg.alpha = 0f;
    }

    private void AnimateHPBar(float targetRatio)
    {
        if (hpBarCoroutine != null) StopCoroutine(hpBarCoroutine);
        hpBarCoroutine = StartCoroutine(AnimateHPBarCoroutine(targetRatio));
    }
    private void AnimateHPBar(int current, int max)
        => AnimateHPBar(max > 0 ? (float)current / max : 0f);

    private IEnumerator AnimateHPBarCoroutine(float targetRatio)
    {
        float startRatio = hpBarFillImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < hpBarAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = hpBarCurve.Evaluate(Mathf.Clamp01(elapsed / hpBarAnimDuration));
            hpBarFillImage.fillAmount = Mathf.Lerp(startRatio, targetRatio, t);

            yield return null;
        }

        hpBarFillImage.fillAmount = targetRatio;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    private static void SetStatText(TextMeshProUGUI label, int value)
    {
        if (label != null) label.text = value.ToString();
    }
}