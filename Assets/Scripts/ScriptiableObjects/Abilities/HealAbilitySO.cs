using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Heal Ability")]
public class HealAbilitySO : AbilityBaseSO
{
    [Header("Heal Settings")]
    [UnityEngine.Serialization.FormerlySerializedAs("canHealSelf")]
    [SerializeField] private bool _canHealSelf = true;
    public bool CanHealSelf => _canHealSelf;

    [UnityEngine.Serialization.FormerlySerializedAs("coefficent")]
    [SerializeField] private float _coefficent = 1.5f;
    public float Coefficent => _coefficent;

    public override void Execute(Unit caster, Vector3Int targetPosition, Action onAbilityInvoke)
    {
        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);

        if (targetObject is Unit targetUnit)
        {
            int healAmount = GetPower(caster);
            targetUnit.Heal(healAmount);

            int teamHealAmount = healAmount / 2;

            foreach (Unit unit in TurnManager.Instance.GetAlivePlayerUnits())
            {
                if (unit != targetUnit)
                {
                    unit.Heal(teamHealAmount);
                    if (AbilityEffectPrefab != null)
                    {
                        Vector3 worldPos = GridManager.Instance.GridToWorld(unit.GridPosition);
                        GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                        Destroy(effect, 2f);
                    }
                }
            }
            onAbilityInvoke?.Invoke();

            Debug.Log($"{caster.name} healed {targetUnit.name} for {healAmount} HP!");

            if (AbilityEffectPrefab != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorld(targetPosition);
                GameObject effect = Instantiate(AbilityEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        else
        {
            Debug.LogWarning($"No unit to heal at {targetPosition}");
        }
    }

    public override bool IsValidTarget(Vector3Int casterPosition, Vector3Int targetPosition, Unit caster)
    {
        if (!GetTilesInRange(casterPosition).Contains(targetPosition))
            return false;

        IGridObject targetObject = GridObjectRegistry.Instance.GetObjectAt(targetPosition);
        if (!(targetObject is Unit targetUnit))
            return false;

        if (!CanHealSelf && targetUnit == caster)
            return false;

        if (targetUnit.CurrentHealth <= 0)
            return false;


        // TODO: Add team check - only heal allies
        if (TypeOfTarget == TargetType.Ally && targetUnit.UnitFaction != caster.UnitFaction)
        {
            return false;
        }

        return true;
    }

    public override List<AbilityUIStat> GetDetailedStats(Unit caster)
    {
        return new List<AbilityUIStat>
        {
            new AbilityUIStat { Label = "Heal Amount", Value = GetPower(caster).ToString() },
            new AbilityUIStat { Label = "Team Heal", Value = (GetPower(caster) / 2).ToString() }
        };
    }
}