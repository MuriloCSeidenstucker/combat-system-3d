using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _graphics;

    [Header("Gamepad Settings")]
    //TODO: Check how to implement by new input system.
    [SerializeField] private float _holdTime = 0.3f;

    [Header("Development Settings")]
    [SerializeField] private DevelopmentSettings _settings;

    private PlayerInput _playerInput;
    private PlayerInputAction _inputAction;
    private CharacterMovement _playerMovement;
    private CameraController _cameraController;
    private Quaternion _currentRotation;
    private float _sprintTimer;

    private const string c_keyboardMouse = "Keyboard&Mouse";

    public bool IsCurrentDeviceKeyboardMouse => _playerInput.currentControlScheme == c_keyboardMouse;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputAction = new PlayerInputAction();
        _playerMovement = GetComponent<CharacterMovement>();
        _cameraController = GetComponentInChildren<CameraController>();
    }

    private void OnEnable()
    {
        _inputAction.Player.Enable();
    }

    private void Update()
    {
        Vector3 movementInput = _settings.AutoMove ? _currentRotation * Vector3.forward : GetAndProcessMovementInput();

        if (GetSprintInput())
        {
            _playerMovement.Sprint();
        }

        if (_inputAction.Player.Sprint.WasReleasedThisFrame())
        {
            _playerMovement.StopSprint();
        }

        _playerMovement.SetVelocity(movementInput);

        if (_inputAction.Player.Jump.WasPressedThisFrame())
        {
            _playerMovement.Jump();
        }

        UpdateMeshRotation(movementInput);
    }

    private bool GetSprintInput()
    {
        if (IsCurrentDeviceKeyboardMouse) return _inputAction.Player.Sprint.IsPressed();

        if (_inputAction.Player.Sprint.IsPressed())
            _sprintTimer += Time.deltaTime;
        else
            _sprintTimer = 0f;

        if (_sprintTimer >= _holdTime) return true;

        return false;
    }

    private Vector3 GetAndProcessMovementInput()
    {
        Vector2 movementInputRaw = _inputAction.Player.Move.ReadValue<Vector2>();
        UpdatePlayerRotation();
        Vector3 movementInputProcessed = _currentRotation * new Vector3(movementInputRaw.x, 0f, movementInputRaw.y);

        return movementInputProcessed;
    }

    private void UpdatePlayerRotation()
    {
        Vector3 desiredForward = Vector3.ProjectOnPlane(_cameraController.transform.forward, Vector3.up);
        _currentRotation = Quaternion.LookRotation(desiredForward);
    }

    private void UpdateMeshRotation(in Vector3 movementInput)
    {
        if (movementInput != Vector3.zero)
        {
            _graphics.rotation = Quaternion.LookRotation(movementInput);
        }
    }

    public Vector2 GetLookInput() => _inputAction.Player.Look.ReadValue<Vector2>();

    private void OnDisable()
    {
        _inputAction.Player.Disable();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _settings.EnableGizmos)
        {
            Gizmos.color = Color.blue;
            GizmosUtils.DrawVector(_cameraController.transform.position, _cameraController.transform.forward, thickness: 1);
            Gizmos.color = Color.red;
            GizmosUtils.DrawVector(_cameraController.transform.position, _cameraController.transform.right, thickness: 1);

            Vector3 forwardProjected = Vector3.ProjectOnPlane(_cameraController.transform.forward, Vector3.up);
            Gizmos.color = Color.cyan;
            GizmosUtils.DrawVector(transform.position, forwardProjected, thickness: 1);

            Vector3 rightProjected = Vector3.ProjectOnPlane(_cameraController.transform.right, Vector3.up);
            Gizmos.color = Color.magenta;
            GizmosUtils.DrawVector(transform.position, rightProjected, thickness: 1);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(_cameraController.transform.position + _cameraController.transform.forward, transform.position + forwardProjected);
            Gizmos.DrawLine(_cameraController.transform.position + _cameraController.transform.right, transform.position + rightProjected);
        }
    }
}
