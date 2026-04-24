using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject[] titleText;
    [SerializeField] private GameObject creditsPanel;
    private void Awake()
    {
        playButton.onClick.AddListener(HandlePlayClicked);
        optionsButton.onClick.AddListener(HandleOptionsClicked);
        creditsButton.onClick.AddListener(HandleCreditsClicked);
        quitButton.onClick.AddListener(HandleQuitClicked);
    }
    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(HandlePlayClicked);
        optionsButton.onClick.RemoveListener(HandleOptionsClicked);
        creditsButton.onClick.AddListener(HandleCreditsClicked);
        quitButton.onClick.RemoveListener(HandleQuitClicked);
    }
    private void HandlePlayClicked()
    {
        SceneLoader.Instance.LoadScene("LevelSelection");
    }
    private void HandleOptionsClicked()
    {
        AudioSettingsUI.Instance.gameObject.SetActive(true);
    }

    private void HandleCreditsClicked()
    {
        if (creditsPanel == null) return;
        creditsPanel.SetActive(true);
    }
    private void HandleQuitClicked()
    {
        Application.Quit();
    }
}