using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HighlightManager
{
    private float highlightAnimDuration = 0.3f;
    private readonly GameObject _highlightPrefab;
    private readonly Transform _container;

    private readonly Queue<GameObject> _highlightPool = new Queue<GameObject>();
    private readonly List<GameObject> _activeHighlights = new List<GameObject>();
    private readonly Vector3 _originalScale;

    public HighlightManager(GameObject highlightPrefab, Transform container)
    {
        _highlightPrefab = highlightPrefab;
        _container = container;
        if (_highlightPrefab != null)
        {
            _originalScale = _highlightPrefab.transform.localScale;
        }
        else
        {
            _originalScale = Vector3.one;
        }
    }

    public void CreateHighlight(Vector3Int gridPosition, Color color, bool withAnimation = true)
    {
        if (_highlightPrefab == null) return;


        Vector3 worldPosition = GridManager.Instance.GridToWorld(gridPosition);
        GameObject highlight = GetHighlightFromPool();

        highlight.transform.DOKill();

        foreach (var r in highlight.GetComponentsInChildren<SpriteRenderer>())
            r.DOKill();

        foreach (var l in highlight.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>())
            l.DOKill();

        highlight.transform.position = worldPosition;
        highlight.SetActive(true);

        highlight.transform.localScale = Vector3.zero;

        if (withAnimation)
        {
            CreateHighlightWithAnimation(highlight, color);

        }
        else
        {
            CreateHighlightWithoutAnimation(highlight, color);
        }
        _activeHighlights.Add(highlight);
    }

    public void ClearAllHighlights(bool withAnimation = true)
    {
        foreach (GameObject highlight in _activeHighlights)
        {
            if (highlight != null)
            {
                if (withAnimation)
                {
                    ClearHighlightWithAnimation(highlight);
                }
                else
                {
                    ReturnHighlightToPool(highlight);
                }
            }
        }
        _activeHighlights.Clear();
    }

    public void SetHighlightsActive(bool isActive)
    {
        foreach (GameObject highlight in _activeHighlights)
        {
            if (highlight != null) highlight.SetActive(isActive);
        }
    }

    private GameObject GetHighlightFromPool()
    {
        GameObject obj;

        if (_highlightPool.Count > 0)
        {
            obj = _highlightPool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_highlightPrefab, _container);
        }

        return obj;
    }

    private void ReturnHighlightToPool(GameObject highlight)
    {
        highlight.SetActive(false);
        _highlightPool.Enqueue(highlight);
    }

    private void CreateHighlightWithAnimation(GameObject highlight, Color color)
    {
        highlight.transform.DOScale(_originalScale, highlightAnimDuration);

        highlight.GetComponentInChildren<SpriteRenderer>().color = color;

        foreach (var light in highlight.GetComponentsInChildren<Light2D>())
        {
            float targetIntensity = light.GetComponent<LightDefaultIntensity>().defaultIntensity;

            light.intensity = 0f;

            DOTween.To(
                () => light.intensity,
                x => light.intensity = x,
                targetIntensity,
                highlightAnimDuration);
        }
    }

    private void ClearHighlightWithAnimation(GameObject highlight)
    {
        Sequence destroySeq = DOTween.Sequence();

        destroySeq.Append(highlight.transform.DOScale(0f, highlightAnimDuration));

        foreach (var light in highlight.GetComponentsInChildren<Light2D>())
        {
            destroySeq.Join(DOTween.To(
                () => light.intensity,
                x => light.intensity = x,
                0f,
                highlightAnimDuration));
        }

        destroySeq.AppendCallback(() => ReturnHighlightToPool(highlight));
    }

    private void CreateHighlightWithoutAnimation(GameObject highlight, Color color)
    {
        highlight.transform.localScale = _originalScale;
        highlight.GetComponentInChildren<SpriteRenderer>().color = color;

        foreach (var light in highlight.GetComponentsInChildren<Light2D>())
        {
            light.intensity = light.GetComponent<LightDefaultIntensity>().defaultIntensity;
        }
    }
}
