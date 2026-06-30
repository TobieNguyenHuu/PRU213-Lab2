using UnityEngine;

public class DustTrail : MonoBehaviour
{
    public ParticleSystem dustParticles;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground") && dustParticles != null)
        {
            dustParticles.Play();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground") && dustParticles != null)
        {
            dustParticles.Stop();
        }
    }
}
