using DG.Tweening;
using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Abilities/Range Attack")]
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

            GameObject projectileGameObject = Instantiate(Projectile, caster.transform.position, Quaternion.identity);

            Vector3 direction = caster.transform.position - targetWorldPosition;

            if (direction.x > 0)
            {
                projectileGameObject.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (direction.x < 0)
            {
                projectileGameObject.transform.localScale = new Vector3(1, 1, 1);
            }

            attackSequence.Append(projectileGameObject.transform.DOJump(targetWorldPosition, ProjectileJumpHeight, 1, ProjectileJumpDuration));
            attackSequence.AppendCallback(() =>
            {
                onAbilityInvoke?.Invoke();
                targetUnit.TakeDamage(damage, caster);
                Destroy(projectileGameObject);
                Debug.Log($"{caster.name} attacked {targetUnit.name} for {damage} damage!");

                // Spawn effect if available
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

            // Must be orthogonal (either dx or dy should be 0)
            if (dx > 0 && dy > 0)
                return false;
        }

        return true;
    }
}
