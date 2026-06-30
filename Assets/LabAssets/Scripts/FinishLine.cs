using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public float loadDelay = 1f;
    public ParticleSystem finishEffect;

    bool finished;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (finished || !other.CompareTag("Player"))
        {
            return;
        }

        finished = true;

        if (finishEffect != null)
        {
            finishEffect.Play();
        }

        GameManager.Instance?.FinishRace();
    }

    public void ResetFinish()
    {
        finished = false;
    }
}
