using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TurnButtonsPanel : MonoBehaviour
{
    [SerializeField] private Button rewindToPreviousTurnButton;
    [SerializeField] private Button restartCurrentTurnButton;

    private void Start()
    {
        rewindToPreviousTurnButton.onClick.AddListener(OnRewindToPreviousTurn);
        restartCurrentTurnButton.onClick.AddListener(OnRestartCurrentTurn);
    }

    private void OnDestroy()
    {
        rewindToPreviousTurnButton.onClick.RemoveListener(OnRewindToPreviousTurn);
        restartCurrentTurnButton.onClick.RemoveListener(OnRestartCurrentTurn);
    }
    public void OnRewindToPreviousTurn()
    {
        TurnManager.Instance.RewindOneStep();
    }

    public void OnRestartCurrentTurn()
    {
        TurnManager.Instance.RewindToCurrentTurn();
    }
}