using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 10.0f;
    [SerializeField] private float _movementAcc = 100.0f;
    [SerializeField] private float _rotationAcc = 10.0f;

    [Header("Ground Collision Settings")]
    [SerializeField] LayerMask groundedLayerMask = default;
    [SerializeField] float groundedRaycastDistance = 0.1f;

    private Rigidbody _rigidbody;
    private Vector3 _currentVelocity = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;

    bool isGrounded;
    bool wasGroundedLastFrame;

    public IColliderInfo ColliderInfo { get; private set; }
    public bool IsGrounded { get { return isGrounded == wasGroundedLastFrame && isGrounded; } }
    public bool IsJumping { get { return _currentVelocity.y > 0; } }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        Collider thisCollider = GetComponent<Collider>();
        ColliderInfo = ColliderInfoFactory.NewColliderInfo(thisCollider);
    }

    private void FixedUpdate()
    {
        RotateCharacter();
        MoveCharacter();

        CheckCapsuleCollisionsBottom();
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

    private void CheckCapsuleCollisionsBottom()
    {
        int raycastCount = 5;
        Vector3[] raycastPositions = new Vector3[raycastCount];

        raycastPositions[0] = GetColliderBottom();
        raycastPositions[1] = GetColliderBottom() + Vector3.left * ColliderInfo.Radius * 0.5f;
        raycastPositions[2] = GetColliderBottom() + Vector3.right * ColliderInfo.Radius * 0.5f;
        raycastPositions[3] = GetColliderBottom() + Vector3.forward * ColliderInfo.Radius * 0.5f;
        raycastPositions[4] = GetColliderBottom() + Vector3.back * ColliderInfo.Radius * 0.5f;

        RaycastHit[] hitBuffer = new RaycastHit[5];
        float raycastDistance = ColliderInfo.Radius * 0.5f + groundedRaycastDistance * 2f;
        Vector3 raycastDirection = Vector3.down;

        wasGroundedLastFrame = isGrounded;
        isGrounded = false;

        int hitCount = 0;
        for (int i = 0; i < raycastPositions.Length; i++)
        {
            Debug.DrawLine(raycastPositions[i], raycastPositions[i] + raycastDirection * raycastDistance);
            if (Physics.RaycastNonAlloc(raycastPositions[i], raycastDirection, hitBuffer, raycastDistance, groundedLayerMask, QueryTriggerInteraction.Ignore) > 0)
            {
                ++hitCount;
            }
        }

        isGrounded = _currentVelocity.magnitude > 10.0f ? hitCount == 3 : hitCount > 0;

        if (isGrounded && !IsJumping)
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
        Vector3 desiredVelocity = velocityInput.normalized * _movementSpeed;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, desiredVelocity, _movementAcc * Time.deltaTime);
    }

    public void SetRotation(Vector3 rotationInput)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(rotationInput);
        _currentRotation = Quaternion.Slerp(_currentRotation, desiredRotation, _rotationAcc * Time.deltaTime);
    }
}
