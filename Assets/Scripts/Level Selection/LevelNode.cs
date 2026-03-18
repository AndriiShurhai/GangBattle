using System;
using TMPro;
using UnityEditor.ShaderGraph;
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
    [SerializeField] private GameObject _threeStarsSpritesContainer;
    [SerializeField] private GameObject _twoStarsSpritesContainer;
    [SerializeField] private GameObject _oneStarSpriteContainer;

    public bool IsLocked { get; private set; }
    public string LevelName => _levelName;
    public string LevelDescription => _levelDescription;
    public int LevelIndex => _levelIndex;
    public string LevelSceneName => _sceneName;
    public string LevelID => _levelID;

    public int StarsOnLevel { get; private set; }

    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    private void Awake()
    {
    }
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

        Color color = isLocked ? Color.red : Color.green;
        color.a = (MapManager.Instance.CurrentState == MapManager.ZoomState.World || isLocked) ? 0f : 1f;
        if (MapManager.Instance.CurrentState == MapManager.ZoomState.World)
        {
            Debug.Log($"Map is in World state, setting level {LevelName} sprite to locked.");
        }

        if (isLocked)
        {
            Debug.Log($"level setted{LevelName} to locked.");
        }
        _spriteRenderer.color = color;
        SetStarsOnLevel(StarsOnLevel);
        SetStarsColors();
    }

    public void SetStarsOnLevel(int starsAmount)
    {
        StarsOnLevel = starsAmount;
        if (StarsOnLevel == 1)
        {
            _oneStarSpriteContainer.SetActive(true);
        }
        else if (StarsOnLevel == 2)
        {
            _twoStarsSpritesContainer.SetActive(true);
        }
        else if (StarsOnLevel == 3)
        {
            _threeStarsSpritesContainer.SetActive(true);
        }
        else
        {
            _oneStarSpriteContainer.gameObject.SetActive(false);
            _twoStarsSpritesContainer.gameObject.SetActive(false);
            _threeStarsSpritesContainer.gameObject.SetActive(false);
        }
    }

    public GameObject GetCurrentStarsContainer()
    {
        return StarsOnLevel switch
        {
            1 => _oneStarSpriteContainer,
            2 => _twoStarsSpritesContainer,
            3 => _threeStarsSpritesContainer,
            _ => null
        };
    }
    private void SetStarsColors()
    {
        switch (StarsOnLevel)
        {
            case 1:
                Color color = _oneStarSpriteContainer.GetComponentInChildren<SpriteRenderer>().color;
                color.a = IsLocked ? 0f : 1f;
                _oneStarSpriteContainer.GetComponentInChildren<SpriteRenderer>().color = color;
                break;

            case 2:
                foreach (SpriteRenderer sr in _twoStarsSpritesContainer.GetComponentsInChildren<SpriteRenderer>())
                {
                    Color color2 = sr.color;
                    color2.a = IsLocked ? 0f : 1f;
                    sr.color = color2;
                }
                break;

            case 3:
                foreach (SpriteRenderer sr in _threeStarsSpritesContainer.GetComponentsInChildren<SpriteRenderer>())
                {
                    Color color3 = sr.color;
                    color3.a = IsLocked ? 0f : 1f;
                    sr.color = color3;
                }
                break;

            default:
                foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                {
                    Color color4 = sr.color;
                    color4.a = 0f;
                    sr.color = color4;
                }
                break;
        }
    }
}