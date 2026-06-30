using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Driver : MonoBehaviour
{
    [Header("Scene Tuned Values")]
    public float torqueAmount = 17f;
    public float boostSpeed = 62f;
    public float baseSpeed = 32f;

    [Header("Movement")]
    [SerializeField] float surfaceSpeedLerp = 0.26f;
    [SerializeField] float brakeDamping = 0.12f;
    [SerializeField] float carveForce = 6.5f;
    [SerializeField] float airDriftForce = 2.5f;
    [SerializeField] float airDiveForce = 22f;
    [SerializeField] float maxSpeed = 72f;
    [SerializeField] float groundedLeanMultiplier = 0.75f;
    [SerializeField] float airFlipMultiplier = 3.05f;
    [SerializeField] float crashSlowMultiplier = 0.35f;

    [Header("Boost Energy")]
    [SerializeField] float boostEnergyMax = 1f;
    [SerializeField] float boostEnergyDrainPerSecond = 0.52f;
    [SerializeField] float boostEnergyRegenPerSecond = 0.36f;
    [SerializeField] float minBoostEnergyToStart = 0.08f;

    [Header("Tricks")]
    [SerializeField] float minTrickAirTime = 0.18f;
    [SerializeField] float minTrickRotation = 40f;
    [SerializeField] float bigTrickRotation = 180f;
    [SerializeField] int smallTrickScore = 150;
    [SerializeField] int bigTrickScore = 400;

    Rigidbody2D rb;
    SurfaceEffector2D activeSurfaceEffector;
    float lastGroundedTime = -999f;
    float airTime;
    float airRotation;
    float lastAirAngle;
    float temporaryBoostMultiplier = 1f;
    float boostTimer;
    float boostEnergy = 1f;
    float outOfBoundsMinX;
    float outOfBoundsMaxX;
    float outOfBoundsMinY;
    Vector3 startPosition;
    Quaternion startRotation;
    bool wasGrounded = true;
    bool outOfBoundsConfigured;
    bool manualBoostActive;

    public float Speed => rb == null ? 0f : rb.linearVelocity.magnitude;
    public bool IsGrounded => Time.time - lastGroundedTime < 0.14f;
    public bool IsAirborne => !IsGrounded;
    public bool IsManualBoostActive => manualBoostActive;
    public bool IsManualBoostReady => boostEnergy >= minBoostEnergyToStart;
    public float BoostEnergy01 => Mathf.Clamp01(boostEnergy / Mathf.Max(0.01f, boostEnergyMax));

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boostEnergy = boostEnergyMax;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Start()
    {
        GameManager.Ensure().RegisterPlayer(this);
    }

    void Update()
    {
        if (SnowboardInput.PausePressed() && GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }

        UpdateBoostTimer();
        UpdateManualBoost();
        CheckOutOfBounds();
        UpdateTrickTracking();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        float horizontal = SnowboardInput.Horizontal();
        float vertical = SnowboardInput.Vertical();
        bool manualBoostActive = IsManualBoostActive;
        bool brakeHeld = vertical < -0.1f || SnowboardInput.BrakeHeld();
        bool forwardHeld = vertical > 0.1f;
        bool grounded = IsGrounded;
        bool brakingOnGround = brakeHeld && grounded;
        bool divingInAir = brakeHeld && !grounded;
        float leanControl = grounded ? groundedLeanMultiplier : airFlipMultiplier;

        rb.AddTorque(-horizontal * torqueAmount * leanControl, ForceMode2D.Force);
        ApplyCarveMovement(horizontal);

        float targetSurfaceSpeed = baseSpeed;
        if (forwardHeld)
        {
            targetSurfaceSpeed = baseSpeed * 1.35f;
        }

        if (brakingOnGround)
        {
            targetSurfaceSpeed = baseSpeed * 0.35f;
        }

        if (manualBoostActive)
        {
            targetSurfaceSpeed = boostSpeed;
        }

        targetSurfaceSpeed *= temporaryBoostMultiplier;

        if (activeSurfaceEffector != null)
        {
            activeSurfaceEffector.speed = Mathf.Lerp(activeSurfaceEffector.speed, targetSurfaceSpeed, surfaceSpeedLerp);
        }

        if (brakingOnGround && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, rb.linearVelocity * 0.62f, brakeDamping);
        }

        if (divingInAir)
        {
            rb.AddForce(Vector2.down * airDiveForce, ForceMode2D.Force);
        }

        ClampSpeed(manualBoostActive);
    }

    void ApplyCarveMovement(float horizontal)
    {
        if (Mathf.Abs(horizontal) < 0.01f)
        {
            return;
        }

        float controlForce = IsGrounded ? carveForce : airDriftForce;
        rb.AddForce(Vector2.right * horizontal * controlForce, ForceMode2D.Force);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CacheGroundContact(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CacheGroundContact(collision);
    }

    void CacheGroundContact(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Ground") && collision.collider.GetComponent<SurfaceEffector2D>() == null)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.05f)
            {
                lastGroundedTime = Time.time;
                activeSurfaceEffector = collision.collider.GetComponent<SurfaceEffector2D>();
                return;
            }
        }
    }

    void ClampSpeed(bool boosting)
    {
        float allowedSpeed = (boosting ? Mathf.Max(maxSpeed, boostSpeed) : maxSpeed) * temporaryBoostMultiplier;
        if (rb.linearVelocity.magnitude > allowedSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * allowedSpeed;
        }
    }

    void UpdateBoostTimer()
    {
        if (boostTimer <= 0f)
        {
            temporaryBoostMultiplier = 1f;
            return;
        }

        boostTimer -= Time.deltaTime;
        if (boostTimer <= 0f)
        {
            temporaryBoostMultiplier = 1f;
        }
    }

    void UpdateManualBoost()
    {
        manualBoostActive = false;

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        bool boostHeld = SnowboardInput.BoostHeld();
        if (boostHeld && boostEnergy > 0f)
        {
            manualBoostActive = true;
            boostEnergy = Mathf.Max(0f, boostEnergy - boostEnergyDrainPerSecond * Time.deltaTime);
            return;
        }

        if (!boostHeld)
        {
            boostEnergy = Mathf.Min(boostEnergyMax, boostEnergy + boostEnergyRegenPerSecond * Time.deltaTime);
        }
    }

    void UpdateTrickTracking()
    {
        bool grounded = IsGrounded;

        if (!grounded)
        {
            if (wasGrounded)
            {
                airTime = 0f;
                airRotation = 0f;
                lastAirAngle = rb.rotation;
            }

            airTime += Time.deltaTime;
            airRotation += Mathf.Abs(Mathf.DeltaAngle(lastAirAngle, rb.rotation));
            lastAirAngle = rb.rotation;
        }
        else if (!wasGrounded)
        {
            TryScoreLandingTrick();
            airTime = 0f;
            airRotation = 0f;
        }

        wasGrounded = grounded;
    }

    void TryScoreLandingTrick()
    {
        if (airTime < minTrickAirTime || airRotation < minTrickRotation || GameManager.Instance == null)
        {
            return;
        }

        bool bigTrick = airRotation >= bigTrickRotation;
        string trickName = bigTrick ? "Full rotation" : "Air trick";
        int trickScore = bigTrick ? bigTrickScore : smallTrickScore;
        GameManager.Instance.CompleteTrick(trickName, trickScore);
    }

    public void ApplyTemporaryBoost(float multiplier, float duration)
    {
        temporaryBoostMultiplier = Mathf.Max(temporaryBoostMultiplier, multiplier);
        boostTimer = Mathf.Max(boostTimer, duration);
        GameManager.Instance?.ActivateBoost(duration);
    }

    public void ConfigureOutOfBounds(float minX, float maxX, float minY)
    {
        outOfBoundsMinX = minX;
        outOfBoundsMaxX = maxX;
        outOfBoundsMinY = minY;
        outOfBoundsConfigured = true;
    }

    void CheckOutOfBounds()
    {
        if (!outOfBoundsConfigured || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        Vector3 position = transform.position;
        if (position.y < outOfBoundsMinY || position.x < outOfBoundsMinX || position.x > outOfBoundsMaxX)
        {
            GameManager.Instance.FailOutOfBounds();
        }
    }

    public void SlowDownAfterCrash(Vector2 impactPoint)
    {
        rb.linearVelocity *= crashSlowMultiplier;
        rb.angularVelocity *= crashSlowMultiplier;

        Vector2 away = ((Vector2)transform.position - impactPoint).normalized;
        if (away.sqrMagnitude > 0.01f)
        {
            rb.AddForce(away * 4f, ForceMode2D.Impulse);
        }
    }

    public void ResetRunState()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        temporaryBoostMultiplier = 1f;
        boostTimer = 0f;
        boostEnergy = boostEnergyMax;
        manualBoostActive = false;
        airTime = 0f;
        airRotation = 0f;
        lastGroundedTime = Time.time;
    }
}
