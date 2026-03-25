using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Abilities/Boost Unit Ability")]
public class BoostUnitAbilitySO : AbilityBaseSO
{
    [UnityEngine.Serialization.FormerlySerializedAs("duration")]
    [SerializeField] private int _duration = 2;
    public int Duration => _duration;
    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke = null)
    {
        IGridObject gridObj = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (gridObj is Unit unit)
        {
            int strengthBosst = GetPower(unit);
            int intelligenceBoost = GetPower(unit);
            int agilityBoost = GetPower(unit);

            unit.BoostUnit(strengthBosst, intelligenceBoost, agilityBoost); 
            unit.ApplyEffect(EffectStatusType.Boosted, Duration);

            if (AbilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
            onAbilityInvoke?.Invoke();
        }
    }
}
