using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BiomeController : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Identity")]
    public string biomName;
    public RegionController regionController;

    [Header("Visuals")]
    [Tooltip("Automatically assigned if left empty")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("SpriteRenderer used to visualize the biome region")]
    public SpriteRenderer regionSpriteRenderer;

    [Tooltip("SpriteRenderer with logical bounds to zoom in")]
    public SpriteRenderer zoomSpriteRenderer;

    [Tooltip("Scale multiplier when hovered")]
    public float hoverScale = 1.2f;

    [Tooltip("Time in seconds for one pulse cycle when hovered")]
    public float hoverPulse = 0.06f;

    [Tooltip("Speed of the pulse effect when hovered")]
    public float pulseSpeed = 2f;

    [Tooltip("Blend speed of the highlight color when hovered")]
    [Range(0f, 1f)]
    public float highlightBlendSpeed = 0.1f;

    [Tooltip("Color to blend towards when hovered")]
    public Color highlightColor = Color.white;

    [Tooltip("Time in seconds to return to original state after unhovering")]
    public float unhoverDuration = 0.15f;

    [Header("Level Nodes")]
    [Tooltip("Parent transform containing level nodes for this biome")]
    public Transform levelNodesParent;

    [HideInInspector] public Bounds worldBounds;

    private Vector3 originalScale;
    private Color originalColor;
    private float pulseTime;
    private Coroutine coroutine;

    private void Reset() => spriteRenderer = GetComponent<SpriteRenderer>();

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MapManager.Instance.CurrentState != MapManager.ZoomState.World) return;
        UpdateBounds();
        SetCoroutine(PulseHighlight());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetCoroutine(RestoreHighlight());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("MapManager instance not found. Cannot zoom to biome.");
            return;
        }

        // Reset visual state immediately before the zoom transition takes over.
        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (coroutine != null) { StopCoroutine(coroutine); coroutine = null; }

        Debug.Log($"Clicked on biome: {biomName}");
        MapManager.Instance.RequestZoomToBiome(this);
    }

    public void UpdateBounds()
    {
        if (zoomSpriteRenderer != null)
            worldBounds = zoomSpriteRenderer.bounds;
    }

    public SpriteRenderer GetRegionSpriteRenderer() => regionSpriteRenderer;

    private IEnumerator PulseHighlight()
    {
        pulseTime = 0f;

        while (true)
        {
            pulseTime += Time.deltaTime;

            float pulse = 1f + Mathf.Sin(pulseTime * pulseSpeed) * hoverPulse;
            transform.localScale = originalScale * (hoverScale * pulse);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, highlightColor, highlightBlendSpeed);

            yield return null;
        }
    }

    private IEnumerator RestoreHighlight()
    {
        float elapsed = 0f;
        Vector3 fromScale = transform.localScale;
        Color fromColor = spriteRenderer != null ? spriteRenderer.color : originalColor;

        while (elapsed < unhoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / unhoverDuration);
            transform.localScale = Vector3.Lerp(fromScale, originalScale, t);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(fromColor, originalColor, t);
            yield return null;
        }

        transform.localScale = originalScale;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        coroutine = null;
    }

    private void SetCoroutine(IEnumerator routine)
    {
        if (coroutine != null) { StopCoroutine(coroutine); coroutine = null; }
        coroutine = StartCoroutine(routine);
    }
}