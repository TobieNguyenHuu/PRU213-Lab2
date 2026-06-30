using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    const string RootName = "__MusicManager";

    [Header("--- AUDIO CLIPS (Kéo file nhạc vào đây) ---")]
    public AudioClip menuMusicClip;
    public AudioClip gameplayMusicClip;
    public AudioClip endGameMusicClip;

    [Header("--- SETTINGS ---")]
    [Range(0f, 1f)] public float menuVolume = 0.36f;
    [Range(0f, 1f)] public float gameplayVolume = 0.34f;
    [Range(0f, 1f)] public float endGameVolume = 0.40f;

    static MusicManager instance;
    AudioSource source;
    AudioClip currentClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Ensure().PlayForScene(SceneManager.GetActiveScene().name);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Ensure().PlayForScene(scene.name);
    }

    static MusicManager Ensure()
    {
        if (instance != null) return instance;

        GameObject existing = GameObject.Find(RootName);
        if (existing != null && existing.TryGetComponent(out MusicManager existingManager))
        {
            instance = existingManager;
            return instance;
        }

        GameObject managerObject = new GameObject(RootName);
        instance = managerObject.AddComponent<MusicManager>();
        DontDestroyOnLoad(managerObject);
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    void PlayForScene(string sceneName)
    {
        AudioClip targetClip = GetMusicClip(sceneName);
        if (targetClip == null)
        {
            StopMusic();
            return;
        }

        float targetVolume = GetVolume(sceneName);

        if (currentClip == targetClip && source != null && source.isPlaying)
        {
            source.volume = targetVolume;
            return;
        }

        currentClip = targetClip;
        source.clip = targetClip;
        source.volume = targetVolume;
        source.Play();
    }

    AudioClip GetMusicClip(string sceneName)
    {
        if (sceneName == SceneFlow.MenuSceneName) return menuMusicClip;
        if (sceneName == SceneFlow.EndGameSceneName) return endGameMusicClip;
        return SceneFlow.IsGameScene(sceneName) ? gameplayMusicClip : null;
    }

    float GetVolume(string sceneName)
    {
        if (sceneName == SceneFlow.MenuSceneName) return menuVolume;
        if (sceneName == SceneFlow.EndGameSceneName) return endGameVolume;
        return SceneFlow.IsGameScene(sceneName) ? gameplayVolume : 0.36f;
    }

    void StopMusic()
    {
        currentClip = null;
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }
    }
}