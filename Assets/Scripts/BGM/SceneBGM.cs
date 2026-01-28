using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    public AudioClip bgm;
    public float volume = 0.2f;

    void Start()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayBGM(bgm, volume);
    }
}