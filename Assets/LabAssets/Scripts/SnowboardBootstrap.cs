using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnowboardBootstrap : MonoBehaviour
{
    const string RuntimeRootName = "__SnowboardRuntime";

    static readonly Vector2[] TrackGuidePositions =
    {
        new Vector2(-36f, 14f),
        new Vector2(-24f, 12f),
        new Vector2(-10f, 7f),
        new Vector2(6f, 4f),
        new Vector2(22f, -1f),
        new Vector2(42f, -5f),
        new Vector2(66f, -11f),
        new Vector2(92f, -18f),
        new Vector2(110f, -23f),
        new Vector2(130f, -29f),
        new Vector2(151f, -34f),
        new Vector2(171f, -40f)
    };

    static Dictionary<Color, Sprite> spriteCache = new Dictionary<Color, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootstrapLoadedScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureScene();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureScene();
    }

    public static void ResetChallenges()
    {
        foreach (PowerUp powerUp in FindObjectsByType<PowerUp>(FindObjectsInactive.Include))
        {
            powerUp.ResetPowerUp();
        }

        foreach (TrickZone trickZone in FindObjectsByType<TrickZone>(FindObjectsInactive.Include))
        {
            trickZone.ResetZone();
        }

        foreach (FinishLine finishLine in FindObjectsByType<FinishLine>(FindObjectsInactive.Include))
        {
            finishLine.ResetFinish();
        }

        foreach (JumpRamp ramp in FindObjectsByType<JumpRamp>(FindObjectsInactive.Include))
        {
            ramp.ResetRamp();
        }
    }

    static void ConfigureScene()
    {
        if (!SceneFlow.IsGameScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (GameObject.Find(RuntimeRootName) != null)
        {
            return;
        }

        GameObject runtimeRoot = new GameObject(RuntimeRootName);
        GameManager manager = GameManager.Ensure();
        GameObject player = FindPlayer();

        ConfigurePlayer(player, manager);
        ConfigureGround();
        ConfigureOutOfBounds(player);
        ConfigureScenery();
        ConfigureFinishLine();
        ConfigureCamera(player);
        CreateChallengeObjects(runtimeRoot.transform);
        CreateWeather(runtimeRoot.transform, player);
        GameHud.Ensure();
        manager.StartGame();
    }

    static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player;
        }

        return GameObject.Find("Barry");
    }

    static void ConfigurePlayer(GameObject player, GameManager manager)
    {
        if (player == null)
        {
            return;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = Mathf.Max(rb.gravityScale, 2.4f);
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        Driver driver = player.GetComponent<Driver>();
        if (driver == null)
        {
            driver = player.AddComponent<Driver>();
        }

        if (player.GetComponent<CrashDetector>() == null)
        {
            player.AddComponent<CrashDetector>();
        }

        if (player.GetComponent<BoarderAnimator2D>() == null)
        {
            player.AddComponent<BoarderAnimator2D>();
        }

        manager.RegisterPlayer(driver);
    }

    static void ConfigureGround()
    {
        PhysicsMaterial2D snowMaterial = new PhysicsMaterial2D("Runtime Snow")
        {
            friction = 0.04f,
            bounciness = 0.02f
        };

        float levelSpeed = 32f + (SceneFlow.CurrentLevelNumber - 1) * 4f;

        foreach (Collider2D collider2D in FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude))
        {
            if (collider2D.CompareTag("Ground") || collider2D.name.Contains("Level Sprite Shape"))
            {
                collider2D.sharedMaterial = snowMaterial;
                collider2D.usedByEffector = true;

                SurfaceEffector2D effector = collider2D.GetComponent<SurfaceEffector2D>();
                if (effector != null)
                {
                    effector.useFriction = true;
                    effector.useBounce = true;
                    effector.speed = levelSpeed;
                }
            }
        }
    }

    static void ConfigureOutOfBounds(GameObject player)
    {
        if (player == null || !TryGetGroundBounds(out Bounds groundBounds))
        {
            return;
        }

        Driver driver = player.GetComponent<Driver>();
        if (driver == null)
        {
            return;
        }

        driver.ConfigureOutOfBounds(
            groundBounds.min.x - 35f,
            groundBounds.max.x + 35f,
            groundBounds.min.y - 22f);
    }

    static bool TryGetGroundBounds(out Bounds groundBounds)
    {
        bool hasBounds = false;
        groundBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Collider2D collider2D in FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude))
        {
            if (!collider2D.CompareTag("Ground") && !collider2D.name.Contains("Level Sprite Shape"))
            {
                continue;
            }

            if (!hasBounds)
            {
                groundBounds = collider2D.bounds;
                hasBounds = true;
                continue;
            }

            groundBounds.Encapsulate(collider2D.bounds);
        }

        return hasBounds;
    }

    static void ConfigureScenery()
    {
        foreach (SpriteRenderer spriteRenderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
        {
            string objectName = spriteRenderer.gameObject.name;
            bool isRock = objectName.Contains("Snow-Rock");
            bool isTree = objectName.Contains("Snow-Tree");
            if (!isRock && !isTree)
            {
                continue;
            }

            if (isRock)
            {
                ConfigureLowRockHazard(spriteRenderer);
                continue;
            }

            ConfigureBackgroundTree(spriteRenderer);
        }
    }

    static void ConfigureLowRockHazard(SpriteRenderer spriteRenderer)
    {
        GameObject rock = spriteRenderer.gameObject;
        TrySetTag(rock, "Untagged");
        spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, 2);

        foreach (Collider2D collider in rock.GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }

        BoxCollider2D hazardCollider = rock.GetComponent<BoxCollider2D>();
        if (hazardCollider == null)
        {
            hazardCollider = rock.AddComponent<BoxCollider2D>();
        }

        Bounds bounds = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
        float width = Mathf.Max(0.55f, bounds.size.x * 0.52f);
        float height = Mathf.Max(0.12f, bounds.size.y * 0.18f);
        ObstacleHazard hazard = rock.GetComponent<ObstacleHazard>();
        if (hazard == null)
        {
            hazard = rock.AddComponent<ObstacleHazard>();
        }

        hazard.Configure(false, 75);
        hazardCollider.enabled = true;
        hazardCollider.isTrigger = true;
        hazardCollider.offset = new Vector2(bounds.center.x, bounds.min.y + height * 0.5f);
        hazardCollider.size = new Vector2(width, height);
    }

    static void ConfigureBackgroundTree(SpriteRenderer spriteRenderer)
    {
        GameObject tree = spriteRenderer.gameObject;
        TrySetTag(tree, "Untagged");

        foreach (Collider2D collider in tree.GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }

        foreach (ObstacleHazard hazard in tree.GetComponents<ObstacleHazard>())
        {
            Destroy(hazard);
        }
    }

    static void ConfigureFinishLine()
    {
        GameObject finish = GameObject.Find("Finish Line");
        if (finish == null)
        {
            return;
        }

        TrySetTag(finish, "Finish");
        if (finish.GetComponent<FinishLine>() == null)
        {
            finish.AddComponent<FinishLine>();
        }
    }

    static void ConfigureCamera(GameObject player)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || player == null)
        {
            return;
        }

        CameraFollow2D follow = mainCamera.GetComponent<CameraFollow2D>();
        if (follow == null)
        {
            follow = mainCamera.gameObject.AddComponent<CameraFollow2D>();
        }

        follow.SetTarget(player.transform);
    }

    static void CreateChallengeObjects(Transform parent)
    {
        CreatePowerUp("Speed Boost", PowerUp.PowerUpType.SpeedBoost, PositionOnTrack(-2f, 3.05f), new Color(1f, 0.83f, 0.16f, 1f), parent);
        CreatePowerUp("Invincibility", PowerUp.PowerUpType.Invincibility, PositionOnTrack(34f, 3.15f), new Color(0.35f, 1f, 0.55f, 1f), parent);
        CreatePowerUp("Shortcut", PowerUp.PowerUpType.Shortcut, PositionOnTrack(72f, 3.0f), new Color(1f, 0.45f, 0.85f, 1f), parent);
    }

    static float TrackCenterYAt(float x)
    {
        for (int i = 1; i < TrackGuidePositions.Length; i++)
        {
            Vector2 previous = TrackGuidePositions[i - 1];
            Vector2 next = TrackGuidePositions[i];
            if (x <= next.x)
            {
                float t = Mathf.InverseLerp(previous.x, next.x, x);
                return Mathf.Lerp(previous.y, next.y, t);
            }
        }

        Vector2 beforeLast = TrackGuidePositions[TrackGuidePositions.Length - 2];
        Vector2 last = TrackGuidePositions[TrackGuidePositions.Length - 1];
        float extrapolatedT = Mathf.InverseLerp(beforeLast.x, last.x, x);
        return Mathf.Lerp(beforeLast.y, last.y, extrapolatedT);
    }

    static Vector2 PositionOnTrack(float x, float yOffset)
    {
        return new Vector2(x, TrackCenterYAt(x) + yOffset);
    }

    static void CreatePowerUp(string name, PowerUp.PowerUpType type, Vector2 position, Color color, Transform parent)
    {
        GameObject powerUp = CreateSpriteObject(name, CreateDiscSprite(color, 32), position, 0.95f, parent);
        TrySetTag(powerUp, "PowerUp");

        CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;
        collider.isTrigger = true;

        PowerUp powerUpLogic = powerUp.AddComponent<PowerUp>();
        powerUpLogic.Configure(type, 125, 4f);
    }

    static GameObject CreateSpriteObject(string name, Sprite sprite, Vector2 position, float scale, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.position = position;
        gameObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 10;
        return gameObject;
    }

    static void CreateWeather(Transform parent, GameObject player)
    {
        GameObject snowObject = new GameObject("Falling Snow");
        snowObject.transform.SetParent(parent, false);

        ParticleSystem snow = snowObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = snow.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = snow.emission;
        emission.rateOverTime = 90f;

        ParticleSystem.ShapeModule shape = snow.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(34f, 1f, 1f);

        ParticleSystem.VelocityOverLifetimeModule velocity = snow.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        velocity.y = new ParticleSystem.MinMaxCurve(-2.5f, -1f);

        ParticleSystemRenderer renderer = snowObject.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 100;

        SnowWeather weather = snowObject.AddComponent<SnowWeather>();
        if (Camera.main != null)
        {
            weather.SetTarget(Camera.main.transform);
        }
        else if (player != null)
        {
            weather.SetTarget(player.transform);
        }
    }

    static Sprite CreateDiscSprite(Color color, int size)
    {
        if (spriteCache.TryGetValue(color, out Sprite sprite))
        {
            return sprite;
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.46f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance) / 2f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        spriteCache[color] = sprite;
        return sprite;
    }

    static void TrySetTag(GameObject gameObject, string tag)
    {
        try
        {
            gameObject.tag = tag;
        }
        catch (UnityException)
        {
            gameObject.tag = "Untagged";
        }
    }
}
