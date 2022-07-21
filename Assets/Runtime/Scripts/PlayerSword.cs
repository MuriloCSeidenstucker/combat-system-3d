using UnityEngine;

public class PlayerSword : MeleeAttack
{
    [SerializeField] private float _swordDamage = 20.0f;

    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    protected override Quaternion GetRotation() => _player.CurrentRotation;

    protected override float GetDamage() => _swordDamage;

    public void Attack() => TriggerDamage();
}
