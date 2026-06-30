using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JumpRamp : MonoBehaviour
{
    [SerializeField] float upwardImpulse = 9.5f;
    [SerializeField] float forwardImpulse = 3f;
    [SerializeField] float launchCooldown = 0.45f;

    float lastLaunchTime = -999f;

    public void Configure(float upward, float forward)
    {
        upwardImpulse = upward;
        forwardImpulse = forward;
    }

    void Reset()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        collider2D.isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryLaunch(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryLaunch(collision.collider);
    }

    void TryLaunch(Collider2D other)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        Driver driver = other.GetComponentInParent<Driver>();
        if (driver == null || Time.time - lastLaunchTime < launchCooldown)
        {
            return;
        }

        Rigidbody2D body = driver.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            return;
        }

        lastLaunchTime = Time.time;
        body.AddForce(new Vector2(forwardImpulse, upwardImpulse), ForceMode2D.Impulse);
    }

    public void ResetRamp()
    {
        lastLaunchTime = -999f;
    }
}
