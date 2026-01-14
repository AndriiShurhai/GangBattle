using UnityEngine;


public interface IRewindable
{
    string RewindID { get; }
    object CaptureState();
    void RestoreState(object state);
    void RegisterSelf();
}
