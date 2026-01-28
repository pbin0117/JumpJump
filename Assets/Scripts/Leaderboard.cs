using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [Header("UI References")]
    public Transform rowContainer;    // The "LeaderboardPanel" with the Layout Group
    public GameObject rowPrefab;      // The "ScoreRow" prefab you just made

    [Header("Configuration")]
    public int maxEntries = 5;        // Only keep top 5

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
        // 1. Load existing data
        LeaderboardData data = LoadLeaderboard();

        // 2. Add CURRENT run (from PlayerPrefs)
        float currentRunTime = PlayerPrefs.GetFloat("LastRunTime", 0);
        
        // Only add if it's a valid run (time > 0)
        if (currentRunTime > 0)
        {
            ScoreEntry newEntry = new ScoreEntry { name = "YOU", time = currentRunTime };
            data.scores.Add(newEntry);
        }

        // 3. Sort the list (Shortest time is better)
        data.scores.Sort((a, b) => a.time.CompareTo(b.time));

        // 4. Trim to top 5
        if (data.scores.Count > maxEntries)
        {
            data.scores.RemoveRange(maxEntries, data.scores.Count - maxEntries);
        }

        // 5. Save the updated list
        SaveLeaderboard(data);

        // 6. Display it
        UpdateUI(data);
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
            if (data.scores[i].name == "YOU")
            {
                texts[1].color = Color.yellow;
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