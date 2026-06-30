using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [SerializeField] int points = 100;
    [SerializeField] float spinSpeed = 140f;

    bool collected;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        GameManager.Instance?.AddCollectible(points);
        gameObject.SetActive(false);
    }

    public void ResetCollectible()
    {
        collected = false;
        gameObject.SetActive(true);
    }
}
