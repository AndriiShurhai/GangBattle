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

    private void Awake()
    {
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    private void Start()
    {
        optionsPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        settingsButton.onClick.RemoveListener(OpenSettings);
        mainMenuButton.onClick.RemoveListener(LoadMainMenu);
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
}