using UnityEngine;

public class ButterflyController : MonoBehaviour
{
    public float orbitRadius  = 2.0f;
    public float orbitSpeed   = 0.6f;
    public float bobHeight    = 0.3f;
    public float bobSpeed     = 1.2f;
    public float wingFlapSpeed = 8.0f;
    public float wanderRadius  = 6.0f;
    public float wanderInterval = 4.0f;

    private Vector3   _center;
    private Vector3   _wanderTarget;
    private float     _orbitAngle;
    private float     _wanderTimer;
    private float     _timeOffset;
    private Transform _wingL;
    private Transform _wingR;

    void Start()
    {
        _center       = transform.position;
        _wanderTarget = _center;
        _timeOffset   = Random.Range(0f, Mathf.PI * 2f);
        _orbitAngle   = Random.Range(0f, Mathf.PI * 2f);
        orbitRadius   = Random.Range(1.2f, 3.0f);
        orbitSpeed    = Random.Range(0.4f, 0.9f);
        bobHeight     = Random.Range(0.15f, 0.5f);

        _wingL = transform.Find("WingL");
        _wingR = transform.Find("WingR");
    }

    void Update()
    {
        float t = Time.time + _timeOffset;

        _orbitAngle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(_orbitAngle) * orbitRadius;
        float z = Mathf.Sin(_orbitAngle) * orbitRadius;
        float y = Mathf.Sin(t * bobSpeed) * bobHeight;

        Vector3 targetPos = _center + new Vector3(x, y, z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 4f);

        Vector3 dir = targetPos - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        if (_wingL != null)
            _wingL.localRotation = Quaternion.Euler(0, Mathf.Sin(t * wingFlapSpeed) * 60f, 0);
        if (_wingR != null)
            _wingR.localRotation = Quaternion.Euler(0, -Mathf.Sin(t * wingFlapSpeed) * 60f, 0);

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            _wanderTimer = wanderInterval + Random.Range(-1f, 1f);
            Vector2 rand2D = Random.insideUnitCircle * wanderRadius;
            _wanderTarget = _center + new Vector3(rand2D.x, 0, rand2D.y);
        }
        _center = Vector3.MoveTowards(_center, _wanderTarget, Time.deltaTime * 0.5f);
    }
}
