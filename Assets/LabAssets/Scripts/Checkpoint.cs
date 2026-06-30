using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] int checkpointNumber = 1;
    [SerializeField] int points = 200;

    bool reached;

    void Reset()
    {
        Collider2D checkpointCollider = GetComponent<Collider2D>();
        checkpointCollider.isTrigger = true;
    }

    public void Configure(int number, int scoreValue)
    {
        checkpointNumber = number;
        points = scoreValue;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        Driver driver = other.GetComponentInParent<Driver>();
        if (driver == null)
        {
            return;
        }

        reached = true;
        SetVisualReached(true);
        GameManager.Instance.ReachCheckpoint(checkpointNumber, points);
    }

    public void ResetCheckpoint()
    {
        reached = false;
        SetVisualReached(false);
    }

    void SetVisualReached(bool isReached)
    {
        Color color = isReached
            ? new Color(0.35f, 1f, 0.55f, 0.55f)
            : new Color(0.2f, 0.75f, 1f, 0.55f);

        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.color = color;
        }
    }
}
