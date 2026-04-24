using DG.Tweening;
using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Abilities/Teleport Ability")]
public class TeleportAbilitySO : AbilityBaseSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("teleportingDuration")]
    [SerializeField] private float _teleportingDuration = 0.4f;
    public float TeleportingDuration => _teleportingDuration;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        Vector3 targetWorldPosition = GridManager.Instance.GridToWorld(targetPosition);

        Sequence attackSequence = DOTween.Sequence();

        onAbilityInvoke?.Invoke();
        attackSequence.Append(caster.transform.DOScale(0, TeleportingDuration));
        attackSequence.AppendCallback(() =>
        {
            caster.transform.position = targetWorldPosition;
            GridObjectRegistry.Instance.MoveObject(caster, caster.GridPosition, targetPosition);
            // Spawn effect if available
            if (AbilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
        });

        attackSequence.Append(caster.transform.DOScale(caster.transform.localScale, TeleportingDuration));
    }

    public override bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!GetTilesInRange(casterPosition).Contains(targetPosition))
        {
            return false;
        }

        if (GridObjectRegistry.Instance.GetObjectAt(targetPosition) != null) return false;

        if (!GridManager.Instance.IsWalkable(targetPosition)) return false;

        return true;
    }
}
