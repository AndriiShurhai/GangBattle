using UnityEngine;

[ExecuteAlways]
public class BackgroundFitter : MonoBehaviour
{
    void Awake() => Fit();
    void OnEnable() => Fit(); // catches editor reloads

    void Fit()
    {
        Camera cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (cam == null || sr == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        transform.localScale = new Vector3(
            worldWidth / sr.sprite.bounds.size.x,
            worldHeight / sr.sprite.bounds.size.y,
            1f
        );

        // make sure it's centered on camera
        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z
        );
    }
}