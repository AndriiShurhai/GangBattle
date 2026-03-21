using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCharacterInfoManager : MonoBehaviour
{
    [SerializeField] private GameObject unitIconsPanel;
    [SerializeField] private UnitStatsUI unitStats;
    [SerializeField] private List<GameObject> teamIconsUI;
    [SerializeField] private List<GameObject> unitIconsPlaceHolders;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup panelCanvasGroup;
    private Coroutine toggleCoroutine;
    private Vector3 panelOriginalScale;

    private void Awake()
    {
        TurnManager.OnUnitsInitialized += SetupUnits;

        panelOriginalScale = unitIconsPanel.transform.localScale;

        panelCanvasGroup = unitIconsPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = unitIconsPanel.AddComponent<CanvasGroup>();

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        unitIconsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        TurnManager.OnUnitsInitialized -= SetupUnits;
    }

    public void SetupUnits()
    {
        List<Unit> playerUnits = TurnManager.Instance.GetPlayerUnits();

        for (int i = 0; i < playerUnits.Count; i++)
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

        foreach (var template in teamIconsUI)
        {
            template.SetActive(false);  
        }

        foreach(var template in unitIconsPlaceHolders)
        {
            template.SetActive(false);
        }

        unitIconsPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (toggleCoroutine != null)
            StopCoroutine(toggleCoroutine);

        bool isOpen = unitIconsPanel.activeSelf && panelCanvasGroup.alpha > 0.5f;
        toggleCoroutine = StartCoroutine(isOpen ? AnimateOut() : AnimateIn());
    }

    private IEnumerator AnimateIn()
    {
        unitIconsPanel.SetActive(true);
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
        unitIconsPanel.transform.localScale = panelOriginalScale * 0.85f;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            panelCanvasGroup.alpha = scaleCurve.Evaluate(t);
            unitIconsPanel.transform.localScale = Vector3.LerpUnclamped(panelOriginalScale * 0.85f, panelOriginalScale, BounceOut(t));

            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
        unitIconsPanel.transform.localScale = panelOriginalScale;
    }

    private IEnumerator AnimateOut()
    {
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        Vector3 startScale = unitIconsPanel.transform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            panelCanvasGroup.alpha = 1f - t;
            unitIconsPanel.transform.localScale = Vector3.LerpUnclamped(startScale, panelOriginalScale * 0.85f, t);

            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
        unitIconsPanel.SetActive(false);
    }

    private float BounceOut(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }

    public void ShowUnitInfo(Unit unit)
    {
        unitStats.Initialize(unit.gameObject);
        unitStats.ShowForUnit(unit);
    }
}