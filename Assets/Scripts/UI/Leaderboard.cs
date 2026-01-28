using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [Header("UI References")]
    public Transform rowContainer;    // The "LeaderboardPanel" with the Layout Group
    public GameObject rowPrefab;      // The "ScoreRow" prefab you just made

    [Header("Input Window")]
    public GameObject nameEntryPanel;      // Drag the Panel here
    public TMP_InputField nameInputField;  // Drag the Input Field here

    // Internal Data
    private LeaderboardData currentData;
    private float currentRunTime;

    // Internal Data Structure
    [System.Serializable]
    public class ScoreEntry
    {
        public string name;
        public float time;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<ScoreEntry> scores = new List<ScoreEntry>();
    }

    void Start()
    {
        // 1. Unlock Cursor (So you can click the button)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Load existing leaderboard
        currentData = LoadLeaderboard();

        UpdateUI(currentData);

        // 3. Check for a NEW run
        currentRunTime = PlayerPrefs.GetFloat("LastRunTime", 0);

        if (currentRunTime > 0)
        {
            // ALWAYS show the input box if there is a new time
            nameEntryPanel.SetActive(true);
        }
        else
        {
            // Just viewing the board (from Main Menu) - hide input, show list
            nameEntryPanel.SetActive(false);
            UpdateUI(currentData);
        }
    }

    public void SubmitName()
    {
        string playerName = nameInputField.text;
        
        // Optional: Default to "Anonymous" if they leave it blank
        if (string.IsNullOrEmpty(playerName)) playerName = "Anonymous";

        // 1. Add the new score
        ScoreEntry newEntry = new ScoreEntry { name = playerName, time = currentRunTime };
        currentData.scores.Add(newEntry);

        // 2. Sort (Fastest time at the top)
        currentData.scores.Sort((a, b) => a.time.CompareTo(b.time));

        // 3. Save and Update
        SaveLeaderboard(currentData);
        UpdateUI(currentData);

        // 4. Close the input window
        nameEntryPanel.SetActive(false);
        
        // 5. Clear the stored run so we don't save it twice
        PlayerPrefs.DeleteKey("LastRunTime");
    }

    void UpdateUI(LeaderboardData data)
    {
        // Clear old rows (if any)
        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn new rows
        for (int i = 0; i < data.scores.Count; i++)
        {
            GameObject row = Instantiate(rowPrefab, rowContainer);
            
            // Get the TMP components (Assumption: Child 0=Rank, 1=Name, 2=Time)
            // Ideally, make a separate script for the Row, but this is faster for now:
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            
            texts[0].text = (i + 1).ToString();         // Rank
            texts[1].text = data.scores[i].name;        // Name
            texts[2].text = FormatTime(data.scores[i].time); // Time

            // Highlight "YOU" with a different color (e.g., Orange)
            if (data.scores[i].time == currentRunTime && data.scores[i].name == nameInputField.text)
            {
                // Highlight the whole row (Rank, Name, and Time) for better visibility
                texts[0].color = Color.yellow; 
                texts[1].color = Color.yellow; 
                texts[2].color = Color.yellow; 
            }
            else
            {
                // (Optional) Ensure others are white (in case the prefab is saved as yellow)
                texts[0].color = Color.white;
                texts[1].color = Color.white;
                texts[2].color = Color.white;
            }
        }
    }

    // --- SAVE / LOAD SYSTEM ---
    
    void SaveLeaderboard(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("LeaderboardData", json);
        PlayerPrefs.Save();
    }

    LeaderboardData LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString("LeaderboardData", "{}");
        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        
        if (data == null) data = new LeaderboardData();

        return data;
    }

    string FormatTime(float timeInSeconds)
    {   
        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);
        float milliseconds = (timeInSeconds % 1) * 100;
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}