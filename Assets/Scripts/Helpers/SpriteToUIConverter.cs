using UnityEngine;
using UnityEngine.UI;

public class SpriteToUIConverter : MonoBehaviour
{
    void Start()
    {
        ConvertAll();
    }

    [ContextMenu("Convert All Children")]
    void ConvertAll()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in renderers)
        {
            GameObject obj = sr.gameObject;

            // Add Image if not present
            Image img = obj.GetComponent<Image>();
            if (img == null)
                img = obj.AddComponent<Image>();

            // Ensure RectTransform exists
            if (obj.GetComponent<RectTransform>() == null)
            {
                RectTransform rt = obj.AddComponent<RectTransform>();
                rt.localScale = Vector3.one;
            }

            if (sr.sprite == null)
            {
                Destroy(img);
                Debug.LogWarning($"SpriteRenderer on {obj.name} has no sprite assigned. Skipping.");
                continue;
            }
            // Copy sprite
            img.sprite = sr.sprite;

            // Optional: preserve color
            img.color = sr.color;

            // Optional: remove SpriteRenderer
            Destroy(sr);
        }
    }
}