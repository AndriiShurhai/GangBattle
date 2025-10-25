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

    private bool isGameOver;

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

            unit.OnUnitDied += Unit_OnUnitDied;
        }

        StartPlayerTurn();
    }

    private void Unit_OnUnitDied(Unit unit)
    {
        if (isGameOver) return;

        if (playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);  
            unit.OnUnitDied -= Unit_OnUnitDied;
            
            if (playerUnits.Count == 0)
            {
                Debug.Log("enemies win");
                isGameOver = true;
                EndGame();
            }
        }

        else if(enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);
            unit.OnUnitDied -= Unit_OnUnitDied;

            if (enemyUnits.Count == 0)
            {
                Debug.Log("player wins");
                isGameOver = true;
                EndGame();
            }
        }
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
            enemyUnit.GetComponent<AIBrain>()?.TakeTurn();
            yield return new WaitForSeconds(3f);
            enemyUnit.HasTakenActionThisTurn = true;
        }

        Debug.Log("--- ENEMY TURN END ---");

        EndTurn();
    }

    private void EndGame()
    {
        StopAllCoroutines();

        characterSelectionController.ClearSelection();

        characterSelectionController.gameObject.SetActive(false);

        // show a victory or defeat screen or something
    }


    public List<Unit> GetPlayerUnits()
    {
        return playerUnits;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemyUnits;
    }
}
