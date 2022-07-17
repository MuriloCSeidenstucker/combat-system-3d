using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _maxGroundSpeed = 10.0f;
    [SerializeField] private float _movementAcc = 100.0f;
    [SerializeField] private float _rotationAcc = 10.0f;

    [Header("Jump Settings")]
    [SerializeField] float maxJumpHeight = 4.0f;
    [SerializeField] float jumpPeakTime = 0.4f;

    float Gravity { get { return maxJumpHeight * 2 / (jumpPeakTime * jumpPeakTime); } }
    public float JumpSpeed { get { return Gravity * jumpPeakTime; } }

    [Header("Ground Collision Settings")]
    [SerializeField] LayerMask _groundedLayerMask = default;
    [SerializeField] float _groundedRaycastDistance = 0.1f;
    [SerializeField] float _obstacleRaycastDistance = 0.1f;

    private Rigidbody _rigidbody;
    private Vector3 _currentVelocity = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;

    private bool _isGrounded;
    private int _raycastCount = 5;
    private List<RaycastHit> _groundHits;
    private Vector3[] _raycastPositions;

    public IColliderInfo ColliderInfo { get; private set; }
    public bool IsGrounded { get { return _isGrounded; } }
    public bool IsJumping { get { return _currentVelocity.y > 0; } }

    private void Awake()
    {
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
        _currentVelocity.y -= Gravity * Time.fixedDeltaTime;
    }

    public void Jump()
    {
        if (CanJump())
        {
            _currentVelocity.y = JumpSpeed;
        }
    }

    protected bool CanJump()
    {
        return IsGrounded && !IsJumping;
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
        if (Physics.SphereCast(transform.position + Vector3.up * ColliderInfo.Height * 0.5f, ColliderInfo.Radius, _currentVelocity.normalized, out var hit, _obstacleRaycastDistance))
        {
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_currentVelocity, hit.normal);
            _currentVelocity = projectedVelocity.normalized * _currentVelocity.magnitude;
        }
    }

    private void CheckGroundCollision()
    {
        _raycastPositions[0] = GetColliderBottom();
        _raycastPositions[1] = GetColliderBottom() + Vector3.right * ColliderInfo.Radius * 0.5f;
        _raycastPositions[2] = GetColliderBottom() + Vector3.left * ColliderInfo.Radius * 0.5f;
        _raycastPositions[3] = GetColliderBottom() + Vector3.forward * ColliderInfo.Radius * 0.5f;
        _raycastPositions[4] = GetColliderBottom() + Vector3.back * ColliderInfo.Radius * 0.5f;

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
            for (int i = 0; i < hitCounts; i++)
            {
                if (_groundHits[i].point.y > surfacePosition.y)
                {
                    surfacePosition.y = _groundHits[i].point.y;
                }
            }
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

        Vector3 desiredVelocity = velocityInput.normalized * _maxGroundSpeed;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, desiredVelocity, _movementAcc * Time.deltaTime);
    }

    public void SetRotation(Vector3 rotationInput)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(rotationInput);
        _currentRotation = Quaternion.Slerp(_currentRotation, desiredRotation, _rotationAcc * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Vector3 pos = (transform.position + Vector3.up * ColliderInfo.Height * 0.5f) + _currentVelocity.normalized * _obstacleRaycastDistance;
            Gizmos.DrawWireSphere(pos, ColliderInfo.Radius);
        }
    }
}
