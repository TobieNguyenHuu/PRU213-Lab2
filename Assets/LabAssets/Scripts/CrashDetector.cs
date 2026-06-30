using UnityEngine;

public class CrashDetector : MonoBehaviour
{
    public float loadDelay = 1f;
    public ParticleSystem crashEffect;
    public AudioClip crashSFX;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsCrashTarget(other))
        {
            return;
        }

        TriggerCrash(other.transform.position, other.name);
    }

    bool IsCrashTarget(Collider2D other)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return false;
        }

        ObstacleHazard hazard = other.GetComponentInParent<ObstacleHazard>();
        if (hazard != null)
        {
            return hazard.CostsLife;
        }

        return other.CompareTag("Ground")
            || other.CompareTag("Obstacle");
    }

    void TriggerCrash(Vector2 impactPoint, string source)
    {
        if (crashEffect != null)
        {
            crashEffect.Play();
        }

        if (audioSource != null && crashSFX != null)
        {
            audioSource.PlayOneShot(crashSFX);
        }

        GameManager.Instance?.ApplyCrash(impactPoint, source);
    }
}
