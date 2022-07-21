using UnityEngine;
using UnityEngine.UI;

public enum DrainMode
{
    Constant, Instant
}

public class CharacterStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100.0f;

    [Header("Stamina Settings")]
    [SerializeField] private float _maxStamina = 100.0f;
    [SerializeField] private float _staminaRegenValue = 5.0f;
    [SerializeField] private float _normalRegenSpeed = 1.0f;
    [SerializeField] private float _staminaTimeToRegen = 1.0f;

    //TODO: Implement component to handle UI.
    [Header("UI Settings")]
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Slider _staminaBar;

    private float _currentHealth;
    private float _currentStamina;
    private float _staminaRegenTimer;

    public float CurrentHealth { get => _currentHealth; private set => _currentHealth = value; }
    public float CurrentStamina { get => _currentStamina; private set => _currentStamina = value; }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;

        _healthBar.maxValue = _maxHealth;
        _staminaBar.maxValue = _maxStamina;
    }

    private void Update()
    {
        if (CanRegenerateStamina())
        {
            RegenerateStamina();
        }
    }

    private void LateUpdate()
    {
        _healthBar.value = _currentHealth;
        _staminaBar.value = _currentStamina;
    }

    private void RegenerateStamina()
    {
        if (_currentStamina > _maxStamina - 0.01f) return;

        _currentStamina += _staminaRegenValue * _normalRegenSpeed * Time.deltaTime;
    }

    private bool CanRegenerateStamina()
    {
        return Time.time >= _staminaRegenTimer;
    }

    public void RegenerateHealth(float value)
    {
        var currentHealth = _currentHealth;
        currentHealth += value;
        _currentHealth = currentHealth > _maxHealth ? _maxHealth : currentHealth;
    }

    public void DrainHealth(float value, DrainMode drainMode)
    {
        if (_currentHealth <= 0f) return;

        switch (drainMode)
        {
            case DrainMode.Constant:
                _currentHealth -= value * Time.deltaTime;
                break;
            case DrainMode.Instant:
                _currentHealth -= value;
                break;
            default:
                break;
        }
    }

    public void DrainStamina(float value, DrainMode drainMode)
    {
        if (_currentStamina <= 0f) return;

        _staminaRegenTimer = Time.time + _staminaTimeToRegen;

        switch (drainMode)
        {
            case DrainMode.Constant:
                _currentStamina -= value * Time.deltaTime;
                break;
            case DrainMode.Instant:
                _currentStamina -= value;
                break;
            default:
                break;
        }
    }
}
