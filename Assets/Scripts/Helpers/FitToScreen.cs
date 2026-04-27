using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToScreen : MonoBehaviour
{
    [Header("Aspect ratio you designed at (e.g. 16/9)")]
    public float referenceAspect = 16f / 9f;

    [Header("Don't touch — right-click → Save Reference to fill these")]
    public Vector3 referencePosition;
    public Vector3 referenceScale;

    void Start() => Adapt();

    void Adapt()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float refHalfWidth = cam.orthographicSize * referenceAspect;
        float curHalfWidth = cam.orthographicSize * cam.aspect;
        float ratio = curHalfWidth / refHalfWidth;

        transform.position = new Vector3(
            referencePosition.x * ratio,
            referencePosition.y,   // Y is fine, ortho height doesn't change
            referencePosition.z
        );

        transform.localScale = new Vector3(
            referenceScale.x * ratio,
            referenceScale.y * ratio,
            referenceScale.z
        );
    }

    [ContextMenu("Save Reference")]
    public void SaveReference()
    {
        referencePosition = transform.position;
        referenceScale = transform.localScale;
    }
}