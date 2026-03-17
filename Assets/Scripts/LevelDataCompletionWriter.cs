using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public struct SaveData
{
    public List<LevelCompletionData> LevelsCompletionData;
}
[System.Serializable]
public struct LevelCompletionData
{
    public string LevelID;
    public int starsCount;
}

public class LevelDataCompletionWriter : MonoBehaviour
{
    private void Start()
    {
        TurnManager.Instance.OnLevelCompleted += OnLevelCompleted;
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnLevelCompleted -= OnLevelCompleted;
    }

    private void OnLevelCompleted()
    {
        int stars = TurnManager.Instance.CalculateStarsEarned();

        var completionData = new LevelCompletionData
        {
            LevelID = CurrentLevelDataHolder.Instance.CurrentLevelID,
            starsCount = stars
        };

        SaveDataService.SaveLevelsCompletionData(completionData);

        Debug.Log($"Level completed and saved — ID: {completionData.LevelID}, Stars: {stars}");
    }
}