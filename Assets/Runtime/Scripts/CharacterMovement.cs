using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 10.0f;
    [SerializeField] private float _movementAcc = 100.0f;
    [SerializeField] private float _rotationAcc = 10.0f;

    [Header("Jump Settings")]
    [SerializeField] private float _maxJumpHeight = 4.0f;
    [SerializeField] private float _jumpPeakTime = 0.4f;
    [SerializeField] private float _jumpStaminaDrainValue = 10.0f;

    [Header("Sprint Settings")]
    [SerializeField] private float _sprintSpeed = 15.0f;
    [SerializeField] private float _sprintStaminaDrainValue = 2.0f;

    [Header("Ground Collision Settings")]
    [SerializeField] LayerMask _groundedLayerMask = default;
    [SerializeField] float _groundedRaycastDistance = 0.1f;

    [Header("Obstacle Collision Settings")]
    [SerializeField] LayerMask _obstacleLayerMask = default;
    [SerializeField] float _obstacleRaycastDistance = 0.1f;

    [Header("Development Settings")]
    [SerializeField] private DevelopmentSettings _settings;

    private CharacterStats _stats;
    private Rigidbody _rigidbody;
    private Vector3 _currentVelocity = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;
    private bool _isGrounded;
    private int _raycastCount = 5;
    private List<RaycastHit> _groundHits;
    private Vector3[] _raycastPositions;
    private Vector3 _lastFrameVelocity = Vector3.zero;
    private bool _isSprinting;

    public IColliderInfo ColliderInfo { get; private set; }
    public float Gravity { get { return _maxJumpHeight * 2 / (_jumpPeakTime * _jumpPeakTime); } }
    public float JumpSpeed { get { return Gravity * _jumpPeakTime; } }
    public bool IsSprinting { get { return _isSprinting; } }
    public bool IsGrounded { get { return _isGrounded; } }
    public bool IsJumping { get { return _currentVelocity.y > 0; } }

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        _rigidbody = GetComponent<Rigidbody>();

        Collider thisCollider = GetComponent<Collider>();
        ColliderInfo = ColliderInfoFactory.NewColliderInfo(thisCollider);

        _raycastPositions = new Vector3[_raycastCount];
        _groundHits = new List<RaycastHit>(_raycastCount * 2);
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        CheckGroundCollision();
        RotateCharacter();
        CheckObstacleCollision();
        MoveCharacter();
    }

    void ApplyGravity()
    {
        if (_isGrounded) return;

        _currentVelocity.y -= Gravity * Time.fixedDeltaTime;
    }

    private bool CanJump()
    {
        return IsGrounded && !IsJumping && _stats.CurrentStamina > _jumpStaminaDrainValue;
    }

    private bool CanSprint()
    {
        return IsGrounded && !IsJumping && _stats.CurrentStamina > 0;
    }

    private void MoveCharacter()
    {
        if (_currentVelocity == Vector3.zero) return;

        Vector3 previousPosition = _rigidbody.position;
        Vector3 currentPosition = previousPosition + _currentVelocity * Time.fixedDeltaTime;
        _rigidbody.MovePosition(currentPosition);
    }

    private void RotateCharacter()
    {
        if (_currentRotation == Quaternion.identity) return;

        _rigidbody.MoveRotation(_currentRotation.normalized);
    }

    private void CheckObstacleCollision()
    {
        if (Physics.SphereCast(transform.position + Vector3.up * ColliderInfo.Height * 0.5f, ColliderInfo.Radius, _currentVelocity.normalized, out var hit, _obstacleRaycastDistance, _obstacleLayerMask))
        {
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_currentVelocity, hit.normal);
            _currentVelocity = projectedVelocity.normalized * _currentVelocity.magnitude;
        }
    }

    private void CheckGroundCollision()
    {
        if (_currentVelocity.x != 0f || _currentVelocity.z != 0f)
        {
            _lastFrameVelocity = new Vector3(_currentVelocity.x, 0f, _currentVelocity.z).normalized;
        }

        Vector3 right = Vector3.Cross(Vector3.up, _lastFrameVelocity);

        _raycastPositions[0] = GetColliderBottom();
        _raycastPositions[1] = GetColliderBottom() + right.normalized * ColliderInfo.Radius * 0.5f;
        _raycastPositions[2] = GetColliderBottom() + -right.normalized * ColliderInfo.Radius * 0.5f;
        _raycastPositions[3] = GetColliderBottom() + _lastFrameVelocity * ColliderInfo.Radius * 0.5f;
        _raycastPositions[4] = GetColliderBottom() + -_lastFrameVelocity * ColliderInfo.Radius * 0.5f;

        float raycastDistance = (_rigidbody.position - GetColliderBottom()).sqrMagnitude + (_groundedRaycastDistance * _groundedRaycastDistance);

        int hitCounts = 0;
        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            Debug.DrawRay(_raycastPositions[i], Vector3.down * raycastDistance);
            if (Physics.Raycast(_raycastPositions[i], Vector3.down, out var hit, raycastDistance, _groundedLayerMask))
            {
                _groundHits.Add(hit);
                hitCounts++;
            }
        }

        Vector3 surfacePosition = _rigidbody.position;
        if (hitCounts > 0)
        {
            surfacePosition.y = _groundHits[0].point.y;
        }

        _isGrounded = hitCounts > 0;

        if (_isGrounded && !IsJumping)
        {
            _currentVelocity.y = 0f;
            _rigidbody.position = surfacePosition;
        }

        _groundHits.Clear();
    }

    private Vector3 GetColliderBottom()
    {
        return _rigidbody.position + ColliderInfo.Center + Vector3.down * (ColliderInfo.Height * 0.5f - ColliderInfo.Radius * 0.5f);
    }

    public void SetVelocity(Vector3 velocityInput)
    {
        if (!IsGrounded) return;

        float targetSpeed = IsSprinting ? _sprintSpeed : _movementSpeed;
        Vector3 desiredVelocity = velocityInput.normalized * targetSpeed;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, desiredVelocity, _movementAcc * Time.deltaTime);
    }

    public void SetRotation(Vector3 rotationInput)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(rotationInput);
        _currentRotation = Quaternion.Slerp(_currentRotation, desiredRotation, _rotationAcc * Time.deltaTime);
    }

    public void Jump()
    {
        if (CanJump())
        {
            _stats.DrainStamina(_jumpStaminaDrainValue, DrainMode.Instant);
            _currentVelocity.y = JumpSpeed;
        }
    }

    public void Sprint()
    {
        if (CanSprint())
        {
            _stats.DrainStamina(_sprintStaminaDrainValue, DrainMode.Constant);
            _isSprinting = true;
        }
        else
        {
            StopSprint();
        }
    }

    public void StopSprint() => _isSprinting = false;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _settings.EnableGizmos)
        {
            Gizmos.color = Color.magenta;
            Vector3 pos = (transform.position + Vector3.up * ColliderInfo.Height * 0.5f) + _currentVelocity.normalized * _obstacleRaycastDistance;
            Gizmos.DrawWireSphere(pos, ColliderInfo.Radius);
        }
    }
}
