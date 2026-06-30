using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrickZone : MonoBehaviour
{
    [SerializeField] int trickPoints = 250;
    [SerializeField] string trickName = "Clean trick line";

    bool scored;

    public void Configure(string newName, int points)
    {
        trickName = newName;
        trickPoints = points;
    }

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (scored || !other.CompareTag("Player"))
        {
            return;
        }

        Driver driver = other.GetComponentInParent<Driver>();
        if (driver == null || !driver.IsAirborne)
        {
            return;
        }

        scored = true;
        GameManager.Instance?.CompleteTrick(trickName, trickPoints);
    }

    public void ResetZone()
    {
        scored = false;
    }
}
