using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to Main Camera. WASD to fly, mouse to look around (hold right-click).
/// </summary>
public class CameraFly : MonoBehaviour
{
    public float moveSpeed  = 10f;
    public float lookSpeed  = 0.15f;
    public float sprintMult = 3f;

    private float _yaw;
    private float _pitch;

    void Start()
    {
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.rightButton.isPressed) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        _yaw   += delta.x * lookSpeed;
        _pitch -= delta.y * lookSpeed;
        _pitch  = Mathf.Clamp(_pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    void HandleMove()
    {
        if (Keyboard.current == null) return;

        float speed = moveSpeed;
        if (Keyboard.current.leftShiftKey.isPressed) speed *= sprintMult;

        Vector3 dir = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) dir += transform.forward;
        if (Keyboard.current.sKey.isPressed) dir -= transform.forward;
        if (Keyboard.current.aKey.isPressed) dir -= transform.right;
        if (Keyboard.current.dKey.isPressed) dir += transform.right;
        if (Keyboard.current.eKey.isPressed) dir += Vector3.up;
        if (Keyboard.current.qKey.isPressed) dir -= Vector3.up;

        transform.position += dir * speed * Time.deltaTime;
    }
}
