using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene"; // Change to your actual game scene name!

    [Header("UI References")]
    public TextMeshProUGUI heightValueText;
    public GameObject settingsPopupPanel;
    public GameObject leaderboardPopupPanel;

    void Start()
    {
        // 1. Ensure cursor is unlocked (just in case we came from somewhere weird)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Load and display the Personal Best Height
        // We use "GetFloat" with a default value of 0 if they haven't played yet.
        float bestHeight = PlayerPrefs.GetFloat("PersonalBestHeight", 0f);
        heightValueText.text = Mathf.FloorToInt(bestHeight).ToString() + " M";
        
        // Ensure settings popup is closed
        settingsPopupPanel.SetActive(false);
        leaderboardPopupPanel.SetActive(false);
    }

    // --- BUTTON FUNCTIONS ---

    public void PlayGame()
    {
        // Optional: Add a sound effect here
        SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleLeaderboard(bool show)
    {
        leaderboardPopupPanel.SetActive(show);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game..."); // Shows in Unity Editor
        Application.Quit(); // Works in built game
    }

    // --- SETTINGS / RESET LOGIC ---

    public void ToggleSettingsPanel(bool show)
    {
        settingsPopupPanel.SetActive(show);
    }

    public void ResetAllSaveData()
    {
        // THE BIG RED BUTTON OPTION
        // This deletes absolutely everything saved in PlayerPrefs.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Data Wiped!");

        // Refresh the displayed height immediately (it should now be 0 M)
        heightValueText.text = "0 M";

        // Close the panel
        ToggleSettingsPanel(false);
    }
}