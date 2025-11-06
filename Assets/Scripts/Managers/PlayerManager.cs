using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player arm prefab")]
    [SerializeField] private PlayerArm _arm;

    public PlayerArm Arm => _arm;

    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable() => CursorManager.Instance.SetGameplayCursor();

    private void OnDisable() => CursorManager.Instance.SetUICursor();

    public void CustomUpdate() => _arm.CustomUpdate();
}