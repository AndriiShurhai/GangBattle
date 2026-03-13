using UnityEngine;
using System;
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
    public GameObject visualEffectPrefab;

    public StatusEffect(EffectStatusType type, int duration, Action tickAction = null, GameObject visualEffectPrefab = null)
    {
        this.type = type;
        this.duration = duration;
        this.tickAction = tickAction;
        if (visualEffectPrefab != null)
        {
            this.visualEffectPrefab = visualEffectPrefab;
        }
    }

    public void InstantiateVisualEffect(Unit unit)
    {
        if (visualEffectPrefab != null)
        {
            GameObject effectInstance = GameObject.Instantiate(visualEffectPrefab, unit.transform);
            effectInstance.transform.localPosition = Vector3.zero; 
        }
    }

    public void RemoveVisualEffect(Unit unit)
    {
        if (visualEffectPrefab != null)
        {
            foreach (Transform child in unit.transform)
            {
                if (child.gameObject.name.Contains(visualEffectPrefab.name))
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Returns a deep copy of this effect with the same type, duration, tickAction, and visualEffectPrefab.
    /// Use when capturing snapshots so that subsequent Tick() calls do not mutate stored snapshots.
    /// </summary>
    public StatusEffect Clone() => new StatusEffect(type, duration, tickAction, visualEffectPrefab);

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
