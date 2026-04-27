using DG.Tweening;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Range Attack")]
public class RangeAttackAbilitySO : AbilityBaseSO
{
    [Header("Attack Settings")]
    [UnityEngine.Serialization.FormerlySerializedAs("canAttackDiagonally")]
    [SerializeField] private bool _canAttackDiagonally = true;
    public bool CanAttackDiagonally => _canAttackDiagonally;

    [UnityEngine.Serialization.FormerlySerializedAs("projectile")]
    [SerializeField] private GameObject _projectile;
    public GameObject Projectile => _projectile;

    [UnityEngine.Serialization.FormerlySerializedAs("projectileJumpHeight")]
    [SerializeField] private float _projectileJumpHeight = 0.5f;
    public float ProjectileJumpHeight => _projectileJumpHeight;

    [UnityEngine.Serialization.FormerlySerializedAs("projectileJumpDuration")]
    [SerializeField] private float _projectileJumpDuration = 0.3f;
    public float ProjectileJumpDuration => _projectileJumpDuration;

    [Header("Projectile Flight")]
    [SerializeField] private Ease _flightEase = Ease.OutQuad;
    [SerializeField] private float _impactSquashDuration = 0.08f;

    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            Vector3 spawnPos = caster.transform.position;
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(targetPosition);
            int damage = GetPower(caster);

            GameObject projectileGameObject = Instantiate(Projectile, spawnPos, Quaternion.identity);

            // --- Rotate to face target on spawn ---
            Vector3 toTarget = targetWorldPosition - spawnPos;
            float initialAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            projectileGameObject.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);

            // --- Punch scale on spawn ---
            projectileGameObject.transform.localScale = Vector3.zero;
            projectileGameObject.transform.DOScale(Vector3.one, 0.08f).SetEase(Ease.OutBack);

            Sequence attackSequence = DOTween.Sequence();

            // --- Flight arc with per-frame nose tracking ---
            Vector3 previousPos = spawnPos;

            attackSequence.Append(
                projectileGameObject.transform
                    .DOJump(targetWorldPosition, ProjectileJumpHeight, 1, ProjectileJumpDuration)
                    .SetEase(_flightEase)
                    .OnUpdate(() =>
                    {
                        if (projectileGameObject == null) return;

                        Vector3 currentPos = projectileGameObject.transform.position;
                        Vector3 delta = currentPos - previousPos;

                        if (delta.sqrMagnitude > 0.0001f)
                        {
                            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                            projectileGameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                        }

                        previousPos = currentPos;
                    })
            );

            // --- Impact: squash then destroy ---
            attackSequence.Append(
                projectileGameObject.transform
                    .DOScale(new Vector3(1.4f, 0.6f, 1f), _impactSquashDuration)
                    .SetEase(Ease.OutQuad)
            );

            attackSequence.AppendCallback(() =>
            {
                onAbilityInvoke?.Invoke();
                targetUnit.TakeDamage(damage, caster);
                Destroy(projectileGameObject);
                Debug.Log($"{caster.name} attacked {targetUnit.name} for {damage} damage!");

                if (AbilityEffectPrefab != null)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                    GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                    Destroy(effect, 2f);
                }
            });
        }
        else
        {
            Debug.LogWarning($"No valid target at {targetPosition}");
        }
    }

    public override bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!base.IsValidTarget(casterPosition, targetPosition, caster))
            return false;

        if (caster.HasStatus(EffectStatusType.Provoked) && caster.ForcedUnitGridPosition != targetPosition)
        {
            Debug.Log($"IT IS NOT PROVOKED UNIT. FORCED UNIT POSITION: {caster.ForcedUnitGridPosition} YOUR TARGET GRID POSITION: {targetPosition}");
            return false;
        }

        if (!CanAttackDiagonally)
        {
            int dx = Mathf.Abs(targetPosition.x - casterPosition.x);
            int dy = Mathf.Abs(targetPosition.y - casterPosition.y);

            if (dx > 0 && dy > 0)
                return false;
        }

        return true;
    }
}