using UnityEngine;
using UnityEngine.UI;

public enum StaminaDrainMode
{
    Constant, Instant
}

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100.0f;
    [SerializeField] private float _maxStance = 100.0f;

    [Header("Stamina Settings")]
    [SerializeField] private float _maxStamina = 100.0f;
    [SerializeField] private float _staminaRegenValue = 5.0f;
    [SerializeField] private float _normalRegenSpeed = 1.0f;
    [SerializeField] private float _staminaTimeToRegen = 1.0f;

    //TODO: Implement component to handle UI.
    [Header("UI Settings")]
    [SerializeField] private Slider _staminaBar;

    private float _currentHealth;
    private float _currentStamina;
    private float _currentStance;
    private float _staminaRegenTimer = 0f;

    public float CurrentHealth { get => _currentHealth; private set => _currentHealth = value; }
    public float CurrentStamina { get => _currentStamina; private set => _currentStamina = value; }
    public float CurrentStance { get => _currentStance; private set => _currentStance = value; }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;
        _currentStance = _maxStance;

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
        _staminaBar.value = _currentStamina;
    }

    private void RegenerateStamina()
    {
        if (_currentStamina > _maxStamina - 0.01f) return;

        _currentStamina += _staminaRegenValue * _normalRegenSpeed * Time.deltaTime;
    }

    private bool CanRegenerateStamina()
    {
        _staminaRegenTimer += Time.deltaTime;

        return _staminaRegenTimer >= _staminaTimeToRegen;
    }

    public void DrainStamina(float value, StaminaDrainMode drainMode)
    {
        if (_currentStamina <= 0f) return;

        _staminaRegenTimer = 0f;

        switch (drainMode)
        {
            case StaminaDrainMode.Constant:
                _currentStamina -= value * Time.deltaTime;
                break;
            case StaminaDrainMode.Instant:
                _currentStamina -= value;
                break;
            default:
                break;
        }
    }
}
