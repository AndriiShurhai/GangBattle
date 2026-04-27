using UnityEngine;

[ExecuteAlways]
public class CameraWidthFitter : MonoBehaviour
{
    [Tooltip("World units wide your game is designed for")]
    public float referenceWorldWidth = 16f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    void Update()
    {
#if UNITY_EDITOR
            // Don't override camera if MapManager is transitioning or in biome view
            if (MapManager.Instance != null &&
                MapManager.Instance.CurrentState != MapManager.ZoomState.World) return;
            Fit();
#endif
    }

    void Fit()
    {
        if (cam == null) cam = GetComponent<Camera>();
        cam.orthographicSize = referenceWorldWidth / (2f * cam.aspect);
    }
}