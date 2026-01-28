using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinTrigger : MonoBehaviour
{
    [Header("Cinematic Settings")]
    public float zoomDuration = 2.5f;    // How long the zoom lasts
    public float targetCamSize = 15f;    // How far to zoom out (try 12 or 15)
    public float exitForce = 20f;        // Speed of the "fly away"
    public CanvasGroup fadeOverlay;

    private bool hasTriggered = false;
    public GameManager myGameManager; 

    private void OnTriggerEnter(Collider other)
    {   
        // Check if it's the player hitting the trigger
        if (other.CompareTag("Player") && !hasTriggered)
        {   
            hasTriggered = true;

             // Tell manager to save stats to PlayerPrefs
             myGameManager.SaveStatsForEndScreen();

             // Load the end scene (Make sure to add it to Build Settings!)
             StartCoroutine(PlayWinSequence(other.gameObject));
        }
    }

    private IEnumerator PlayWinSequence(GameObject player)
    {
        var controller = player.GetComponent<RagdollController>(); 
        if (controller != null) controller.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null) rb.gravityScale = 0.5f;

        Camera mainCam = Camera.main;
        float startSize = mainCam.orthographicSize;
        float elapsed = 0f; 

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            float percent = elapsed / zoomDuration;

            mainCam.orthographicSize = Mathf.SmoothStep(startSize, targetCamSize, percent);
            fadeOverlay.alpha = percent;

            yield return null;
        }

        SceneManager.LoadScene("Ending");
    }
}