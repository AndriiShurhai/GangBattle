using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private CharacterSelectionController characterSelectionController;

    public enum TurnState { PlayerTurn, EnemyTurn }
    private TurnState currentState;

    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in allUnits)
        {
            if (unit.UnitFaction == Faction.Player)
            {
                playerUnits.Add(unit);
            }
            else if (unit.UnitFaction == Faction.Enemy)
            {
                enemyUnits.Add(unit);
            }
        }

        StartEnemyTurn();
    }

    public void StartPlayerTurn()
    {
        currentState = TurnState.PlayerTurn;
        Debug.Log("--- PLAYER TURN START ---");

        if (characterSelectionController != null)
        {
            Debug.Log("enabling character selection controller");
            characterSelectionController.gameObject.SetActive(true);
        }

        foreach (Unit unit in playerUnits)
        {
            unit.HasTakenActionThisTurn = false;
        }
    }

    public void StartEnemyTurn()
    {
        currentState = TurnState.EnemyTurn;
        Debug.Log("--- ENEMY TURN START ---");

        if (characterSelectionController != null)
        {
            Debug.Log("Disabling character selection controller");
            characterSelectionController.gameObject.SetActive(false);
        }

        foreach (Unit unit in enemyUnits)
        {
            unit.HasTakenActionThisTurn = false;
        }

        StartCoroutine(EnemyTurnRoutine());
    }
    public void EndTurn()
    {
        if (currentState == TurnState.PlayerTurn)
        {
            StartEnemyTurn();
        }
        else if (currentState == TurnState.EnemyTurn)
        {
            StartPlayerTurn();
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        foreach (Unit enemyUnit in enemyUnits)
        {
            Debug.Log($"{enemyUnit.name} is thinking...");
            yield return new WaitForSeconds(3f);
            enemyUnit.HasTakenActionThisTurn = true;
        }

        Debug.Log("--- ENEMY TURN END ---");

        EndTurn();
    }
}
