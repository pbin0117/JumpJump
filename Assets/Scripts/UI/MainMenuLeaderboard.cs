using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MainMenuLeaderboard : MonoBehaviour
{
    [Header("UI References")]
    public Transform rowContainer;    // The "LeaderboardPanel" with the Layout Group
    public GameObject rowPrefab;      // The "ScoreRow" prefab you just made


    // Internal Data
    private LeaderboardData currentData;

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
        // Load existing leaderboard
        currentData = LoadLeaderboard();

        UpdateUI(currentData);
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
            texts[0].color = Color.white;
            texts[1].color = Color.white;
            texts[2].color = Color.white;
            
        }
    }

    // --- SAVE / LOAD SYSTEM ---

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