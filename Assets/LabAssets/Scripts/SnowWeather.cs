using UnityEngine;

public class SnowWeather : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] Vector3 offset = new Vector3(0f, 8f, 0f);

    public void SetTarget(Transform target)
    {
        followTarget = target;
    }

    void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

        transform.position = followTarget.position + offset;
    }
}
