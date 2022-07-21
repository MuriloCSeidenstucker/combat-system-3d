using UnityEngine;

public abstract class MeleeAttack : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector3 _boxDamageArea = Vector3.one;
    [SerializeField] private float _boxDamageAreaDistance = 2.0f;
    [SerializeField] private float _boxDamageAreaHeigth = 1.0f;

    private Collider[] _collidersResults = new Collider[1];

    private Quaternion BoxRotation => GetRotation();
    private Vector3 BoxCenterPosition => (transform.position + Vector3.up * _boxDamageAreaHeigth) + (BoxRotation * Vector3.forward) * _boxDamageAreaDistance;

    protected void TriggerDamage()
    {
        if (Physics.OverlapBoxNonAlloc(BoxCenterPosition, _boxDamageArea * 0.5f, _collidersResults, BoxRotation, _layerMask, QueryTriggerInteraction.Ignore) > 0)
        {
            for (int i = 0; i < _collidersResults.Length; i++)
            {
                if (_collidersResults[i].TryGetComponent<IDamageable>(out var target))
                {
                    target.TakeDamage(GetDamage());
                }
            }
        }
    }

    protected abstract Quaternion GetRotation();
    protected abstract float GetDamage();


    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.matrix = Matrix4x4.TRS(BoxCenterPosition, BoxRotation, _boxDamageArea);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
