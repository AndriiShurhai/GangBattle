using System;
using System.Collections;
using UnityEngine;

public abstract class AbilityActionBase : AIActionSO
{
    protected abstract AbilityBaseSO GetAbility();


    public override bool CanExecute(Unit aiUnit)
    {
        AbilityBaseSO ability = GetAbility();
        if (ability == null) return false;
        return aiUnit.CanUseAbility(ability);
    }
}