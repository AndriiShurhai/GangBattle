using UnityEngine;

public class CurrentLevelDataHolder : MonoBehaviour
{
    public static CurrentLevelDataHolder Instance { get; private set; }

    public int    CurrentLevelIndex       { get; set; }
    public string CurrentLevelID         { get; set; }
    public string CurrentLevelName       { get; set; }
    public string CurrentLevelDescription { get; set; }
    public string CurrentLevelSceneName  { get; set; }
    public string CurrentBiomeName       { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LevelNode.OnLevelNodeClick += OnLevelNodeClick;
    }

    private void OnDestroy()
    {
        LevelNode.OnLevelNodeClick -= OnLevelNodeClick;
    }

    private void OnLevelNodeClick(LevelNode node)
    {
        CurrentLevelID          = node.LevelID;
        CurrentLevelIndex       = node.LevelIndex;
        CurrentLevelName        = node.LevelName;
        CurrentLevelDescription = node.LevelDescription;
        CurrentLevelSceneName   = node.LevelSceneName;
        CurrentBiomeName        = MapManager.Instance.ActiveBiome.biomName;


        Debug.Log("Level node information:");
        Debug.Log($"ID: {CurrentLevelID}");
        Debug.Log($"Index: {CurrentLevelIndex}");
        Debug.Log($"Name: {CurrentLevelName}");
        Debug.Log($"Description: {CurrentLevelDescription}");
        Debug.Log($"Scene Name: {CurrentLevelSceneName}");
        Debug.Log($"Biome Name: {CurrentBiomeName}");
        Debug.Log($"Current level set to: {CurrentLevelName} (ID: {CurrentLevelID})");
    }
}