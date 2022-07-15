using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _maxGroundSpeed = 10.0f;
    [SerializeField] private float _movementAcc = 100.0f;
    [SerializeField] private float _rotationAcc = 10.0f;

    [Header("Ground Collision Settings")]
    [SerializeField] LayerMask _groundedLayerMask = default;
    [SerializeField] float _groundedRaycastDistance = 0.1f;

    private Rigidbody _rigidbody;
    private Vector3 _currentVelocity = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;

    private bool _isGrounded;
    private bool _wasGroundedLastFrame;
    private int _raycastCount = 5;
    private Vector3[] _raycastPositions;
    private RaycastHit[] _groundHits;

    public IColliderInfo ColliderInfo { get; private set; }
    public bool IsGrounded { get { return _isGrounded == _wasGroundedLastFrame && _isGrounded; } }
    public bool IsJumping { get { return _currentVelocity.y > 0; } }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        Collider thisCollider = GetComponent<Collider>();
        ColliderInfo = ColliderInfoFactory.NewColliderInfo(thisCollider);

        _raycastPositions = new Vector3[_raycastCount];
        _groundHits = new RaycastHit[_raycastCount];
    }

    private void FixedUpdate()
    {
        RotateCharacter();
        MoveCharacter();

        CheckGroundCollision();
    }

    private void MoveCharacter()
    {
        if (_currentVelocity == Vector3.zero) return;

        Vector3 previousPosition = _rigidbody.position;
        Vector3 currentPosition = previousPosition + _currentVelocity * Time.fixedDeltaTime;
        _rigidbody.MovePosition(currentPosition);

        if (previousPosition.y != _rigidbody.position.y)
            Assert.AreApproximatelyEqual(previousPosition.y, _rigidbody.position.y);
    }

    private void RotateCharacter()
    {
        if (_currentRotation == Quaternion.identity) return;

        _rigidbody.MoveRotation(_currentRotation.normalized);
    }

    private void CheckGroundCollision()
    {
        _raycastPositions[0] = GetColliderBottom();
        _raycastPositions[1] = GetColliderBottom() + Vector3.left * ColliderInfo.Radius * 0.5f;
        _raycastPositions[2] = GetColliderBottom() + Vector3.right * ColliderInfo.Radius * 0.5f;
        _raycastPositions[3] = GetColliderBottom() + Vector3.forward * ColliderInfo.Radius * 0.5f;
        _raycastPositions[4] = GetColliderBottom() + Vector3.back * ColliderInfo.Radius * 0.5f;

        float raycastDistance = ColliderInfo.Radius * 0.5f + _groundedRaycastDistance;
        Vector3 raycastDirection = Vector3.down;

        _wasGroundedLastFrame = _isGrounded;
        _isGrounded = false;

        int hitCount = 0;
        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            Debug.DrawLine(_raycastPositions[i], _raycastPositions[i] + raycastDirection * raycastDistance);
            if (Physics.RaycastNonAlloc(_raycastPositions[i], raycastDirection, _groundHits, raycastDistance, _groundedLayerMask, QueryTriggerInteraction.Ignore) > 0)
            {
                ++hitCount;
            }
        }

        _isGrounded = _currentVelocity.magnitude > _maxGroundSpeed ? hitCount == _raycastCount : hitCount > 0;

        if (_isGrounded && !IsJumping)
        {
            _currentVelocity.y = 0;
        }
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
}
