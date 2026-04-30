using UnityEngine;
using System.IO;

public class DebugSaveWiper : MonoBehaviour
{
    void Start()
    {
        // The compiler will ONLY include this next block if you are inside Unity
#if UNITY_EDITOR
        GameInput.Instance.OnDataWipeAction += WipeAllData;
#endif
    }

    public void WipeAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared.");

        string saveDirectory = Application.persistentDataPath;
        if (Directory.Exists(saveDirectory))
        {
            DirectoryInfo directory = new DirectoryInfo(saveDirectory);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            Debug.Log("Persistent Data cleared.");
        }
    }
}