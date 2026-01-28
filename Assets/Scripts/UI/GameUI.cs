using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering; // For Volume

public class GameUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI timerText;
    public Slider currentHeightSlider;
    public Slider highScoreSlider; // The "Ghost" slider
    public GameObject pauseMenuPanel;

    [Header("Visual Settings")]
    public Volume globalVolume;
    private bool isHighQuality = true;

    void Start()
    {
        // Sync visuals state
        if (globalVolume != null) isHighQuality = globalVolume.enabled;
        
        // Setup Slider Ranges
        if (GameManager.Instance != null)
        {
            float top = GameManager.Instance.levelTopHeight;
            if (currentHeightSlider != null) currentHeightSlider.maxValue = top;
            if (highScoreSlider != null) highScoreSlider.maxValue = top;
        }
    }

    void Update()
    {
        // Safety Check
        if (GameManager.Instance == null) return;

        // 1. Sync UI Visibility
        // Show pause menu if GM says we are paused
        if (pauseMenuPanel != null)
        {
            if (pauseMenuPanel.activeSelf != GameManager.Instance.IsPaused)
            {
                pauseMenuPanel.SetActive(GameManager.Instance.IsPaused);
            }
        }

        // 2. Update Timer Text
        UpdateTimer(GameManager.Instance.CurrentTimer);

        // 3. Update Sliders
        if (currentHeightSlider != null)
            currentHeightSlider.value = GameManager.Instance.CurrentHeight;

        if (highScoreSlider != null)
            highScoreSlider.value = GameManager.Instance.MaxHeightReached;
    }

    void UpdateTimer(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        float milliseconds = (time % 1) * 100;
        timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    // --- BUTTON EVENTS (Link these in Inspector) ---

    public void OnPauseClicked()
    {
        GameManager.Instance.TogglePause();
    }

    public void OnRestartClicked()
    {
        GameManager.Instance.RestartRun();
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    public void OnVisualsToggleClicked()
    {
        // Visuals are strictly "View" logic, so keeping them here is fine!
        isHighQuality = !isHighQuality;
        if (globalVolume != null) globalVolume.enabled = isHighQuality;
    }
}