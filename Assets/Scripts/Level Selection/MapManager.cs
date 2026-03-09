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

    private BiomeController activeBiome;

    private Vector3 worldCameraPosition;
    private float worldOrthographicSize;
    private Coroutine transition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;

        worldCameraPosition = mainCamera.transform.position;
        worldOrthographicSize = mainCamera.orthographicSize;

        returnFromRegionButton.GetComponent<Button>().onClick.AddListener(() => ZoomOut());
        returnFromRegionButton.GetComponent<Button>().interactable = false;
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

        Coroutine camZoom = StartCoroutine(LerpCamera(
            new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y, mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.worldBounds),
            zoomDuration
        ));

        yield return camZoom;

        Coroutine fadeRegion = StartCoroutine(
            FadeSprite(activeBiome.GetRegionSpriteRenderer(), 1f, 0.3f)
        );

        Coroutine regionZoom = StartCoroutine(LerpCamera(
            new Vector3(activeBiome.GetRegionSpriteRenderer().bounds.center.x, activeBiome.GetRegionSpriteRenderer().bounds.center.y, mainCamera.transform.position.z),
            CalculateOrthographicSize(activeBiome.GetRegionSpriteRenderer().bounds),
            zoomDuration
        ));

        yield return fadeRegion;

        Coroutine fadeBiome = StartCoroutine(
            FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 0f, fadeDuration)
        );

        returnFromRegionButton.GetComponent<Button>().interactable = true;    
        Coroutine fadeReturnButton = StartCoroutine(
            FadeImage(returnFromRegionButton.GetComponent<Image>(), 1f, fadeDuration)
        );

        yield return regionZoom;    

        CurrentState = ZoomState.Biome;
    }

    private IEnumerator ZoomOutRoutine()
    {
        CurrentState = ZoomState.Transitioning;

        Coroutine camZoom = StartCoroutine(LerpCamera(
           new Vector3(activeBiome.worldBounds.center.x, activeBiome.worldBounds.center.y, mainCamera.transform.position.z),
           CalculateOrthographicSize(activeBiome.worldBounds),
           zoomDuration
       ));

        yield return camZoom;


        Coroutine fadeRegion = StartCoroutine(
            FadeSprite(activeBiome.GetRegionSpriteRenderer(), 0f, 0.3f)
        );

        returnFromRegionButton.GetComponent<Button>().interactable = false;

        Coroutine fadeReturnButton = StartCoroutine(
            FadeImage(returnFromRegionButton.GetComponent<Image>(), 0f, fadeDuration)
        );

        Coroutine biomeZoom = StartCoroutine(LerpCamera(
            worldCameraPosition,
            worldOrthographicSize,
            zoomDuration
        ));

        yield return fadeRegion;

        Coroutine fadeBiome = StartCoroutine(
            FadeSprite(activeBiome.GetComponent<SpriteRenderer>(), 1f, fadeDuration)
        );

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
            float progress = elapsed / duration;
            float t = zoomCurve.Evaluate(progress);
            mainCamera.transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, t);
            mainCamera.orthographicSize = Mathf.LerpUnclamped(startSize, targetSize, t);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;
    }

    private IEnumerator FadeSprite(SpriteRenderer spriteRenderer, float targetAlpha, float duration)
    {
        Color startColor = spriteRenderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        spriteRenderer.color = targetColor;
    }

    private IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        image.color = targetColor;
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
