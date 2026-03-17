using UnityEngine;
using System.Collections.Generic;
using System;

public class RegionController : MonoBehaviour
{
    [SerializeField] private List<LevelNode> regionLevels = new();

    private void Start()
    {
        if (regionLevels.Count == 0) return;

        foreach (var level in regionLevels)
            level.SetLocked(true);

        ApplySaveData(SaveDataService.ReadLevelsCompletionSaveData());
    }

    public List<LevelNode> GetLevelsInRegion() => regionLevels;


    private void ApplySaveData(SaveData saveData)
    {
        List<LevelCompletionData> completions = saveData.LevelsCompletionData;


        bool regionStarted = completions.Exists(x => x.LevelID == regionLevels[0].LevelID);
        if (!regionStarted)
        {
            regionLevels[0].SetLocked(false);
            return;
        }

        int lastCompletedListIndex = -1;
        int lastCompletedStars = 0;

        for (int i = 0; i < regionLevels.Count; i++)
        {
            LevelNode node = regionLevels[i];
            LevelCompletionData data = completions.Find(x => x.LevelID == node.LevelID);

            if (string.IsNullOrEmpty(data.LevelID)) continue;

            node.SetLocked(false);
            node.SetStarsOnLevel(data.starsCount);

            if (i > lastCompletedListIndex)
            {
                lastCompletedListIndex = i;
                lastCompletedStars = data.starsCount;
            }
        }

        int nextIndex = lastCompletedListIndex + 1;
        if (lastCompletedStars > 0 && nextIndex < regionLevels.Count)
        {
            Debug.Log($"Unlocking next level: {regionLevels[nextIndex].LevelName}");
            regionLevels[nextIndex].SetLocked(false);
        }
    }
}
