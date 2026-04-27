using UnityEngine;

public class ScreenAdapter : MonoBehaviour
{
    [System.Serializable]
    public class AdaptedObject
    {
        public GameObject obj;

        [Header("Viewport position (0-1). 0.5,0.5 = center, 0,0 = bottom-left, 1,1 = top-right")]
        public Vector2 viewportPosition = new Vector2(0.5f, 0.5f);

        [Header("Offset in world units from that anchor")]
        public Vector2 worldOffset = Vector2.zero;

        [Header("Scale relative to camera height (1 = full screen height)")]
        public float relativeScale = 0.1f;

        [Header("Is this the background? (fills entire screen)")]
        public bool isBackground = false;
    }

    public AdaptedObject[] objects;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        Adapt();
    }

    void Adapt()
    {
        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        foreach (var o in objects)
        {
            if (o.obj == null) continue;

            if (o.isBackground)
            {
                SpriteRenderer sr = o.obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    o.obj.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, o.obj.transform.position.z);
                    o.obj.transform.localScale = new Vector3(
                        worldWidth / sr.sprite.bounds.size.x,
                        worldHeight / sr.sprite.bounds.size.y,
                        1f
                    );
                }
                continue;
            }

            // Convert viewport to world position
            Vector3 viewportPos = new Vector3(o.viewportPosition.x, o.viewportPosition.y, Mathf.Abs(cam.transform.position.z));
            Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
            worldPos.x += o.worldOffset.x;
            worldPos.y += o.worldOffset.y;
            worldPos.z = o.obj.transform.position.z;
            o.obj.transform.position = worldPos;

            // Scale relative to world height
            float scale = worldHeight * o.relativeScale;
            o.obj.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}