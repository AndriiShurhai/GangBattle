using UnityEngine;

public interface IPlayerState
{
    void Enter();
    void Update();
    void Exit();
    void OnClick(Vector3Int position);
}
