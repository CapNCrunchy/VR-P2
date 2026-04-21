using UnityEngine;

public class SwingAnimator : MonoBehaviour
{
    public float swingAngle = 30f;
    public float swingSpeed = 0.8f;
    public float timeOffset = 0f;

    void Update()
    {
        float angle = Mathf.Sin((Time.time + timeOffset) * swingSpeed * Mathf.PI * 2f) * swingAngle;
        transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}

public class SeesawAnimator : MonoBehaviour
{
    public float tiltAngle = 18f;
    public float speed     = 0.5f;

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed * Mathf.PI * 2f) * tiltAngle;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}

public class WindSway : MonoBehaviour
{
    public float swayAmount    = 4f;
    public float swaySpeed     = 1.2f;
    public float swaySmoothing = 3f;

    private Quaternion _base;
    private float      _offset;

    void Start()
    {
        _base   = transform.localRotation;
        _offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float sway = Mathf.Sin((Time.time + _offset) * swaySpeed * Mathf.PI * 2f) * swayAmount;
        Quaternion target = _base * Quaternion.Euler(sway, 0f, sway * 0.3f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, target,
                                                   Time.deltaTime * swaySmoothing);
    }
}
