using Unity.VisualScripting;
using UnityEngine;

public enum EffectStatusType
{
    None,
    Stunned,
    Rooted,
    Burned,
    Provoked
}
public class StatusEffect
{
    public EffectStatusType type;
    public int duration;

    public StatusEffect(EffectStatusType type, int duration)
    {
        this.type = type;
        this.duration = duration;
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
                unit.TakeDamage(5, null);
                break;
            default:
                break;
        }
        duration--;
        Debug.Log($"TICK HAS BEEN CALLED. DURATION: {duration}");
    }
}
