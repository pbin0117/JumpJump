using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton: Lets other scripts call "GameManager.Instance" easily
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsPaused { get; private set; } = false;
    
    [Header("Player Tracking")]
    public Transform playerTransform;
    public float levelTopHeight = 500f; // The "Finish Line" Y position

    // Data Properties (Read-only for other scripts)
    public float CurrentTimer { get; private set; }
    public float CurrentHeight { get; private set; }
    public float MaxHeightReached { get; private set; }
    public int TotalJumps { get; private set; } 
    public int TotalFalls { get; private set; }

    void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    void Start()
    {
        // Reset state on load
        ResumeGame();
        CurrentTimer = 0f;
        MaxHeightReached = 0f;
        TotalJumps = 0;
        TotalFalls = 0;
    }

    void Update()
    {
        // 1. Input: Global Pause Toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // 2. Input: Quick Restart
        if (Input.GetKeyDown(KeyCode.R) && !IsPaused)
        {
            RestartRun();
        }

        // 3. Game Loop (Only run if playing)
        if (!IsPaused)
        {
            CurrentTimer += Time.deltaTime;

            if (playerTransform != null)
            {
                // Track Height
                CurrentHeight = playerTransform.position.y;

                // Track High Score (High Water Mark)
                if (CurrentHeight > MaxHeightReached)
                {
                    MaxHeightReached = CurrentHeight;

                    float allTimeBest = PlayerPrefs.GetFloat("PersonalBestHeight", 0f);
                    if (MaxHeightReached > allTimeBest)
                    {
                        PlayerPrefs.SetFloat("PersonalBestHeight", MaxHeightReached);
                        // No need to PlayerPrefs.Save() here, it causes lag. 
                        // Unity saves automatically on exit, or we do it at the end screen.
                    }
                }
            }
        }
    }

    // --- PUBLIC COMMANDS ---

    public void TogglePause()
    {
        IsPaused = !IsPaused;

        // Physics & Time Control
        Time.timeScale = IsPaused ? 0f : 1f;
        
        // (Optional) Unlock cursor if you were locking it
        Cursor.visible = IsPaused;
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RestartRun()
    {
        Time.timeScale = 1f; // Unfreeze before reloading!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }

    public void AddJump()
    {
        TotalJumps++;
    }

    public void AddFall()
    {
        TotalFalls++;
    }

    public void SaveStatsForEndScreen()
    {   
        PlayerPrefs.SetFloat("LastRunHeight", MaxHeightReached);
        PlayerPrefs.SetFloat("LastRunTime", CurrentTimer);
        PlayerPrefs.SetInt("LastRunJumps", TotalJumps);
        PlayerPrefs.SetInt("LastRunFalls", TotalFalls);
        PlayerPrefs.Save();
        
        Debug.Log("Stats Saved! Time: " + CurrentTimer);
    }
}