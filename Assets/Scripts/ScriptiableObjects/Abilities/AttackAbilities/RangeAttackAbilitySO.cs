using DG.Tweening;
using UnityEngine;
using System;

[CreateAssetMenu(menuName ="Abilities/Range Attack")]
public class RangeAttackAbilitySO : AbilityBaseSO
{
    [Header("Attack Settings")]
    public bool canAttackDiagonally = true;
    public GameObject projectile;

    private void Awake()
    {
    }
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            // TODO: Play attack animation
            // TODO: Show damage numbers
            // TODO: Play sound effect

            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(targetPosition);
            int damage = GetPower(caster);

            Sequence attackSequence = DOTween.Sequence();

            GameObject projectileGameObject = Instantiate(projectile, caster.transform.position, Quaternion.identity);

            attackSequence.Append(projectileGameObject.transform.DOJump(targetWorldPosition, 0.5f, 1, 0.3f));
            attackSequence.AppendCallback(() =>
            {
                onAbilityInvoke?.Invoke();
                targetUnit.TakeDamage(damage, caster);
                Destroy(projectileGameObject);
                Debug.Log($"{caster.name} attacked {targetUnit.name} for {damage} damage!");

                // Spawn effect if available
                if (abilityEffectPrefab != null)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                    GameObject effect = Instantiate(abilityEffectPrefab, worldPos, Quaternion.identity);
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

        if (!canAttackDiagonally)
        {
            int dx = Mathf.Abs(targetPosition.x - casterPosition.x);
            int dy = Mathf.Abs(targetPosition.y - casterPosition.y);

            // Must be orthogonal (either dx or dy should be 0)
            if (dx > 0 && dy > 0)
                return false;
        }

        return true;
    }
}
