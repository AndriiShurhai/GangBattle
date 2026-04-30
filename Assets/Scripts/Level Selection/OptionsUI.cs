using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class OptionsUI : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitGameButton;

    private void Awake()
    {
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        quitGameButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += ToggleOptionsPanel;
        optionsPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        GameInput.Instance.OnPauseAction -= ToggleOptionsPanel;
        settingsButton.onClick.RemoveListener(OpenSettings);
        mainMenuButton.onClick.RemoveListener(LoadMainMenu);
        quitGameButton.onClick.RemoveListener(QuitGame);
    }

    public void ToggleOptionsPanel()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }   

    private void OpenSettings()
    {
        AudioSettingsUI.Instance?.gameObject.SetActive(true);
    }

    private void LoadMainMenu()
    {
        SceneLoader.Instance.LoadScene("MainMenu");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}