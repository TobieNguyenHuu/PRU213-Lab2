using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlow
{
    public const string MenuSceneName = "Menu";
    public const string GameSceneName = "Level1";
    public const string Level2SceneName = "Level2";
    public const string Level3SceneName = "Level3";
    public const string EndGameSceneName = "EndGame";

    public static readonly string[] GameSceneNames =
    {
        GameSceneName,
        Level2SceneName,
        Level3SceneName
    };

    public static RunResult LastResult { get; private set; } = RunResult.Empty;
    public static int CampaignScore { get; private set; }
    public static int CampaignSnowflakes { get; private set; }
    public static int CampaignBestCombo { get; private set; }
    public static int CurrentLevelNumber => Mathf.Max(1, GetLevelIndex(SceneManager.GetActiveScene().name) + 1);

    public static void StoreResult(bool finished, int score, int snowflakes, int bestCombo)
    {
        LastResult = new RunResult(finished, score, snowflakes, bestCombo);
    }

    public static void ClearResult()
    {
        LastResult = RunResult.Empty;
        CampaignScore = 0;
        CampaignSnowflakes = 0;
        CampaignBestCombo = 0;
    }

    public static void LoadMenu()
    {
        Time.timeScale = 1f;
        LoadScene(MenuSceneName);
    }

    public static void LoadGame()
    {
        Time.timeScale = 1f;
        ClearResult();
        LoadScene(GameSceneName);
    }

    public static void CompleteLevel(int levelScore, int snowflakes, int bestCombo)
    {
        Time.timeScale = 1f;
        AddCampaignResult(levelScore, snowflakes, bestCombo);

        int currentLevel = GetLevelIndex(SceneManager.GetActiveScene().name);
        int nextLevel = currentLevel + 1;
        if (nextLevel >= 0 && nextLevel < GameSceneNames.Length)
        {
            LoadScene(GameSceneNames[nextLevel]);
            return;
        }

        StoreResult(true, CampaignScore, CampaignSnowflakes, CampaignBestCombo);
        LoadScene(EndGameSceneName);
    }

    public static void FailRun(int levelScore, int snowflakes, int bestCombo)
    {
        Time.timeScale = 1f;
        AddCampaignResult(levelScore, snowflakes, bestCombo);
        StoreResult(false, CampaignScore, CampaignSnowflakes, CampaignBestCombo);
        LoadScene(EndGameSceneName);
    }

    public static void LoadEndGame()
    {
        Time.timeScale = 1f;
        LoadScene(EndGameSceneName);
    }

    public static bool IsGameScene(string sceneName)
    {
        return GetLevelIndex(sceneName) >= 0;
    }

    static void AddCampaignResult(int levelScore, int snowflakes, int bestCombo)
    {
        CampaignScore += Mathf.Max(0, levelScore);
        CampaignSnowflakes += Mathf.Max(0, snowflakes);
        CampaignBestCombo = Mathf.Max(CampaignBestCombo, bestCombo);
    }

    static int GetLevelIndex(string sceneName)
    {
        for (int i = 0; i < GameSceneNames.Length; i++)
        {
            if (GameSceneNames[i] == sceneName)
            {
                return i;
            }
        }

        return -1;
    }

    static void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogWarning($"Scene '{sceneName}' is not in Build Settings. Add it to ProjectSettings/EditorBuildSettings.asset.");
    }
}

public readonly struct RunResult
{
    public static readonly RunResult Empty = new RunResult(false, 0, 0, 0);

    public readonly bool Finished;
    public readonly int Score;
    public readonly int Snowflakes;
    public readonly int BestCombo;

    public RunResult(bool finished, int score, int snowflakes, int bestCombo)
    {
        Finished = finished;
        Score = score;
        Snowflakes = snowflakes;
        BestCombo = bestCombo;
    }
}
