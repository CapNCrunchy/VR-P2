using UnityEngine;

public class TreeShake : MonoBehaviour
{
    public float shakeDuration  = 1.2f;
    public float shakeIntensity = 12f;
    public float shakeSpeed     = 8f;
    public float cooldown       = 2f;

    private float     _shakeTimer;
    private float     _cooldownTimer;
    private Quaternion _baseRotation;
    private bool      _shaking;

    void Start()
    {
        _baseRotation = transform.localRotation;
        AddTriggerCollider();
    }

    void AddTriggerCollider()
    {
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius    = 2.2f;
        sc.center    = new Vector3(0f, 2f, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand") && !other.CompareTag("MainCamera")) return;
        TriggerShake();
    }

    public void TriggerShake()
    {
        if (_cooldownTimer > 0f) return;
        _shaking       = true;
        _shakeTimer    = shakeDuration;
        _cooldownTimer = cooldown;
    }

    void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

        if (_shaking)
        {
            _shakeTimer -= Time.deltaTime;
            float t       = 1f - (_shakeTimer / shakeDuration);
            float fade    = Mathf.Sin(t * Mathf.PI);           // ramp up then down
            float sway    = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity * fade;
            transform.localRotation = _baseRotation * Quaternion.Euler(sway, 0f, sway * 0.4f);

            if (_shakeTimer <= 0f)
            {
                _shaking = false;
                transform.localRotation = _baseRotation;
            }
        }
    }
}
