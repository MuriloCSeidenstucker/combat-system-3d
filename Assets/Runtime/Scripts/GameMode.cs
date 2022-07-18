using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMode : MonoBehaviour
{
    [Header("Development Settings")]
    [SerializeField] private DevelopmentSettings _settings;

    private void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.isPaused = _settings.StartGamePaused;
#endif
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
