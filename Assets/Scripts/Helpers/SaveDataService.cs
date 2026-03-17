using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveDataService
{
    private static readonly string filePath = Application.persistentDataPath + "/LevelsCompletionData.json";

    public static SaveData ReadLevelsCompletionSaveData()
    {
        if (System.IO.File.Exists(filePath))
        {
            string saveDataJson = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SaveData>(saveDataJson);
        }
        else
        {
            return new SaveData() { LevelsCompletionData = new List<LevelCompletionData>() };
        }
    }

    public static void WriteLevelsCompletionSaveData(SaveData saveData)
    {
        File.WriteAllText(filePath, JsonUtility.ToJson(saveData));
    }

    public static void SaveLevelsCompletionData(LevelCompletionData newData)
    {
        SaveData saveData = ReadLevelsCompletionSaveData();
        List<LevelCompletionData> list = saveData.LevelsCompletionData;

        int existingIndex = list.FindIndex(x => x.LevelID == newData.LevelID);

        if (existingIndex >= 0)
        {
            if (list[existingIndex].starsCount < newData.starsCount)
                list[existingIndex] = newData;
        }
        else
        {
            list.Add(newData);
        }

        WriteLevelsCompletionSaveData(saveData);
    }

}