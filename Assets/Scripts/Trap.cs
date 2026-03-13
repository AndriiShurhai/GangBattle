using System.Collections;
using UnityEngine;

public class TrapSnapshotState
{
    public Vector3Int gridPosition;
    public int remainingDuration;
    public int roundSinceRegister;
    public bool isActive;

}
public class Trap : MonoBehaviour, IGridObject, IRewindable
{
    [SerializeField] private EffectStatusType effectStatusType = EffectStatusType.None;
    [SerializeField] private int abilityDuration = 2;
    [SerializeField] private float destroyDelay = 0.8f;

    private Vector3Int gridPosition;
    private int damage;
    private int remainingDuration;
    private int roundSinceRegister = 0;
    private bool isActive = true;
    private RewindableID id;

    public string RewindID => id.ID;

    public Vector3Int GridPosition { get => gridPosition; set => gridPosition = value; }
    public bool BlocksMovement { get => false; }
    public void OnEnable()
    {
        Unit.OnUnitEnteredTile += Unit_OnUnitEnteredTile;
    }

    public void OnDisable()
    {
        Unit.OnUnitEnteredTile -= Unit_OnUnitEnteredTile;
    }

    public void RegisterSelf()
    {
        RewindManager.Instance.RegisterRewindable(this);
    }

    public object CaptureState()
    {
        Debug.Log($"Remaining duration before snapshot: {remainingDuration}");
        TrapSnapshotState state = new TrapSnapshotState
        {
            gridPosition = this.gridPosition,
            remainingDuration = this.remainingDuration,
            isActive = this.isActive,
            roundSinceRegister = this.roundSinceRegister,
        };

        return state;
    }

    public object CaptureDeactivatedState()
    {
        TrapSnapshotState state = new TrapSnapshotState
        {
            gridPosition = this.gridPosition,
            remainingDuration = 0,
            isActive = false,
            roundSinceRegister = 0
        };

        return state;
    }
    public void RestoreState(object state)
    {
        var s = (TrapSnapshotState)state;

        gameObject.SetActive(s.isActive);
        if (!isActive && s.isActive)
        {
            TrapRegistry.Instance.RegisterTrap(this);
        }
        else if (isActive && !s.isActive)
        {
            TrapRegistry.Instance.UnregisterTrap(this);
        }

        this.gridPosition = s.gridPosition;
        this.remainingDuration = s.remainingDuration;
        this.isActive = s.isActive;
        this.roundSinceRegister = s.roundSinceRegister;


        Debug.Log($"Trap has been restored. Duration: {remainingDuration}");
    }
    private void Unit_OnUnitEnteredTile(Unit unit, Vector3Int tilePosition)
    {
        if (tilePosition == this.GridPosition)
        {
            TriggerTrap(unit);
        }
    }

    public void Initialize(Vector3Int position, int trapDamage, int duration)
    {
        gridPosition = position;    
        damage = trapDamage;
        remainingDuration = duration; 

        TrapRegistry.Instance.RegisterTrap(this);

        id = gameObject.AddComponent<RewindableID>();
        RegisterSelf();

        Debug.Log($"Trap placed at position {position} with damage {damage} for {duration} turns");
    }

    public void OnGridPositionChanged()
    {
        //
    }

    public void TriggerTrap(Unit steppingUnit)
    {
        Debug.Log($"{steppingUnit} stepped on a trap, {damage}");

        steppingUnit.TakeDamage(damage, null);
        GetComponentInChildren<Animator>().SetTrigger("TrapTrigger");

        if (effectStatusType != EffectStatusType.None)
        {
            switch (effectStatusType)
            {
                case EffectStatusType.Rooted:
                    steppingUnit.ApplyEffect(effectStatusType, abilityDuration);
                    break;
            }
        }

        StartCoroutine(DestroyTrap());
    }

    public void DecreaseDuration()
    {
        if (roundSinceRegister == 0)
        {
            roundSinceRegister = 1;
            return;
        }
        remainingDuration--;

        if (remainingDuration <= 0)
        {
            StartCoroutine(DestroyTrap());
        }
    }

    private IEnumerator DestroyTrap()
    {
        yield return new WaitForSeconds(destroyDelay);
        isActive = false; 
        gameObject.SetActive(false);
        TrapRegistry.Instance.UnregisterTrap(this);
    }
    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
    }
}
