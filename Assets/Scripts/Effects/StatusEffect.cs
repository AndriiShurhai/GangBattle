using UnityEngine;
using System;
using Unity.VisualScripting;
public enum EffectStatusType
{
    None,
    Stunned,
    Rooted,
    Burned,
    Provoked,
    Boosted
}
public class StatusEffect
{
    public EffectStatusType type;
    public int duration;
    public Action tickAction;

    public StatusEffect(EffectStatusType type, int duration, Action tickAction = null)
    {
        this.type = type;
        this.duration = duration;
        this.tickAction = tickAction;
    }

    public void Tick(Unit unit)
    {
        switch (type)
        {
            case EffectStatusType.None:
                break;
            case EffectStatusType.Stunned:
                break;
            case EffectStatusType.Burned:
                break;
            case EffectStatusType.Provoked:
                break;
            default:
                break;
        }
        tickAction?.Invoke();
        duration--;
        Debug.Log($"TICK HAS BEEN CALLED. DURATION: {duration}");
    }
}
