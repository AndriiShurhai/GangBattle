using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public event Action OnPause;
    public event Action OnResume;

    [SerializeField] private GameObject pauseMenuUI;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += TogglePause;
    }

    public void TogglePause()
    {
        if (SceneManager.GetActiveScene().name == "LevelSelection" || SceneManager.GetActiveScene().name == "MainMenu") return;
        if (Time.timeScale > 0)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        AudioManager.Instance.PauseAllSFX();
        OnPause?.Invoke();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        CloseSettings();
        pauseMenuUI.SetActive(false);
        AudioManager.Instance.ResumeAllSFX();
        OnResume?.Invoke();
    }

    public void OpenSettings()
    {
        AudioSettingsUI.Instance?.gameObject.SetActive(true);
    }   

    public void CloseSettings()
    {
        AudioSettingsUI.Instance?.gameObject.SetActive(false);
    }
    public void QuitGame()
    {
        CloseSettings();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        SceneLoader.Instance?.LoadScene("LevelSelection");
    }
}