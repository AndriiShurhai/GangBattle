using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    /// <summary>Fired when the camera finishes zooming into a biome.</summary>
    public static event Action<BiomeController> OnBiomeZoomedIn;
    public static event Action<BiomeController> OnBiomeZoomingIn; 

    /// <summary>Fired when the camera finishes zooming back out to world view.</summary>
    public static event Action OnBiomeZoomedOut;
    public static event Action OnBiomeZoomingOut;

    [Header("Camera")]
    [Tooltip("Main 2D orthographic camera")]
    public Camera mainCamera;

    [Header("Zoom Settings")]
    [Tooltip("Padding multiplier around the biom bounds. 1.1 = 10% padding")]
    public float zoomPadding = 1f;

    [Tooltip("Duration of the zoom animation in seconds")]
    public float zoomDuration = 0.6f;

    [Tooltip("Animation curve for the zoom transition")]
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Biome Fade")]
    [Range(0f, 1f)]
    [Tooltip("Alpha of the biome sprite after zooming in")]
    public float biomeFadeAlpha = 0f;

    [Tooltip("Duration of the biome fade animation in seconds")]
    public float fadeDuration = 0.5f;

    [Header("UI")]
    [Tooltip("Button to return from biome view to world view")]
    public GameObject returnFromRegionButton;
    [Tooltip("Settings button")]
    public GameObject optionsButton;
    [Tooltip("Option Panel")]
    public GameObject optionsPanel;

    public enum ZoomState { World, Transitioning, Biome }
    public ZoomState CurrentState { get; private set; } = ZoomState.World;
    public BiomeController ActiveBiome => activeBiome;

    private BiomeController activeBiome;

    private Vector3 worldCameraPosition;
    private float worldOrthographicSize;
    private Coroutine transition;

    private Button _returnButton;
    private Image _returnButtonImage;

    private Button _optionsButton;
    private Image _optionsButtonImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;

        _returnButton = returnFromRegionButton.GetComponent<Button>();
        _returnButtonImage = returnFromRegionButton.GetComponent<Image>();

        _returnButton.onClick.AddListener(ZoomOut);
        _returnButton.interactable = false;
        
        _optionsButton = optionsButton.GetComponent<Button>();
        _optionsButtonImage = optionsButton.GetComponent<Image>();

        _optionsButton.onClick.AddListener(OpenOptions);
        _optionsButton.interactable = true;
    }

    private void Start()
    {
        worldCameraPosition = mainCamera.transform.position;
        worldOrthographicSize = mainCamera.orthographicSize;
    }
    private void OpenOptions()
    {
        optionsPanel.GetComponent<OptionsUI>().ToggleOptionsPanel();
    }

    public void RequestZoomToBiome(BiomeController biome)
    {
        if (CurrentState != ZoomState.World) return;

        activeBiome = biome;
        activeBiome.UpdateBounds();

        SetTransition(ZoomToBiomeRoutine());
    }

    public void ZoomOut()
    {
        if (CurrentState != ZoomState.Biome) return;
        SetTransition(ZoomOutRoutine());
    }

    private IEnumerator ZoomToBiomeRoutine()
    {
        CurrentState = ZoomState.Transitioning;
        OnBiomeZoomingIn?.Invoke(activeBiome);

        yield return StartCoroutine(LerpCamera(
            new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.worldBounds),
            zoomDuration
        ));

        SpriteRenderer regionRenderer = activeBiome.GetRegionSpriteRenderer();

        StartCoroutine(FadeSprite(regionRenderer, 1f, 0.3f));

        foreach (LevelNode level in activeBiome.regionController.GetLevelsInRegion())
        {
            if (!level.IsLocked)
            {
                level.GetComponent<BoxCollider2D>().enabled = true;
                if (level.GetCurrentStarsContainer() == null)
                {
                    Debug.LogWarning($"Level {level.LevelName} has no stars container assigned. Skipping fade-in for stars.");
                    StartCoroutine(FadeSprite(level.SpriteRenderer, 1f, fadeDuration));
                    continue;
                }
                foreach (SpriteRenderer star in level.GetCurrentStarsContainer().GetComponentsInChildren<SpriteRenderer>())
                    StartCoroutine(FadeSprite(star, 1f, fadeDuration));

                StartCoroutine(FadeSprite(level.SpriteRenderer, 1f, fadeDuration));
            }
            else
            {
                level.GetComponent<BoxCollider2D>().enabled = false;
            }

        }

        Coroutine regionZoom = StartCoroutine(LerpCamera(
            new Vector3(regionRenderer.bounds.center.x, regionRenderer.bounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(regionRenderer.bounds),
            zoomDuration
        ));

        _returnButton.interactable = true;
        _optionsButton.interactable = false;
        StartCoroutine(FadeImage(_returnButtonImage, 1f, fadeDuration));
        StartCoroutine(FadeImage(_optionsButtonImage, 0f, fadeDuration));
        optionsButton.SetActive(false);
        StartCoroutine(FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 0f, fadeDuration));

        yield return regionZoom;

        CurrentState = ZoomState.Biome;
        OnBiomeZoomedIn?.Invoke(activeBiome);
    }

    private IEnumerator ZoomOutRoutine()
    {
        CurrentState = ZoomState.Transitioning;

        foreach (LevelNode level in activeBiome.regionController.GetLevelsInRegion())
        {
            level.GetComponent<BoxCollider2D>().enabled = false;
            if (level.GetCurrentStarsContainer() == null)
            {
                yield return StartCoroutine(FadeSprite(level.SpriteRenderer, 0f, 0.2f));
                continue;
            }

            foreach (SpriteRenderer star in level.GetCurrentStarsContainer().GetComponentsInChildren<SpriteRenderer>())
                StartCoroutine(FadeSprite(star, 0f, 0.2f));

            yield return StartCoroutine(FadeSprite(level.SpriteRenderer, 0f, 0.2f));
        }

        OnBiomeZoomingOut?.Invoke();
        yield return StartCoroutine(LerpCamera(
            new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.worldBounds),
            zoomDuration
        ));

        StartCoroutine(FadeSprite(activeBiome.GetRegionSpriteRenderer(), 0f, 0.3f));

        _returnButton.interactable = false;
        _optionsButton.interactable = true;
        StartCoroutine(FadeImage(_returnButtonImage, 0f, fadeDuration));
        StartCoroutine(FadeImage(_optionsButtonImage, 1f, fadeDuration));
        optionsButton.SetActive(true);

        StartCoroutine(LerpCamera(worldCameraPosition, worldOrthographicSize, zoomDuration));
        yield return StartCoroutine(FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 1f, fadeDuration));

        activeBiome = null;
        CurrentState = ZoomState.World;
        //worldOrthographicSize = mainCamera.orthographicSize; // re-sync after return
        OnBiomeZoomedOut?.Invoke();
    }
    private IEnumerator LerpCamera(Vector3 targetPosition, float targetSize, float duration)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = zoomCurve.Evaluate(elapsed / duration);
            mainCamera.transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, t);
            mainCamera.orthographicSize = Mathf.LerpUnclamped(startSize, targetSize, t);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha, float duration)
    {
        Color start = sr.color;
        Color end = new Color(start.r, start.g, start.b, targetAlpha);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        sr.color = end;
    }

    private IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        Color start = image.color;
        Color end = new Color(start.r, start.g, start.b, targetAlpha);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        image.color = end;
    }

    private float CalculateOrthographicSize(Bounds bounds)
    {
        float sizeFromY = bounds.extents.y;
        float sizeFromX = bounds.extents.x / mainCamera.aspect;

        return Mathf.Max(sizeFromY, sizeFromX) * zoomPadding;
    }

    private void SetTransition(IEnumerator routine)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(routine);
    }
}