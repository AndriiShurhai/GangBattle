using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

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

    public enum ZoomState { World, Transitioning, Biome }
    public ZoomState CurrentState { get; private set; } = ZoomState.World;
    public BiomeController ActiveBiome => activeBiome;

    private BiomeController activeBiome;

    private Vector3 worldCameraPosition;
    private float worldOrthographicSize;
    private Coroutine transition;

    private Button _returnButton;
    private Image _returnButtonImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;

        worldCameraPosition = mainCamera.transform.position;
        worldOrthographicSize = mainCamera.orthographicSize;

        _returnButton = returnFromRegionButton.GetComponent<Button>();
        _returnButtonImage = returnFromRegionButton.GetComponent<Image>();

        _returnButton.onClick.AddListener(ZoomOut);
        _returnButton.interactable = false;
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

        yield return StartCoroutine(LerpCamera(
            new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.worldBounds),
            zoomDuration
        ));

        SpriteRenderer regionRenderer = activeBiome.GetRegionSpriteRenderer();

        StartCoroutine(FadeSprite(regionRenderer, 1f, 0.3f));

        foreach (LevelNode level in activeBiome.regionController.GetLevelsInRegion())
            if (!level.IsLocked) StartCoroutine(FadeSprite(level.SpriteRenderer, 1f, fadeDuration));

        Coroutine regionZoom = StartCoroutine(LerpCamera(
            new Vector3(regionRenderer.bounds.center.x, regionRenderer.bounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(regionRenderer.bounds),
            zoomDuration
        ));

        _returnButton.interactable = true;
        StartCoroutine(FadeImage(_returnButtonImage, 1f, fadeDuration));
        StartCoroutine(FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 0f, fadeDuration));

        yield return regionZoom;

        CurrentState = ZoomState.Biome;
    }

    private IEnumerator ZoomOutRoutine()
    {
        CurrentState = ZoomState.Transitioning;

        yield return StartCoroutine(LerpCamera(
            new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y,
                        mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.worldBounds),
            zoomDuration
        ));

        StartCoroutine(FadeSprite(activeBiome.GetRegionSpriteRenderer(), 0f, 0.3f));

        foreach (LevelNode level in activeBiome.regionController.GetLevelsInRegion())
            StartCoroutine(FadeSprite(level.SpriteRenderer, 0f, 0.3f));

        _returnButton.interactable = false;
        StartCoroutine(FadeImage(_returnButtonImage, 0f, fadeDuration));

        StartCoroutine(LerpCamera(worldCameraPosition, worldOrthographicSize, zoomDuration));
        yield return StartCoroutine(FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 1f, fadeDuration));

        activeBiome = null;
        CurrentState = ZoomState.World;
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
