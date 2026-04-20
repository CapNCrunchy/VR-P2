using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed  = 1.5f;
    public float spinSpeed = 90f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
        AddTriggerCollider();
    }

    void AddTriggerCollider()
    {
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius    = 0.4f;
    }

    void Update()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = _startPos + Vector3.up * bob;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand") && !other.CompareTag("MainCamera")) return;

        CollectibleManager.Instance?.OnCollect();
        gameObject.SetActive(false);
    }
}
