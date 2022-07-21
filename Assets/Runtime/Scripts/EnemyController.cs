using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public void TakeDamage(float damage)
    {
        Debug.Log($"Enemy takes {damage} damage!");
    }
}
