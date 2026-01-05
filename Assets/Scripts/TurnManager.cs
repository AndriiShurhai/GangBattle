using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<Transform> playersPositions;
    public List<Transform> enemiesPositions;

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    [SerializeField] private CharacterSelectionController characterSelectionController;
    [SerializeField] private Button endPlayerTurnButton;
    public enum TurnState { PlayerTurn, EnemyTurn }
    private TurnState currentState;

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

        endPlayerTurnButton.onClick.AddListener(() =>
        {
            EndTurn();
        });
    }

    private void Start()
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            GameObject enemy = Instantiate(enemyUnits[i].gameObject);
            Unit enemyScript = enemy.GetComponent<Unit>();
            enemyScript.Initialize();
            enemyScript.PlaceUnit(enemiesPositions[i].position);

            enemyUnits[i] = enemyScript;
        }

        for (int i = 0; i < playerUnits.Count; i++)
        {
            GameObject player = Instantiate(playerUnits[i].gameObject);
            Unit playerScript = player.GetComponent<Unit>();
            playerScript.Initialize();
            playerScript.PlaceUnit(playersPositions[i].position);

            playerUnits[i] = playerScript;
        }

        foreach (var playerUnit in playerUnits)
        {
            playerUnit.OnUnitDied += Unit_OnUnitDied;
        }

        foreach (var enemyUnit in enemyUnits)
        {
            enemyUnit.OnUnitDied += Unit_OnUnitDied;
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
            unit.ResetUsedAbilities();
            unit.HasTakenActionThisTurn = false;
            unit.ResetUsedMovement();
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
            endPlayerTurnButton.gameObject.SetActive(false);
            StartEnemyTurn();
        }
        else if (currentState == TurnState.EnemyTurn)
        {
            endPlayerTurnButton.gameObject.SetActive(true);
            StartPlayerTurn();
        }


        List<Trap> traps = new List<Trap> (TrapRegistry.Instance.GetTraps());
        foreach (Trap trap in traps)
        {
            trap.DecreaseDuration();
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        List<Unit> enemiesToTakeTurn = new List<Unit>(enemyUnits);

        foreach (Unit enemyUnit in enemiesToTakeTurn)
        {
            if (enemyUnits.Contains(enemyUnit))
            {
                bool isTurnComplete = false;

                enemyUnit.GetComponent<AIBrain>()?.TakeTurn(() =>
                {
                    isTurnComplete = true;
                });

                yield return new WaitUntil(() => isTurnComplete);

                if (enemyUnit != null) enemyUnit.HasTakenActionThisTurn = true;
            }
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
