using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfoUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject headerPanel;
    [SerializeField] private GameObject detailsPanel;

    [Header("Ability Identity")]
    [SerializeField] private Image abilityIconImage;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescriptionText;

    [Header("Stat Values")]
    [SerializeField] private TextMeshProUGUI rangeAmountText;
    [SerializeField] private TextMeshProUGUI rangeTypeText;
    [SerializeField] private TextMeshProUGUI targetTypeText;
    [SerializeField] private TextMeshProUGUI maxUsesText;
    [SerializeField] private TextMeshProUGUI scalingStatText;
    [SerializeField] private TextMeshProUGUI coefficientText;
    [SerializeField] private TextMeshProUGUI powerText;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.25f;
    [SerializeField] private float hideDuration = 0.18f;
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool IsVisible => isVisible;

    private CanvasGroup mainCanvasGroup;
    private GameObject[] animatedPanels;
    private Coroutine visibilityCoroutine;
    private Coroutine swapCoroutine;
    private AbilityBaseSO currentAbility;
    private bool isVisible;
    private Unit currentUnit;

    private void Awake()
    {
        mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
            mainCanvasGroup = mainPanel.AddComponent<CanvasGroup>();

        animatedPanels = new[] { headerPanel, detailsPanel };

        foreach (var panel in animatedPanels)
            if (panel != null && panel.GetComponent<CanvasGroup>() == null)
                panel.AddComponent<CanvasGroup>();

        mainPanel.SetActive(false);
        isVisible = false;
    }
    public void ShowForAbility(AbilityBaseSO ability, Unit currentUnit)
    {
        if (isVisible && currentAbility == ability)
        {
            Hide();
            return;
        }

        if (isVisible)
        {
            currentAbility = ability;
            if (swapCoroutine != null) StopCoroutine(swapCoroutine);
            swapCoroutine = StartCoroutine(AnimateSwap());
            return;
        }

        currentAbility = ability;
        this.currentUnit = currentUnit;
        RefreshUI();
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

        if (swapCoroutine != null) { StopCoroutine(swapCoroutine); swapCoroutine = null; }
        if (visibilityCoroutine != null) StopCoroutine(visibilityCoroutine);
        visibilityCoroutine = StartCoroutine(AnimateHide());
    }
    public void ForceHide()
    {
        if (swapCoroutine != null) { StopCoroutine(swapCoroutine); swapCoroutine = null; }
        if (visibilityCoroutine != null) { StopCoroutine(visibilityCoroutine); visibilityCoroutine = null; }

        currentAbility = null;
        isVisible = false;
        mainPanel.SetActive(false);
    }

    public void Toggle() { if (isVisible) Hide(); else Show(); }

    private void RefreshUI()
    {
        if (currentAbility == null) return;

        if (abilityIconImage != null && currentAbility.AbilityIcon != null)
            abilityIconImage.sprite = currentAbility.AbilityIcon;

        SetText(abilityNameText, currentAbility.AbilityName);
        SetText(abilityDescriptionText, currentAbility.AbilityDescription);

        SetText(rangeAmountText, currentAbility.Range.ToString());
        SetText(rangeTypeText, FormatEnum(currentAbility.TypeOfRange.ToString()));
        SetText(targetTypeText, FormatEnum(currentAbility.TypeOfTarget.ToString()));
        SetText(maxUsesText, currentAbility.MaxUses.ToString());
        SetText(powerText, currentAbility.GetPower(currentUnit).ToString());

        bool hasScaling = currentAbility.TypeOfScaling != StatType.None;
        SetText(scalingStatText, hasScaling ? FormatEnum(currentAbility.TypeOfScaling.ToString()) : "—");
        SetText(coefficientText, hasScaling ? $"×{currentAbility.Coefficient:0.##}" : "—");

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
    private IEnumerator AnimateSwap()
    {
        float halfDuration = showDuration * 0.35f;

        foreach (var panel in animatedPanels)
        {
            if (panel != null)
                StartCoroutine(FadeCanvasGroup(panel.GetComponent<CanvasGroup>(), 1f, 0f, halfDuration));
        }
        yield return new WaitForSeconds(halfDuration);

        RefreshUI();

        foreach (var panel in animatedPanels)
        {
            if (panel != null)
                StartCoroutine(FadeCanvasGroup(panel.GetComponent<CanvasGroup>(), 0f, 1f, halfDuration));
        }
        yield return new WaitForSeconds(halfDuration);

        swapCoroutine = null;
    }

    private IEnumerator RevealPanel(GameObject panel, float duration)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        var t = panel.transform;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(1f, 0.85f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = showCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            cg.alpha = progress;
            t.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, progress);
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
    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label != null) label.text = value;
    }
    private static string FormatEnum(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < raw.Length; i++)
        {
            if (i > 0 && char.IsUpper(raw[i]))
                sb.Append(' ');
            sb.Append(raw[i]);
        }
        return sb.ToString();
    }
}