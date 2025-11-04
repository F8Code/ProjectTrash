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
    [Tooltip("Sideways camera rotation range in degrees")]
    [SerializeField] Vector2 _cameraSidewaysRotationRangeDegrees = new Vector2(-30f, 5f);

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
    [SerializeField, Range(0, 60)] uint _armLiftDegrees = 30;
    [Tooltip("Speed at which the arm lifts or lowers in degrees per second")]
    [SerializeField, Range(10f, 120f)] float _armDegreesPerSecond = 30f;

    public PlayerWrist Wrist => _wrist;

    Quaternion _baseElbowRotation;
    Quaternion _baseWristRotation;
    float _currentElbowZ, _currentWristX;
    float _currentLiftDegrees = 0f;
    bool _shouldLift = false;

    float _armSpeedDebuf = 1f;
    public void SetArmSpeedDebuf(float debuf) => _armSpeedDebuf = debuf;

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

    [SerializeField] Transform _sliceCenter;
    [SerializeField] float _sliceYRotationDegrees = 15f;
    [SerializeField] float _sliceAngle = 45f;
    [SerializeField] float _sliceLength = 3f;

    void MoveSelfAndCamera()
    {
        if (Time.time < 0.5f) return; //Prevent weird mouseInput readings at the very start of the game

        //Reading movement
        Vector2 mouseInput = InputManager.Instance.PlayerActions.MoveHand.ReadValue<Vector2>() * ARM_MOVEMENT_SCALING * _armSpeedDebuf;
        Vector3 movementVector = new Vector3(mouseInput.x, 0, mouseInput.y);

        //Limiting movement
        Vector3 targetLocalPosition = transform.localPosition + movementVector * _armMovementStrength;

        //IS ISNIDE CIRCLE?
        targetLocalPosition = ClampInsideCircle(targetLocalPosition);
        movementVector = (targetLocalPosition - transform.localPosition) / _armMovementStrength;

        //Movement
        transform.position += movementVector * _armMovementStrength;
        _camera.transform.position += movementVector * _cameraMovementStrength;

        //Y axis Rotation based on movement
        float inverseLerp = Mathf.InverseLerp(_armXPositionRange.x, _armXPositionRange.y, transform.localPosition.x);
        float armRotationY = Mathf.Lerp(_armSidewaysRotationRangeDegrees.x, _armSidewaysRotationRangeDegrees.y, inverseLerp);
        _elbowJoint.localRotation = _baseElbowRotation * Quaternion.Euler(_elbowJoint.localEulerAngles.x, armRotationY, _elbowJoint.localEulerAngles.z);

        float cameraRotationY = Mathf.Lerp(_cameraSidewaysRotationRangeDegrees.x, _cameraSidewaysRotationRangeDegrees.y, inverseLerp);
        _camera.transform.localRotation = Quaternion.Euler(_camera.transform.localEulerAngles.x, cameraRotationY, _camera.transform.localEulerAngles.z);
    }

    Vector3 ClampInsideCircle(Vector3 targetLocalPosition)
    {
        Vector3 centerLocalPos = transform.parent.InverseTransformPoint(_sliceCenter.position);
        Vector2 targetXZ = new Vector2(targetLocalPosition.x, targetLocalPosition.z);
        Vector2 centerXZ = new Vector2(centerLocalPos.x, centerLocalPos.z);
        Vector2 toTarget = targetXZ - centerXZ;

        float distance = toTarget.magnitude;

        float alphaRad = _sliceYRotationDegrees * Mathf.Deg2Rad;
        float halfAngleRad = _sliceAngle * 0.5f * Mathf.Deg2Rad;

        float pointAngle = Mathf.Atan2(toTarget.x, toTarget.y);
        float delta = Mathf.DeltaAngle(alphaRad * Mathf.Rad2Deg, pointAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        if (Mathf.Abs(delta) > halfAngleRad)
        {
            float clampedAngle = alphaRad + Mathf.Sign(delta) * halfAngleRad;
            Vector2 dir = new Vector2(Mathf.Sin(clampedAngle), Mathf.Cos(clampedAngle));
            toTarget = dir * distance;
        }

        if (distance > _sliceLength)
            toTarget = toTarget.normalized * _sliceLength;

        Vector2 clampedXZ = centerXZ + toTarget;
        return new Vector3(clampedXZ.x, targetLocalPosition.y, clampedXZ.y);
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

    void OnDisable()
    {
        _wrist.OnTrashGrabbed -= SwitchHeight;
    }
}
