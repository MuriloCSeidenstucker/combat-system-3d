using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float _topClamp = 70.0f;
    [SerializeField] private float _bottomClamp = -30.0f;
    [SerializeField] private float _cameraAngleOverride = 0.0f;

    [Header("Camera Sensitivity")]
    [Range(0.1f, 5.0f)]
    [Min(0.1f)][SerializeField] private float _horizontalSensitivity = 1.0f;
    [Range(0.1f, 5.0f)]
    [Min(0.1f)][SerializeField] private float _verticalSensitivity = 1.0f;

    [Header("Development Settings")]
    [SerializeField] private DevelopmentSettings _settings;

    private PlayerController _player;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private const float c_threshold = 0.01f;

    private void Start()
    {
        _player = GetComponentInParent<PlayerController>();
        _cinemachineTargetYaw = transform.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        Vector2 lookInput = _player.GetLookInput();

        if (lookInput.sqrMagnitude >= c_threshold && !_settings.LockCameraPosition)
        {
            float deltaTimeMultiplier = _player.IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier * _horizontalSensitivity;
            _cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier * _verticalSensitivity;
        }

        _cinemachineTargetYaw = MathUtils.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = MathUtils.ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
}
