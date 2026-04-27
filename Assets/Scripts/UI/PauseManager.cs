using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public event Action OnPause;
    public event Action OnResume;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Mobile Button Prefab")]
    [SerializeField] private Button pauseToggleButtonPrefab;

    private Canvas mobileCanvas;
    private Button mobilePauseButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += TogglePause;
        SceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;

        HandleSceneLoadCompleted(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnPauseAction -= TogglePause;

        SceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
    }

    private void HandleSceneLoadCompleted(string sceneName)
    {
        bool gameplayScene =
            sceneName != "MainMenu" &&
            sceneName != "LevelSelection";

        if (Application.isMobilePlatform && gameplayScene)
        {
            CreateMobileCanvasIfNeeded();
            mobileCanvas.gameObject.SetActive(true);
        }
        else
        {
            if (mobileCanvas != null)
                mobileCanvas.gameObject.SetActive(false);
        }
    }

    private void CreateMobileCanvasIfNeeded()
    {
        if (mobileCanvas != null) return;

        GameObject canvasObj = new GameObject("MobileControlsCanvas");

        mobileCanvas = canvasObj.AddComponent<Canvas>();
        mobileCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mobileCanvas.sortingOrder = 100;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        DontDestroyOnLoad(canvasObj);

        CreatePauseButton();
    }

    private void CreatePauseButton()
    {
        mobilePauseButton = Instantiate(
            pauseToggleButtonPrefab,
            mobileCanvas.transform
        );

        mobilePauseButton.onClick.AddListener(TogglePause);

        RectTransform rect = mobilePauseButton.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);

        rect.sizeDelta = new Vector2(120, 120);
        rect.anchoredPosition = new Vector2(-30, -30);
    }

    public void TogglePause()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MainMenu" || scene == "LevelSelection")
            return;

        if (Time.timeScale > 0f)
            PauseGame();
        else
            ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        AudioManager.Instance?.PauseAllSFX();
        OnPause?.Invoke();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        CloseSettings();
        pauseMenuUI.SetActive(false);
        AudioManager.Instance?.ResumeAllSFX();
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