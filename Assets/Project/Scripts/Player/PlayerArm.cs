using System;
using System.Collections;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerArm : MonoBehaviour
{
    const float ARM_MOVEMENT_SCALING = 0.001f;
    const float ARM_ROTATION_SCALING = 0.5f;
    const float DEFAULT_SPHERECAST_RADIUS = 0.2f;
    const float DEFAULT_SPHERECAST_RANGE = 0.1f;

    [Header("References")]
    [Tooltip("Reference to the wrist joint of the arm")]
    [SerializeField] PlayerWrist _wrist;
    [Tooltip("Reference to the player's camera")]
    [SerializeField] PlayerCamera _camera;
    [Tooltip("Transform to the elbow joint of the arm")]
    [SerializeField] Transform _elbowJoint;

    [Header("Arm movement area settings")]
    [Tooltip("Center point of the circular slice that defines the arm's allowed movement area")]
    [SerializeField] Transform _circleSliceCenter;
    [Tooltip("Rotation of the circular slice around the Y axis in degrees (defines the slice's facing direction)")]
    [SerializeField] float _circleSliceYRotationDegrees = 0f;
    [Tooltip("Total angle of the circular slice in degrees")]
    [SerializeField] float _circleSliceAngle = 120f;
    [Tooltip("Maximum distance you can reach from the circular slice center")]
    [SerializeField] float _circleSliceMaxRadius = 1f;
    [Tooltip("Minimum distance you can reach from the circular slice center")]
    [SerializeField] float _circleSliceMinRadius = 1f;

    [Header("Arm movement settings")]
    [Tooltip("Strength of the arm movement relative to mouse input")]
    [SerializeField, Range(0.1f, 2f)] float _armMovementStrength = 1f;
    [Tooltip("Strength of the camera movement relative to the arm")]
    [SerializeField, Range(0f, 2f)] float _cameraMovementStrength = 0.5f;
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
    [Tooltip("Lift angle of the arm in degrees when trash is not being grabbed")]
    [SerializeField, Range(0, 80)] uint _emptyHandLiftDegrees = 20;
    [Tooltip("Lift angle of the arm in degrees when trash is being held")]
    [SerializeField, Range(0, 80)] uint _grabbingHandLiftDegrees = 45;
    [Tooltip("Speed at which the arm lifts or lowers in degrees per second")]
    [SerializeField, Range(10f, 120f)] float _armDegreesPerSecond = 30f;

    [Header("Trash detection settings")]
    [Tooltip("Modifier of the size of the area around the hand in which trash is detected")]
    [SerializeField, Range(0.1f, 2f)] float _trashDetectionRadiusMultiplier = 1f;
    [Tooltip("Modifier of the height of the area scanned bellow the hand for trash")]
    [SerializeField, Range(0.5f, 2f)] float _trashDetectionHeightMultiplier = 1f;

    public PlayerWrist Wrist => _wrist;

    Quaternion _baseElbowRotation;
    Quaternion _baseWristRotation;
    float _currentElbowZ, _currentWristX;
    float _currentLiftDegrees = 0f;

    float _armSpeedDebuf = 1f;
    public void SetArmSpeedDebuf(float debuf) => _armSpeedDebuf = debuf;

    void Awake()
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

        SetHandHeightAndGrab();

        _wrist.CustomUpdate();
    }

    void MoveSelfAndCamera()
    {
        if (Time.time < 0.5f) return; //Prevent weird mouseInput readings at the very start of the game

        //Reading movement
        Vector2 mouseInput = InputManager.Instance.PlayerActions.MoveHand.ReadValue<Vector2>() * ARM_MOVEMENT_SCALING * _armSpeedDebuf;
        Vector3 movementVector = new Vector3(_camera.transform.right.x, 0f, _camera.transform.right.z) * mouseInput.x + new Vector3(_camera.transform.forward.x, 0f, _camera.transform.forward.z) * mouseInput.y;

        //Limiting movement
        Vector3 targetPosition = transform.position + movementVector * _armMovementStrength;

        ClampInsideCircleSlice(ref targetPosition, out float angleInverseLerp);
        movementVector = (targetPosition - transform.position) / _armMovementStrength;

        //Movement
        transform.position += movementVector * _armMovementStrength;
        _camera.transform.position += movementVector * _cameraMovementStrength;

        //Y axis Rotation based on movement
        float armRotationY = Mathf.Lerp(_armSidewaysRotationRangeDegrees.x, _armSidewaysRotationRangeDegrees.y, angleInverseLerp);
        _elbowJoint.localRotation = _baseElbowRotation * Quaternion.Euler(_elbowJoint.localEulerAngles.x, armRotationY, _elbowJoint.localEulerAngles.z);

        float cameraRotationY = Mathf.Lerp(_cameraSidewaysRotationRangeDegrees.x, _cameraSidewaysRotationRangeDegrees.y, angleInverseLerp);
        _camera.transform.localRotation = Quaternion.Euler(_camera.transform.localEulerAngles.x, cameraRotationY, _camera.transform.localEulerAngles.z);
    }

    void ClampInsideCircleSlice(ref Vector3 targetPosition, out float angleInverseLerp)
    {
        Vector3 sliceCenter = _circleSliceCenter.position;

        Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);
        Vector2 centerXZ = new Vector2(sliceCenter.x, sliceCenter.z);
        Vector2 centerToTarget = targetXZ - centerXZ;

        float distanceFromCenter = centerToTarget.magnitude;
        float sliceDirection = _circleSliceYRotationDegrees * Mathf.Deg2Rad;
        float sliceHalfAngle = _circleSliceAngle * 0.5f * Mathf.Deg2Rad;

        float currentAngle = Mathf.Atan2(centerToTarget.x, centerToTarget.y);
        float currentAngleOffset = Mathf.DeltaAngle(sliceDirection * Mathf.Rad2Deg, currentAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        //Angle clamping
        if (Mathf.Abs(currentAngleOffset) > sliceHalfAngle)
        {
            float clampedAngle = sliceDirection + Mathf.Sign(currentAngleOffset) * sliceHalfAngle;
            Vector2 clampedDirection = new Vector2(Mathf.Sin(clampedAngle), Mathf.Cos(clampedAngle));
            centerToTarget = clampedDirection * distanceFromCenter;
        }

        //Distance clamping
        if (distanceFromCenter > _circleSliceMaxRadius || distanceFromCenter < _circleSliceMinRadius)
            centerToTarget = centerToTarget.normalized * Mathf.Clamp(distanceFromCenter, _circleSliceMinRadius, _circleSliceMaxRadius);

        Vector2 clampedXZ = centerXZ + centerToTarget;
        targetPosition = new Vector3(clampedXZ.x, targetPosition.y, clampedXZ.y);
        angleInverseLerp = ((currentAngleOffset / sliceHalfAngle) + 1f) * 0.5f;
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

    void SetHandHeightAndGrab()
    {
        bool isTryingToGrab = InputManager.Instance.PlayerActions.Grab.ReadValue<float>() == 1f;

        bool shouldWristGrab = isTryingToGrab && (_wrist.IsGrabbing || TrashInProximity());
        _wrist.ShouldGrab(shouldWristGrab);

        if(shouldWristGrab && _wrist.IsTouching && !_wrist.IsGrabbing)
            return;

        bool shouldBeLifted = _wrist.IsGrabbing || !isTryingToGrab;
        float targetLiftAngle = _wrist.IsGrabbing ? _grabbingHandLiftDegrees : _emptyHandLiftDegrees;

        _currentLiftDegrees = Mathf.Clamp(_currentLiftDegrees + (shouldBeLifted ? Time.deltaTime : -Time.deltaTime) * _armDegreesPerSecond, 0f, targetLiftAngle);
        _elbowJoint.localRotation = Quaternion.Euler(_currentLiftDegrees, _elbowJoint.localEulerAngles.y, _elbowJoint.localEulerAngles.z); 
    }

    bool TrashInProximity()
    {
        float radius = DEFAULT_SPHERECAST_RADIUS * _trashDetectionRadiusMultiplier;
        float range = DEFAULT_SPHERECAST_RANGE * _trashDetectionHeightMultiplier;
        int layerMask = ~((1 << GameConstants.Layer.Default) | (1 << GameConstants.Layer.Player));
            
        return Physics.SphereCast(_wrist.Position + Vector3.up * (0.05f + radius), radius, Vector3.down, out RaycastHit hit, range, layerMask);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_circleSliceCenter == null)
            return;

        Vector3 center = _circleSliceCenter.position;
        Quaternion rotation = Quaternion.Euler(0f, _circleSliceYRotationDegrees, 0f);
        Vector3 forward = rotation * Vector3.forward;
        float halfAngle = _circleSliceAngle * 0.5f;

        Handles.color = new Color(0f, 0.8f, 1f, 1f);

        //Outer and inner ring
        Handles.DrawWireArc(center, Vector3.up, Quaternion.Euler(0f, -halfAngle, 0f) * forward, _circleSliceAngle, _circleSliceMaxRadius);
        Handles.DrawWireArc(center, Vector3.up, Quaternion.Euler(0f, -halfAngle, 0f) * forward, _circleSliceAngle, _circleSliceMinRadius);

        //Left side
        Vector3 leftDir = Quaternion.Euler(0f, -halfAngle, 0f) * forward;
        Handles.DrawLine(center + leftDir * _circleSliceMinRadius, center + leftDir * _circleSliceMaxRadius);

        //Right side
        Vector3 rightDir = Quaternion.Euler(0f, halfAngle, 0f) * forward;
        Handles.DrawLine(center + rightDir * _circleSliceMinRadius, center + rightDir * _circleSliceMaxRadius);
    }
#endif
}
