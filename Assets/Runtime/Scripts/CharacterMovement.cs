using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 10.0f;
    [SerializeField] private float _movementAcc = 100.0f;
    [SerializeField] private float _rotationAcc = 10.0f;

    [Header("Development Settings")]
    [SerializeField] private bool _enableGizmos = false;

    private Rigidbody _rigidbody;
    private Vector3 _currentVelocity = Vector3.zero;
    private Quaternion _currentRotation = Quaternion.identity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RotateCharacter();
        MoveCharacter();
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _enableGizmos)
        {
            Gizmos.color = Color.white;
            GizmosUtils.DrawVector(transform.position, _currentVelocity.normalized * 5, thickness: 1);
        }
    }
}
