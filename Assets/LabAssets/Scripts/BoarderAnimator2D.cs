using UnityEngine;

public class BoarderAnimator2D : MonoBehaviour
{
    [SerializeField] float leanAmount = 8f;
    [SerializeField] float squashAmount = 0.08f;
    [SerializeField] float animationSpeed = 8f;

    Driver driver;
    Transform[] visualParts;
    Vector3[] baseScales;
    Quaternion[] baseRotations;

    void Awake()
    {
        driver = GetComponent<Driver>();
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        visualParts = new Transform[renderers.Length];
        baseScales = new Vector3[renderers.Length];
        baseRotations = new Quaternion[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            visualParts[i] = renderers[i].transform;
            baseScales[i] = visualParts[i].localScale;
            baseRotations[i] = visualParts[i].localRotation;
        }
    }

    void Update()
    {
        if (driver == null || visualParts.Length == 0)
        {
            return;
        }

        float horizontal = SnowboardInput.Horizontal();
        float speedRatio = Mathf.InverseLerp(0f, 40f, driver.Speed);
        float targetLean = -horizontal * leanAmount;
        float squash = driver.IsGrounded ? Mathf.Lerp(0f, squashAmount, speedRatio) : -squashAmount;

        for (int i = 0; i < visualParts.Length; i++)
        {
            Quaternion targetRotation = baseRotations[i] * Quaternion.Euler(0f, 0f, targetLean);
            visualParts[i].localRotation = Quaternion.Slerp(visualParts[i].localRotation, targetRotation, animationSpeed * Time.deltaTime);

            Vector3 baseScale = baseScales[i];
            Vector3 targetScale = new Vector3(baseScale.x + squash, baseScale.y - squash, baseScale.z);
            visualParts[i].localScale = Vector3.Lerp(visualParts[i].localScale, targetScale, animationSpeed * Time.deltaTime);
        }
    }
}
