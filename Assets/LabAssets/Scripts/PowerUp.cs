using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        SpeedBoost,
        Invincibility,
        Shortcut
    }

    [SerializeField] PowerUpType type;
    [SerializeField] int points = 125;
    [SerializeField] float duration = 4f;
    [SerializeField] float speedMultiplier = 1.5f;
    [SerializeField] Vector2 shortcutImpulse = new Vector2(12f, 3f);

    bool consumed;

    public void Configure(PowerUpType newType, int newPoints, float newDuration)
    {
        type = newType;
        points = newPoints;
        duration = newDuration;
    }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, -90f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed || !other.CompareTag("Player"))
        {
            return;
        }

        Driver driver = other.GetComponentInParent<Driver>();
        if (driver == null)
        {
            return;
        }

        consumed = true;

        switch (type)
        {
            case PowerUpType.SpeedBoost:
                driver.ApplyTemporaryBoost(speedMultiplier, duration);
                GameManager.Instance?.AddPowerUp("Speed boost", points);
                break;
            case PowerUpType.Invincibility:
                GameManager.Instance?.ActivateInvincibility(duration);
                GameManager.Instance?.AddPowerUp("Invincibility", points);
                break;
            case PowerUpType.Shortcut:
                Rigidbody2D rb = driver.GetComponent<Rigidbody2D>();
                rb.AddForce(shortcutImpulse, ForceMode2D.Impulse);
                GameManager.Instance?.AddPowerUp("Shortcut", points);
                break;
        }

        gameObject.SetActive(false);
    }

    public void ResetPowerUp()
    {
        consumed = false;
        gameObject.SetActive(true);
    }
}
