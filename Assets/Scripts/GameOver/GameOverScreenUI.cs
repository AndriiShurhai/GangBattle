using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreenUI : MonoBehaviour
{
    public static GameOverScreenUI Instance { get; private set; }

    [Header("Root Panels")]
    [SerializeField] private CanvasGroup _screenOverlay;
    [SerializeField] private GameObject _gameOverResultPanel;
    [SerializeField] private RectTransform _levelCompletedPanel;
    [SerializeField] private RectTransform _levelFailedPanel;

    [Header("Win — Stat Texts")]
    [SerializeField] private TextMeshProUGUI _levelCompletedText;
    [SerializeField] private TextMeshProUGUI _enemiesDestroyedText;
    [SerializeField] private TextMeshProUGUI _unitsAliveText;
    [SerializeField] private TextMeshProUGUI _timeTakenText;

    [Header("Win — Star Images")]
    [SerializeField] private RectTransform _enemiesDestroyedStarRect;
    [SerializeField] private RectTransform _unitsAliveStarRect;
    [SerializeField] private RectTransform _timeTakenStarRect;
    [SerializeField] private Image _enemiesDestroyedStarImage;
    [SerializeField] private Image _unitsAliveStarImage;
    [SerializeField] private Image _timeTakenStarImage;

    [Header("Star Colours")]
    [SerializeField] private Color _gainedStarColor = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color _lockedStarColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Fail Panel")]
    [SerializeField] private TextMeshProUGUI _levelFailedText;
    [SerializeField] private RectTransform _failedKnightRect;

    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private CanvasGroup _buttonsGroup;

    [Header("Animation Tuning")]
    [SerializeField] private float _overlayFadeDuration = 0.4f;
    [SerializeField] private float _panelSlideDuration = 0.5f;
    [SerializeField] private float _panelSlideOffset = 80f;
    [SerializeField] private float _countUpDuration = 0.8f;
    [SerializeField] private float _statStaggerDelay = 0.25f;
    [SerializeField] private float _starPopDuration = 0.35f;
    [SerializeField] private int _shakeCount = 8;
    [SerializeField] private float _shakeMagnitude = 18f;

    [Header("Audio")]
    [SerializeField] private string _sfxStarEarned = "sfx_star_earned";
    [SerializeField] private string _sfxCountUp = "sfx_count";

    private Vector2 _knightOriginalPos;

    private static readonly AnimationCurve _easeOutBack = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2.8f),
        new Keyframe(0.75f, 1.1f, 1.2f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _restartButton.onClick.AddListener(RestartLevel);
        _continueButton.onClick.AddListener(ContinueToLevelSelection);

        if (_screenOverlay != null)
        {
            _screenOverlay.alpha = 0f;
            _screenOverlay.blocksRaycasts = false;
        }

        if (_gameOverResultPanel != null) _gameOverResultPanel.SetActive(false);

        if (_buttonsGroup != null)
        {
            _buttonsGroup.alpha = 0f;
            _buttonsGroup.interactable = false;
            _buttonsGroup.blocksRaycasts = false;
        }

        if (_failedKnightRect != null)
            _knightOriginalPos = _failedKnightRect.anchoredPosition;
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(RestartLevel);
        _continueButton.onClick.RemoveListener(ContinueToLevelSelection);
    }
    public void ShowVictoryScreen(VictoryScreenData data)
    {
        StartCoroutine(AnimateVictory(data));
    }

    public void ShowDefeatScreen()
    {
        StartCoroutine(AnimateDefeat());
    }


    private IEnumerator AnimateVictory(VictoryScreenData d)
    {
        _gameOverResultPanel.SetActive(true);
        _levelCompletedPanel.gameObject.SetActive(true);
        _levelFailedPanel.gameObject.SetActive(false);

        SetStarScale(_enemiesDestroyedStarRect, 0f);
        SetStarScale(_unitsAliveStarRect, 0f);
        SetStarScale(_timeTakenStarRect, 0f);
        if (_buttonsGroup) _buttonsGroup.alpha = 0f;

        yield return StartCoroutine(FadeCanvasGroup(_screenOverlay, 0f, 0.75f, _overlayFadeDuration));
        yield return StartCoroutine(SlidePanel(_levelCompletedPanel, _panelSlideOffset, _panelSlideDuration));

        _enemiesDestroyedText.text = $"Enemies Destroyed\n0 / {d.TotalEnemies}";
        _unitsAliveText.text = $"Units Alive\n0 / {d.TotalPlayerUnits}";
        _timeTakenText.text = $"Time Taken\n0 / {d.BestTime}";
        _enemiesDestroyedText.transform.localScale = Vector3.zero;
        _unitsAliveText.transform.localScale = Vector3.zero;
        _timeTakenText.transform.localScale = Vector3.zero;

        yield return StartCoroutine(PopScale(_enemiesDestroyedText.rectTransform, _starPopDuration));
        yield return StartCoroutine(AnimateStatRow(
            _enemiesDestroyedText, _enemiesDestroyedStarRect, _enemiesDestroyedStarImage,
            "Enemies Destroyed", d.EnemiesDestroyed, d.TotalEnemies, d.StarEnemies));

        yield return new WaitForSeconds(_statStaggerDelay);

        yield return StartCoroutine(PopScale(_unitsAliveText.rectTransform, _starPopDuration));
        yield return StartCoroutine(AnimateStatRow(
            _unitsAliveText, _unitsAliveStarRect, _unitsAliveStarImage,
            "Units Alive", d.UnitsAlive, d.TotalPlayerUnits, d.StarUnits));

        yield return new WaitForSeconds(_statStaggerDelay);

        yield return StartCoroutine(PopScale(_timeTakenText.rectTransform, _starPopDuration));
        yield return StartCoroutine(AnimateStatRow(
            _timeTakenText, _timeTakenStarRect, _timeTakenStarImage,
            "Time Taken", d.TimeTaken, d.BestTime, d.StarTime));

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(FadeCanvasGroup(_buttonsGroup, 0f, 1f, 0.3f));
    }

    private IEnumerator AnimateDefeat()
    {
        _gameOverResultPanel.SetActive(true);
        _levelFailedPanel.gameObject.SetActive(true);
        _levelCompletedPanel.gameObject.SetActive(false);

        if (_buttonsGroup) _buttonsGroup.alpha = 0f;

        yield return StartCoroutine(FadeCanvasGroup(_screenOverlay, 0f, 0.75f, _overlayFadeDuration));
        yield return StartCoroutine(SlidePanel(_levelFailedPanel, _panelSlideOffset, _panelSlideDuration));

        if (_failedKnightRect != null)
            yield return StartCoroutine(ShakeRect(_failedKnightRect, _shakeCount, _shakeMagnitude));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeCanvasGroup(_buttonsGroup, 0f, 1f, 0.3f));
    }

    private IEnumerator AnimateStatRow(
        TextMeshProUGUI label,
        RectTransform starRect,
        Image starImage,
        string statName,
        int current,
        int target,
        bool earned)
    {
        float elapsed = 0f;
        int first = 0;
        while (elapsed < _countUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _countUpDuration;
            int display = Mathf.RoundToInt(Mathf.Lerp(0, current, t));
            label.text = $"{statName}\n{display} / {target}";
            if (display != first)
            {
                AudioManager.Instance?.PlaySFX(_sfxCountUp);
                first = display;
            }
            yield return null;
        }
        label.text = $"{statName}\n{current} / {target}";

        starImage.color = earned ? _gainedStarColor : _lockedStarColor;
       
        if (earned)
        {
            AudioManager.Instance?.PlaySFX(_sfxStarEarned);
        }

        yield return StartCoroutine(PopScale(starRect, _starPopDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        if (to > from) { group.interactable = true; group.blocksRaycasts = true; }

        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;

        if (to <= 0f) { group.interactable = false; group.blocksRaycasts = false; }
    }

    private IEnumerator SlidePanel(RectTransform panel, float yOffset, float duration)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>() ?? panel.gameObject.AddComponent<CanvasGroup>();

        Vector2 endPos = panel.anchoredPosition;
        Vector2 startPos = endPos - new Vector2(0, yOffset);

        cg.alpha = 0f;
        panel.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            cg.alpha = t;
            yield return null;
        }

        panel.anchoredPosition = endPos;
        cg.alpha = 1f;
    }

    private IEnumerator PopScale(RectTransform rect, float duration)
    {
        float elapsed = 0f;
        rect.localScale = Vector3.zero;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = _easeOutBack.Evaluate(Mathf.Clamp01(elapsed / duration));
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    private IEnumerator ShakeRect(RectTransform rect, int count, float magnitude)
    {
        float stepDuration = 0.06f;
        for (int i = 0; i < count; i++)
        {
            float sign = (i % 2 == 0) ? 1f : -1f;
            float decay = 1f - (float)i / count;
            float target = sign * magnitude * decay;

            float elapsed = 0f;
            float start = rect.anchoredPosition.x;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float x = Mathf.Lerp(start, _knightOriginalPos.x + target, elapsed / stepDuration);
                rect.anchoredPosition = new Vector2(x, _knightOriginalPos.y);
                yield return null;
            }
        }
        rect.anchoredPosition = _knightOriginalPos;
    }

    private static void SetStarScale(RectTransform rect, float scale)
    {
        if (rect != null) rect.localScale = Vector3.one * scale;
    }

    private void RestartLevel() => SceneLoader.Instance?.ReloadCurrentScene();
    private void ContinueToLevelSelection() => SceneLoader.Instance?.LoadScene("LevelSelection");
}