using System;
using UnityEngine;

public class UnitVisualBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SPUM_Prefabs _spumPrefabs;



    private void Start()
    {

    }

    public void StartRunningAnimation(Vector3 destination)
    {
        FaceCorrectDirection(destination);
        _spumPrefabs.PlayAnimation(PlayerState.MOVE, 0);
    }

    public void StopRunningAnimation()
    {
        _spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
    }
    
    public void TakeDamageAnimation()
    {
        _spumPrefabs.PlayAnimation(PlayerState.DAMAGED, 0);
    }

    public void AttackAnimation()
    {
        _spumPrefabs.PlayAnimation(PlayerState.ATTACK, 0);
    }

    public void StartDebuffAnimation()
    {
        _spumPrefabs.PlayAnimation(PlayerState.DEBUFF, 0);
    }

    public void StopDebuffAnimation()
    {
        _spumPrefabs.PlayAnimation(PlayerState.IDLE, 0);
    }

    public void DeathAnimation(Action onComplete = null)
    {
        _spumPrefabs.PlayAnimation(PlayerState.DEATH, 0);
    }

    private void FaceCorrectDirection(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction.x > 0)
        {
            _spumPrefabs.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x < 0)
        {
            _spumPrefabs.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
