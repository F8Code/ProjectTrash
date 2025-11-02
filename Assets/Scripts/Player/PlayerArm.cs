using System;
using System.Collections;
using UnityEngine;

public class PlayerArm : MonoBehaviour
{
    const float ARM_MOVEMENT_SCALING = 0.001f;
    const float ARM_ROTATION_SCALING = 0.5f;

    [Header("References")]
    [Tooltip("Reference to the wrist joint of the arm")]
    [SerializeField] PlayerWrist _wrist;
    [Tooltip("Reference to the player's camera")]
    [SerializeField] PlayerCamera _camera;
    [Tooltip("Transform to the elbow joint of the arm")]
    [SerializeField] Transform _elbowJoint;

    [Header("Arm movement settings")]
    [Tooltip("Strength of the arm movement relative to mouse input")]
    [SerializeField, Range(0.1f, 2f)] float _armMovementStrength = 1f;
    [Tooltip("Strength of the camera movement relative to the arm")]
    [SerializeField, Range(0f, 2f)] float _cameraMovementStrength = 0.5f;
    [Tooltip("Allowed X-axis movement range of the arm (local position)")]
    [SerializeField] Vector2 _armXPositionRange = new Vector2(-0.4f, 0.4f);
    [Tooltip("Allowed Z-axis movement range of the arm (local position)")]
    [SerializeField] Vector2 _armZPositionRange = new Vector2(-0.03f, 0.4f);
    [Tooltip("Sideways arm rotation range in degrees")]
    [SerializeField] Vector2 _armSidewaysRotationRangeDegrees = new Vector2(-30f, 5f);

    [Header("Wrist rotation settings")]
    [Tooltip("Speed multiplier for arm rotation when rotating the hand around")]
    [SerializeField, Range(0f, 2f)] float _armRotationSpeed = 1f;
    [Tooltip("Range of elbow rotation around the Z-axis in degrees")]
    [SerializeField] Vector2 _elbowRotationRangeDegrees = new Vector2(-60f, 180f);
    [Tooltip("Range of wrist rotation around the X-axis in degrees")]
    [SerializeField] Vector2 _wristRotationRangeDegrees = new Vector2(-60f, 60f);
    [Tooltip("Reverses the elbow rotation direction")]
    [SerializeField] bool _reverseElbowAxis = false;
    [Tooltip("Reverses the wrist rotation direction")]
    [SerializeField] bool _reverseWristAxis = false;

    [Header("Hand lifting settings")]
    [Tooltip("Lift angle of the arm in degrees when trash is grabbed")]
    [SerializeField] uint _armLiftDegrees = 30;
    [Tooltip("Speed at which the arm lifts or lowers in degrees per second")]
    [SerializeField] float _armDegreesPerSecond = 30f;

    Quaternion _baseElbowRotation;
    Quaternion _baseWristRotation;
    float _currentElbowZ, _currentWristX;
    float _currentLiftDegrees = 0f;
    bool _shouldLift = false;

    void Awake()
    {
        _baseElbowRotation = _elbowJoint.localRotation;
        _baseWristRotation = _wrist.transform.localRotation;
    }

    void OnEnable()
    {
        _wrist.OnTrashGrabbed += SwitchHeight;
    }

    public void CustomUpdate()
    {
        if (InputManager.Instance.PlayerActions.RotateHand.ReadValue<float>() == 0f)
            MoveSelfAndCamera();
        else
            RotateHand();

        LiftHand();

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

    void LiftHand()
    {
        _currentLiftDegrees = Mathf.Clamp(_currentLiftDegrees + (_shouldLift ? Time.deltaTime : -Time.deltaTime) * _armDegreesPerSecond, 0f, _armLiftDegrees);
        _elbowJoint.localRotation = Quaternion.Euler(_currentLiftDegrees, _elbowJoint.localEulerAngles.y, _elbowJoint.localEulerAngles.z);
    }

    void SwitchHeight(bool isGrabbing)
    {
        _shouldLift = isGrabbing;
    }
}
