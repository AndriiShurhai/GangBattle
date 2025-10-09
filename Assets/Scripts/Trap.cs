using UnityEngine;

public class Trap : MonoBehaviour, IGridObject
{
    private Vector3Int gridPosition;
    private int damage;
    private int remainingDuration;
    private SpriteRenderer spriteRenderer;
    private GameObject visualObject;

    public Vector3Int GridPosition { get => gridPosition; set => gridPosition = value; }
    public bool BlocksMovement { get => false; }

    public void Initialize(Vector3Int position, int trapDamage, int duration, GameObject visualPrefab)
    {
        gridPosition = position;    
        damage = trapDamage;
        remainingDuration = duration;

        GridObjectRegistry.Instance.RegisterUnit(this);

        if (visualPrefab != null)
        {
            Instantiate(visualPrefab, transform);
        }
        else
        {
            GameObject visualObject = new GameObject("TrapVisual");
            visualObject.transform.SetParent(transform);
            visualObject.transform.localPosition = Vector3.zero;

            spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 0.5f, 0f, 1f); // Orange
            spriteRenderer.sortingOrder = 4;
            spriteRenderer.sortingLayerName = "Trap";

            // Create a simple square sprite (you'd replace this with actual art)
            spriteRenderer.sprite = CreateSquareSprite();
        }

        Debug.Log($"Trap placed at position {position} with damage {damage} for {duration} turns");
    }

    public void OnGridPositionChanged()
    {
        //
    }

    public void TriggerTrap(Unit steppingUnit)
    {
        Debug.Log($"{steppingUnit} stepped on a trap");

        steppingUnit.TakeDamage(damage, null);

        DestroyTrap();
    }

    private void DecreaseDuration()
    {
        remainingDuration--;

        if (remainingDuration <=0)
        {
            DestroyTrap();
        }
    }

    private void DestroyTrap()
    {
        GridObjectRegistry.Instance.UnregisterUnit(this, gridPosition);


        Destroy(gameObject);
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
    }

    public void OnGridPositionChanged(Vector3Int newGridPosition)
    {
        throw new System.NotImplementedException();
    }
}
