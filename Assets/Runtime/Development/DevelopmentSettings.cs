using UnityEngine;

[CreateAssetMenu(fileName = "DevelopmentSettings", menuName = "Data/DevelopmentSettings")]
public class DevelopmentSettings : ScriptableObject
{
    #region GameMode
    [field: Header("Player Settings")]

    [field: SerializeField]
    public bool StartGamePaused { get; private set; } = false;
    #endregion


    #region Player
    [field: Header("Player Settings")]

    [field: SerializeField]
    public bool EnableGizmos { get; private set; } = false;

    [field: SerializeField]
    public bool AutoMove { get; private set; } = false;
    #endregion

    #region Camera
    [field: Header("Camera Settings")]

    [field: SerializeField]
    public bool LockCameraPosition { get; private set; } = false;
    #endregion
}
