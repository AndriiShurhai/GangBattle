using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelNode : MonoBehaviour, IPointerClickHandler
{
    public static event Action<LevelNode> OnLevelNodeClick;

    [Header("Level information")]
    [SerializeField] private int _levelIndex;
    [SerializeField] private string _levelName;
    [SerializeField] private string _levelID;
    [TextArea(3, 5)]
    [SerializeField] private string _levelDescription;
    [SerializeField] private string _sceneName;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _lockedSprite;
    [SerializeField] private Sprite _unlockedSprite;

    public bool IsLocked { get; private set; }
    public string LevelName => _levelName;
    public string LevelDescription => _levelDescription;
    public int LevelIndex => _levelIndex;
    public string LevelSceneName => _sceneName;
    public string LevelID => _levelID;

    public int StarsOnLevel { get; private set; }

    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MapManager.Instance == null) return;

        var state = MapManager.Instance.CurrentState;
        if (state == MapManager.ZoomState.World || state == MapManager.ZoomState.Transitioning)
        {
            Debug.Log($"Level {LevelName} clicked, but map is not zoomed in enough to select levels.");
            return;
        }

        if (IsLocked)
        {
            Debug.Log($"Level {LevelName} is locked.");
            return;
        }

        OnLevelNodeClick?.Invoke(this);
        Debug.Log($"Level {LevelName} clicked!");
        SceneLoader.Instance.LoadSceneByName(LevelSceneName);
    }

    public void SetLocked(bool isLocked)
    {
        IsLocked = isLocked;
        _spriteRenderer.sprite = isLocked ? _lockedSprite : _unlockedSprite;

        Color color = isLocked ? Color.red : Color.green;
        color.a = (MapManager.Instance.CurrentState == MapManager.ZoomState.World || isLocked) ? 0f : 1f;
        _spriteRenderer.color = color;
    }

    public void SetStarsOnLevel(int starsAmount)
    {
        StarsOnLevel = starsAmount;
    }
}