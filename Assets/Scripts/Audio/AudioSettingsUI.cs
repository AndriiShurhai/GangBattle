using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drop onto any Settings panel. Assign the sliders and toggles in the inspector —
/// everything else is wired automatically via AudioManager events.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mute Toggles")]
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Optional Labels (show % value)")]
    [SerializeField] private TMP_Text masterLabel;
    [SerializeField] private TMP_Text musicLabel;
    [SerializeField] private TMP_Text sfxLabel;

    [Header("Optional Reset Button")]
    [SerializeField] private Button resetButton;

    // Track whether we're pushing values into the UI so we don't
    // trigger callbacks and cause infinite loops.
    private bool isSyncing = false;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Slider ranges
        SetSliderRange(masterSlider);
        SetSliderRange(musicSlider);
        SetSliderRange(sfxSlider);
    }

    private void OnEnable()
    {
        // Subscribe to AudioManager events so external changes reflect in the UI
        AudioManager.OnMasterVolumeChanged += HandleMasterVolumeChanged;
        AudioManager.OnMusicVolumeChanged += HandleMusicVolumeChanged;
        AudioManager.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        AudioManager.OnMusicMuteChanged += HandleMusicMuteChanged;
        AudioManager.OnSfxMuteChanged += HandleSfxMuteChanged;

        // Wire slider callbacks
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        // Wire toggle callbacks
        if (musicMuteToggle != null) musicMuteToggle.onValueChanged.AddListener(OnMusicMuteToggled);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteToggled);

        // Wire reset button
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);

        // Sync UI to current AudioManager state
        SyncAllFromAudioManager();
    }

    private void OnDisable()
    {
        AudioManager.OnMasterVolumeChanged -= HandleMasterVolumeChanged;
        AudioManager.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
        AudioManager.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
        AudioManager.OnMusicMuteChanged -= HandleMusicMuteChanged;
        AudioManager.OnSfxMuteChanged -= HandleSfxMuteChanged;

        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

        if (musicMuteToggle != null) musicMuteToggle.onValueChanged.RemoveListener(OnMusicMuteToggled);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.RemoveListener(OnSfxMuteToggled);

        if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
    }

    // ─────────────────────────────────────────────
    //  Slider Callbacks  (UI → AudioManager)
    // ─────────────────────────────────────────────

    private void OnMasterSliderChanged(float value)
    {
        if (isSyncing || AudioManager.Instance == null) return;
        AudioManager.Instance.SetMasterVolume(value);
        UpdateLabel(masterLabel, value);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (isSyncing || AudioManager.Instance == null) return;
        AudioManager.Instance.SetMusicVolume(value);
        UpdateLabel(musicLabel, value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isSyncing || AudioManager.Instance == null) return;
        AudioManager.Instance.SetSfxVolume(value);
        UpdateLabel(sfxLabel, value);
    }

    // ─────────────────────────────────────────────
    //  Toggle Callbacks  (UI → AudioManager)
    // ─────────────────────────────────────────────

    private void OnMusicMuteToggled(bool isOn)
    {
        if (isSyncing || AudioManager.Instance == null) return;
        AudioManager.Instance.SetMusicMuted(isOn);
    }

    private void OnSfxMuteToggled(bool isOn)
    {
        if (isSyncing || AudioManager.Instance == null) return;
        AudioManager.Instance.SetSfxMuted(isOn);
    }

    private void OnResetClicked()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.ResetVolumeSettings();
    }

    // ─────────────────────────────────────────────
    //  AudioManager Event Handlers  (AudioManager → UI)
    // ─────────────────────────────────────────────

    private void HandleMasterVolumeChanged(float value)
    {
        SetSliderSilently(masterSlider, value);
        UpdateLabel(masterLabel, value);
    }

    private void HandleMusicVolumeChanged(float value)
    {
        SetSliderSilently(musicSlider, value);
        UpdateLabel(musicLabel, value);
    }

    private void HandleSfxVolumeChanged(float value)
    {
        SetSliderSilently(sfxSlider, value);
        UpdateLabel(sfxLabel, value);
    }

    private void HandleMusicMuteChanged(bool muted)
    {
        SetToggleSilently(musicMuteToggle, muted);
    }

    private void HandleSfxMuteChanged(bool muted)
    {
        SetToggleSilently(sfxMuteToggle, muted);
    }

    // ─────────────────────────────────────────────
    //  Sync Helpers
    // ─────────────────────────────────────────────

    private void SyncAllFromAudioManager()
    {
        if (AudioManager.Instance == null) return;

        isSyncing = true;

        SetSliderSilently(masterSlider, AudioManager.Instance.MasterVolume);
        SetSliderSilently(musicSlider, AudioManager.Instance.MusicVolume);
        SetSliderSilently(sfxSlider, AudioManager.Instance.SfxVolume);

        SetToggleSilently(musicMuteToggle, AudioManager.Instance.IsMusicMuted);
        SetToggleSilently(sfxMuteToggle, AudioManager.Instance.IsSfxMuted);

        UpdateLabel(masterLabel, AudioManager.Instance.MasterVolume);
        UpdateLabel(musicLabel, AudioManager.Instance.MusicVolume);
        UpdateLabel(sfxLabel, AudioManager.Instance.SfxVolume);

        isSyncing = false;
    }

    /// <summary>Set slider value without firing onValueChanged.</summary>
    private void SetSliderSilently(Slider slider, float value)
    {
        if (slider == null) return;
        isSyncing = true;
        slider.SetValueWithoutNotify(value);
        isSyncing = false;
    }

    /// <summary>Set toggle isOn without firing onValueChanged.</summary>
    private void SetToggleSilently(Toggle toggle, bool value)
    {
        if (toggle == null) return;
        isSyncing = true;
        toggle.SetIsOnWithoutNotify(value);
        isSyncing = false;
    }

    private void SetSliderRange(Slider slider)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    private void UpdateLabel(TMP_Text label, float value)
    {
        if (label == null) return;
        label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}