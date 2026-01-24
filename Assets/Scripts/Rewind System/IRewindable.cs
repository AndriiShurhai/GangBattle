using UnityEngine;


public interface IRewindable
{
    string RewindID { get; }
    object CaptureState();
    object CaptureDeactivatedState();
    void RestoreState(object state);
    void RegisterSelf();
}
