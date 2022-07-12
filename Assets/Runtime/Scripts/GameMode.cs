using UnityEngine;
using UnityEngine.InputSystem;

public class GameMode : MonoBehaviour
{
    private void Awake()
    {
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
