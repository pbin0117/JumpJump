using UnityEngine;
using TMPro;

public class RunSummary : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI fallsText;
    public TextMeshProUGUI jumpsText;

    void Start()
    {
        float t = PlayerPrefs.GetFloat("LastRunTime", 0);
        int f = PlayerPrefs.GetInt("LastRunFalls", 0);
        int j = PlayerPrefs.GetInt("LastRunJumps", 0);

        // Format Time
        float minutes = Mathf.FloorToInt(t / 60);
        float seconds = Mathf.FloorToInt(t % 60);
        float milliseconds = (t % 1) * 100;

        timeText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        fallsText.text = f.ToString();
        jumpsText.text = j.ToString("N0"); // "N0" adds commas like 1,024
    }
}