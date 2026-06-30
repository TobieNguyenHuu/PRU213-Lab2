using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player State")]
    [SerializeField] int startingLives = 3;
    [SerializeField] float crashCooldown = 1.25f;
    [SerializeField] float obstacleBumpCooldown = 0.65f;
    [SerializeField] float postCrashInvincibility = 2f;

    [Header("Scoring")]
    [SerializeField] float speedScorePerSecond = 0.8f;
    [SerializeField] float comboTimeout = 4f;
    [SerializeField] float comboStep = 0.25f;
    [SerializeField] float maxMultiplier = 4f;
    [SerializeField] int finishBonus = 1000;

    Driver player;
    int score;
    int lives;
    int comboCount;
    int bestCombo;
    int snowflakes;
    float comboTimer;
    float multiplier = 1f;
    float currentSpeed;
    float lastCrashTime = -999f;
    float lastBumpTime = -999f;
    float invincibleTimer;
    float boostTimer;
    float messageTimer;
    bool playing;
    bool gameOver;
    bool finished;
    string statusMessage = "Press Start";

    public int Score => score;
    public int Lives => lives;
    public int ComboCount => comboCount;
    public int BestCombo => bestCombo;
    public int Snowflakes => snowflakes;
    public float Multiplier => multiplier;
    public float CurrentSpeed => currentSpeed;
    public float InvincibleTimeLeft => invincibleTimer;
    public float BoostTimeLeft => boostTimer;
    public bool ManualBoostReady => player == null || player.IsManualBoostReady;
    public bool ManualBoostActive => player != null && player.IsManualBoostActive;
    public float ManualBoostEnergy01 => player == null ? 1f : player.BoostEnergy01;
    public bool IsPlaying => playing;
    public bool IsGameOver => gameOver;
    public bool IsFinished => finished;
    public bool IsInvincible => invincibleTimer > 0f;
    public string StatusMessage => statusMessage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        lives = startingLives;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (playing && player != null)
        {
            currentSpeed = player.Speed;
            if (currentSpeed > 3f)
            {
                AddRawScore(Mathf.RoundToInt(currentSpeed * speedScorePerSecond * deltaTime));
            }
        }

        if (comboTimer > 0f)
        {
            comboTimer -= deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        if (invincibleTimer > 0f)
        {
            invincibleTimer -= deltaTime;
        }

        if (boostTimer > 0f)
        {
            boostTimer -= deltaTime;
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f && playing)
            {
                statusMessage = string.Empty;
            }
        }

        if (SnowboardInput.RestartPressed())
        {
            StartGame();
        }
    }

    public static GameManager Ensure()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject("Game Manager");
        return managerObject.AddComponent<GameManager>();
    }

    public void RegisterPlayer(Driver driver)
    {
        player = driver;
    }

    public void OpenMainMenu()
    {
        playing = false;
        Time.timeScale = 0f;
        ShowMessage("Press Start", 999f);
    }

    public void StartGame()
    {
        score = 0;
        lives = startingLives;
        bestCombo = 0;
        snowflakes = 0;
        gameOver = false;
        finished = false;
        lastCrashTime = -999f;
        lastBumpTime = -999f;
        invincibleTimer = 0f;
        boostTimer = 0f;
        ResetCombo();
        playing = true;
        Time.timeScale = 1f;
        ShowMessage($"Level {SceneFlow.CurrentLevelNumber} - Ride!", 1.2f);

        SnowboardBootstrap.ResetChallenges();

        if (player != null)
        {
            player.ResetRunState();
        }
    }

    public void TogglePause()
    {
        if (gameOver || finished)
        {
            return;
        }

        playing = !playing;
        Time.timeScale = playing ? 1f : 0f;
        ShowMessage(playing ? "Ride!" : "Paused", playing ? 1f : 999f);
    }

    public void FinishRace()
    {
        if (!playing || finished)
        {
            return;
        }

        finished = true;
        playing = false;
        AddRawScore(finishBonus);
        ShowMessage($"Level complete! Bonus +{finishBonus}", 999f);
        SceneFlow.CompleteLevel(score, snowflakes, bestCombo);
    }

    public void ApplyCrash(Vector2 impactPoint, string source)
    {
        if (!playing || Time.time - lastCrashTime < crashCooldown)
        {
            return;
        }

        lastCrashTime = Time.time;

        if (IsInvincible)
        {
            AddComboScore(75, "Shield break");
            ShowMessage("Invincible hit blocked", 1.5f);
            return;
        }

        lives = Mathf.Max(0, lives - 1);
        ResetCombo();
        ShowMessage($"{source} crash! Lives: {lives}", 2f);
        invincibleTimer = postCrashInvincibility;

        if (player != null)
        {
            player.SlowDownAfterCrash(impactPoint);
        }

        if (lives <= 0)
        {
            gameOver = true;
            playing = false;
            ShowMessage($"Game Over - Score {score}", 999f);
            SceneFlow.FailRun(score, snowflakes, bestCombo);
        }
    }

    public void ApplyObstacleBump(Vector2 impactPoint, string source)
    {
        if (!playing || Time.time - lastBumpTime < obstacleBumpCooldown)
        {
            return;
        }

        lastBumpTime = Time.time;
        ResetCombo();
        ShowMessage($"{source} bump - slowed", 1.4f);

        if (player != null)
        {
            player.SlowDownAfterCrash(impactPoint);
        }
    }

    public void FailOutOfBounds()
    {
        if (!playing || gameOver || finished)
        {
            return;
        }

        lives = 0;
        gameOver = true;
        playing = false;
        ResetCombo();
        ShowMessage($"Out of map - Score {score}", 999f);
        SceneFlow.FailRun(score, snowflakes, bestCombo);
    }

    public void AddCollectible(int points)
    {
        if (!playing)
        {
            return;
        }

        snowflakes++;
        AddComboScore(points, "Snowflake");
    }

    public void AddPowerUp(string powerUpName, int points)
    {
        if (!playing)
        {
            return;
        }

        AddComboScore(points, powerUpName);
    }

    public void ReachCheckpoint(int checkpointNumber, int points)
    {
        if (!playing)
        {
            return;
        }

        AddComboScore(points, $"Checkpoint {checkpointNumber}");
    }

    public void ActivateBoost(float duration)
    {
        boostTimer = Mathf.Max(boostTimer, duration);
        ShowMessage("Speed boost", 1.5f);
    }

    public void ActivateInvincibility(float duration)
    {
        invincibleTimer = Mathf.Max(invincibleTimer, duration);
        ShowMessage("Invincibility", 1.5f);
    }

    public void CompleteTrick(string trickName, int basePoints)
    {
        if (!playing)
        {
            return;
        }

        AddComboScore(basePoints, trickName);
    }

    void AddComboScore(int basePoints, string label)
    {
        comboCount++;
        bestCombo = Mathf.Max(bestCombo, comboCount);
        multiplier = Mathf.Min(maxMultiplier, 1f + comboCount * comboStep);
        comboTimer = comboTimeout;

        int awarded = Mathf.RoundToInt(basePoints * multiplier);
        AddRawScore(awarded);
        ShowMessage($"{label} +{awarded} x{multiplier:0.0}", 1.7f);
    }

    void AddRawScore(int points)
    {
        if (points <= 0)
        {
            return;
        }

        score += points;
    }

    void ResetCombo()
    {
        comboCount = 0;
        comboTimer = 0f;
        multiplier = 1f;
    }

    void ShowMessage(string message, float duration)
    {
        statusMessage = message;
        messageTimer = duration;
    }
}
