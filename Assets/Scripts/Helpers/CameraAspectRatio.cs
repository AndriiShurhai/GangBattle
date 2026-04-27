using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour
{

    Camera cam;

    void Start()
    {
        cam = Camera.main;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        // Now position relative to actual screen bounds
        // e.g. place something at the right edge:
        transform.position = new Vector3(width / 2f - 1f, 0f, 0f);
    }
}