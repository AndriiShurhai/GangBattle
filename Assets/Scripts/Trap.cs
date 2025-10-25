using UnityEngine;

public class Trap : MonoBehaviour, IGridObject
{
    private Vector3Int gridPosition;
    private int damage;
    private int remainingDuration;

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

        //GridObjectRegistry.Instance.RegisterObject(this);

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

        DestroyTrap();
    }

    private void DecreaseDuration()
    {
        remainingDuration--;

        if (remainingDuration <=0)
        {
            DestroyTrap();
        }
    }

    private void DestroyTrap()
    {
        //GridObjectRegistry.Instance.UnregisterObject(this, gridPosition);
        Destroy(gameObject);
    }
    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
    }
}
