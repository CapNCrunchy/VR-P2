using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Relaxed first-person controller — uses Unity's NEW Input System.
/// Fixes the InvalidOperationException errors.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RelaxedPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float mouseSensitivity = 0.15f;
    public float gravity = -9.81f;

    [Header("Head Bob (subtle)")]
    public float bobFrequency = 1.8f;
    public float bobAmplitude = 0.04f;

    private CharacterController _cc;
    private Camera _cam;
    private float _xRotation = 0f;
    private Vector3 _velocity;
    private float _bobTimer = 0f;
    private float _camBaseY;

    private Mouse _mouse;
    private Keyboard _keyboard;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;

        _cam = GetComponentInChildren<Camera>();
        if (_cam == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform);
            camObj.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            _cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        _camBaseY = _cam.transform.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_mouse == null) _mouse = Mouse.current;
        if (_keyboard == null) _keyboard = Keyboard.current;

        HandleLook();
        HandleMove();
        HandleBob();

        if (_keyboard != null && _keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (_mouse != null && _mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleLook()
    {
        if (_mouse == null || Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = _mouse.delta.ReadValue();
        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        _cam.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMove()
    {
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float h = 0f, v = 0f;
        if (_keyboard != null)
        {
            if (_keyboard.aKey.isPressed || _keyboard.leftArrowKey.isPressed)  h -= 1f;
            if (_keyboard.dKey.isPressed || _keyboard.rightArrowKey.isPressed) h += 1f;
            if (_keyboard.sKey.isPressed || _keyboard.downArrowKey.isPressed)  v -= 1f;
            if (_keyboard.wKey.isPressed || _keyboard.upArrowKey.isPressed)    v += 1f;
        }

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();
        _cc.Move(move * walkSpeed * Time.deltaTime);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    void HandleBob()
    {
        if (_keyboard == null) return;

        bool moving = _keyboard.wKey.isPressed || _keyboard.aKey.isPressed ||
                      _keyboard.sKey.isPressed || _keyboard.dKey.isPressed ||
                      _keyboard.upArrowKey.isPressed || _keyboard.downArrowKey.isPressed ||
                      _keyboard.leftArrowKey.isPressed || _keyboard.rightArrowKey.isPressed;

        if (moving && _cc.isGrounded)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            float bobY = Mathf.Sin(_bobTimer * Mathf.PI * 2f) * bobAmplitude;
            Vector3 localPos = _cam.transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, _camBaseY + bobY, Time.deltaTime * 10f);
            _cam.transform.localPosition = localPos;
        }
        else
        {
            _bobTimer = 0f;
            Vector3 localPos = _cam.transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, _camBaseY, Time.deltaTime * 5f);
            _cam.transform.localPosition = localPos;
        }
    }
}