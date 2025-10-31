using System;
using UnityEngine;

public class PlayerArm : MonoBehaviour
{
    const float ARM_MOVEMENT_SCALING = 0.001f;
    const float ARM_ROTATION_SCALING = 0.5f;

    [Header("References")]
    [SerializeField] PlayerWrist _wrist;
    [SerializeField] PlayerCamera _camera;
    [SerializeField] Transform _elbowJoint;

    [Header("Arm movement settings")]
    [SerializeField, Range(0.1f, 2f)] float _armMovementStrength = 1f;
    [SerializeField, Range(0f, 2f)] float _cameraMovementStrength = 0.5f;
    [SerializeField] Vector2 _armXPositionRange = new Vector2(-0.4f, 0.4f);
    [SerializeField] Vector2 _armZPositionRange = new Vector2(-0.03f, 0.4f);
    [SerializeField] Vector2 _armSidewaysRotationRangeDegrees = new Vector2(-30f, 5f);

    [Header("Wrist rotation settings")]
    [SerializeField, Range(0f, 2f)] float _armRotationSpeed = 1f;
    [SerializeField] Vector2 _elbowRotationRangeDegrees = new Vector2(-60f, 180f);
    [SerializeField] Vector2 _wristRotationRangeDegrees = new Vector2(-60f, 60f);
    [SerializeField] bool _reverseElbowAxis = false;
    [SerializeField] bool _reverseWristAxis = false;

    Quaternion _baseElbowRotation;
    Quaternion _baseWristRotation;
    float _currentElbowZ = 0f, _currentWristX = 0f;

    void Start()
    {
        _baseElbowRotation = _elbowJoint.localRotation;
        _baseWristRotation = _wrist.transform.localRotation;
    }

    public void CustomUpdate()
    {
        if (InputManager.Instance.PlayerActions.RotateHand.ReadValue<float>() == 0f)
            MoveSelfAndCamera();
        else
            RotateHand();
            
        _wrist.CustomUpdate();
    }

    void MoveSelfAndCamera()
    {
        if (Time.time < 0.5f) return; //Prevent weird mouseInput readings at the very start of the game

        //Reading movement
        Vector2 mouseInput = InputManager.Instance.PlayerActions.MoveHand.ReadValue<Vector2>() * ARM_MOVEMENT_SCALING;
        Vector3 movementVector = new Vector3(mouseInput.x, 0, mouseInput.y);

        //Limiting movement
        Vector3 targetLocalPosition = transform.localPosition + movementVector * _armMovementStrength;
        if (targetLocalPosition.x < _armXPositionRange.x || targetLocalPosition.x > _armXPositionRange.y) movementVector.x = 0f;
        if (targetLocalPosition.z < _armZPositionRange.x || targetLocalPosition.z > _armZPositionRange.y) movementVector.z = 0f;

        //Movement
        transform.position += movementVector * _armMovementStrength;
        _camera.transform.position += movementVector * _cameraMovementStrength;

        //Y axis Rotation based on movement
        float inverseLerp = Mathf.InverseLerp(_armXPositionRange.x, _armXPositionRange.y, transform.localPosition.x);
        float rotationY = Mathf.Lerp(_armSidewaysRotationRangeDegrees.x, _armSidewaysRotationRangeDegrees.y, inverseLerp);
        _elbowJoint.localRotation = _baseElbowRotation * Quaternion.Euler(_elbowJoint.localEulerAngles.x, rotationY, _elbowJoint.localEulerAngles.z);
    }
    
    void RotateHand()
    {
        Vector2 mouseInput = InputManager.Instance.PlayerActions.MoveHand.ReadValue<Vector2>() * _armRotationSpeed * ARM_ROTATION_SCALING;

        //Optional axis reversion
        if (_reverseWristAxis) mouseInput.x *= -1f;
        if (_reverseElbowAxis) mouseInput.y *= -1f;

        //Limiting rotation
        _currentElbowZ = Mathf.Clamp(_currentElbowZ + mouseInput.x, _elbowRotationRangeDegrees.x, _elbowRotationRangeDegrees.y);
        _currentWristX = Mathf.Clamp(_currentWristX + mouseInput.y, _wristRotationRangeDegrees.x, _wristRotationRangeDegrees.y);

        _elbowJoint.localRotation = Quaternion.Euler(_elbowJoint.localEulerAngles.x, _elbowJoint.localEulerAngles.y, _currentElbowZ);
        _wrist.transform.localRotation = _baseWristRotation * Quaternion.Euler(_currentWristX, 0f, 0f);
    }
}
