using DG;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attack Ability")]
public class AttackAbilitySO : AbilityBaseSO
{

    [Header("Attack Settings")]
    [UnityEngine.Serialization.FormerlySerializedAs("canAttackDiagonally")]
    [SerializeField] private bool _canAttackDiagonally = true;
    public bool CanAttackDiagonally => _canAttackDiagonally;

    [UnityEngine.Serialization.FormerlySerializedAs("jumpHeight")]
    [SerializeField] private float _jumpHeight = 0.5f;
    public float JumpHeight => _jumpHeight;

    [UnityEngine.Serialization.FormerlySerializedAs("jumpDuration")]
    [SerializeField] private float _jumpDuration = 0.3f;
    public float JumpDuration => _jumpDuration;

    private void Awake()
    {
    }
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(targetPosition);
            Vector3 dir = (targetWorldPosition - caster.transform.position).normalized;

            int damage = GetPower(caster);

            Sequence attackSequence = DOTween.Sequence();

            attackSequence.Append(caster.transform.DOJump(targetWorldPosition, JumpHeight, 1, JumpDuration));
            attackSequence.AppendCallback(() =>
            {
                onAbilityInvoke?.Invoke();
                targetUnit.TakeDamage(damage, caster);
                Debug.Log($"{caster.name} attacked {targetUnit.name} for {damage} damage!");

                // Spawn effect if available
                if (AbilityEffectPrefab != null)
                {
                    Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                    GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                    Destroy(effect, 2f);
                }
            });

            attackSequence.Append(caster.transform.DOJump(caster.transform.position, JumpHeight, 1, JumpDuration));
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

            // Must be orthogonal (either dx or dy should be 0)
            if (dx > 0 && dy > 0)
                return false;
        }

        return true;
    }

    public override List<AbilityUIStat> GetDetailedStats(Unit caster)
    {
        return new List<AbilityUIStat>
        {
            new AbilityUIStat { Label = "Damage", Value = GetPower(caster).ToString() }
        };
    }
}