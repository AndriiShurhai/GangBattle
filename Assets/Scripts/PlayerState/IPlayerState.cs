using UnityEngine;

public interface IPlayerState
{
    void Enter();
    void Exit();
    void OnClick(Vector3Int position);
}
