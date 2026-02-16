using DG.Tweening;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shield Bash Ability")]
public class ShieldBashSO : AbilityBaseSO
{
    public int stunDuration = 2;
    public float stunChance = 0.4f;
    public override void Execute(Unit caster, Vector3Int position, Action onComplete = null)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(position);

        if (targetObject is Unit targetUnit)
        {
            int damage = GetPower(caster);

            caster.transform.DOJump(GridManager.Instance.GridToWorld(position), 0.5f, 1, 0.3f).OnComplete(() =>
            {
                targetUnit.TakeDamage(damage, caster);

                if (IsAttackStunning())
                {
                    targetUnit.ApplyEffect(EffectStatusType.Stunned, stunDuration);
                }

                caster.transform.DOJump(GridManager.Instance.GridToWorld(caster.GridPosition), 0.5f, 1, 0.3f);
            });
        }
    }

    private bool IsAttackStunning()
    {
        float chance = UnityEngine.Random.value;

        return stunChance < chance;
    }
}
