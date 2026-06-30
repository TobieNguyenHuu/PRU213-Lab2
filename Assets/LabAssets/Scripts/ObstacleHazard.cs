using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObstacleHazard : MonoBehaviour
{
    [SerializeField] int crashPenalty = 50;
    [SerializeField] bool costsLife;
    [SerializeField] float hitCooldown = 0.75f;

    float lastHitTime = -999f;

    public bool CostsLife => costsLife;

    public void Configure(bool shouldCostLife, int penalty)
    {
        costsLife = shouldCostLife;
        crashPenalty = Mathf.Max(0, penalty);
    }

    void Reset()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        collider2D.isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        Driver driver = collision.collider.GetComponentInParent<Driver>();
        if (driver == null)
        {
            return;
        }

        Vector2 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : (Vector2)transform.position;

        HandleHit(driver, impactPoint);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        Driver driver = other.GetComponentInParent<Driver>();
        if (driver == null)
        {
            return;
        }

        HandleHit(driver, transform.position);
    }

    void HandleHit(Driver driver, Vector2 impactPoint)
    {
        if (driver == null || Time.time - lastHitTime < hitCooldown)
        {
            return;
        }

        lastHitTime = Time.time;

        bool wasInvincible = GameManager.Instance.IsInvincible;
        if (costsLife)
        {
            GameManager.Instance.ApplyCrash(impactPoint, gameObject.name);
            if (wasInvincible)
            {
                GameManager.Instance.CompleteTrick("Obstacle smash", crashPenalty);
            }

            return;
        }

        if (wasInvincible)
        {
            GameManager.Instance.CompleteTrick("Rock smash", Mathf.Max(75, crashPenalty));
            return;
        }

        GameManager.Instance.ApplyObstacleBump(impactPoint, gameObject.name);
    }
}
