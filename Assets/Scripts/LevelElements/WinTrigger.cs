using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    // Reference to your manager that holds the stats
    public GameManager myGameManager; 

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player hitting the trigger
        if (other.CompareTag("Player"))
        {
             // 1. Tell manager to save stats to PlayerPrefs
             myGameManager.SaveStatsForEndScreen();

             // 2. Load the end scene (Make sure to add it to Build Settings!)
             SceneManager.LoadScene("EndScreenScene");
        }
    }
}