using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitVisualBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SPUM_Prefabs _spumPrefabs;

    private Vector3 _originalScale;
    private Dictionary<SpriteRenderer, Color> _originalColors = new();
    private bool _isPulsing = false;

    public void StartPulse()
    {
        if (_isPulsing) return;
        _isPulsing = true;
        _originalScale = transform.localScale;
        _originalColors.Clear();

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            _originalColors[sr] = sr.color;
        }

        transform
            .DOScale(_originalScale * 1.2f, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId(this); 

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            sr.DOColor(Color.red, 0.35f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetId(this);
        }
    }

    public void StopPulse()
    {
        if (!_isPulsing) return;
        _isPulsing = false;
        DOTween.Kill(this);
        transform.localScale = _originalScale;
        foreach (var pair in _originalColors)
        {
            if (pair.Key != null)
                pair.Key.color = pair.Value;
        }
    }

    public void StartRunningAnimation(Vector3 destination)
    {
        FaceCorrectDirection(destination);
        _spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
    }

    public void StopRunningAnimation() => _spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
    public void TakeDamageAnimation() => _spumPrefabs.PlayAnimation(PlayerState.DAMAGED, 0);
    public void AttackAnimation() => _spumPrefabs.PlayAnimation(PlayerState.ATTACK, 0);
    public void StartDebuffAnimation() => _spumPrefabs.PlayAnimation(PlayerState.DEBUFF, 0);
    public void StopDebuffAnimation() => _spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);

    public void DeathAnimation(Action onComplete = null)
    {
        _spumPrefabs.PlayAnimation(PlayerState.DEATH, 0);
        // TODO: Hook onComplete into the SPUM animation-end callback when the API supports it.
        // For now invoke immediately so callers are never silently dropped.
        onComplete?.Invoke();
    }

    public void PlayAnimation(PlayerState playerState)
    {
        try
        {
            _spumPrefabs.PlayAnimation(playerState, 0);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{nameof(UnitVisualBridge)}: Failed to play {playerState}, falling back to IDLE. Exception: {e}");
            _spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
        }
    }

    private void FaceCorrectDirection(Vector3 targetPosition)
    {
        float dirX = targetPosition.x - transform.position.x;

        if (dirX > 0) _spumPrefabs.transform.localScale = new Vector3(-1, 1, 1);
        else if (dirX < 0) _spumPrefabs.transform.localScale = new Vector3(1, 1, 1);
    }
}