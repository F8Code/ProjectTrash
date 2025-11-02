using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerArm _arm;
    
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() //Remove this function when state machine is implemented
    {
        CustomUpdate();
    }

    void CustomUpdate()
    {
        _arm.CustomUpdate();
    }
    
    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
