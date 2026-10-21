using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWrist : MonoBehaviour
{
    struct ThrowMotionSample
    {
        public float Time;
        public Vector3 LinearVelocity;
        public Vector3 AngularVelocity;
    }

    [Header("References")]
    [Tooltip("Reference to the player's palm")]
    [SerializeField] PlayerPalm _palm;
    [Tooltip("Reference to the player's thumb fingertip joint")]
    [SerializeField] PlayerFingertip _thumb;
    [Tooltip("References to the remaining fingertip joints")]
    [SerializeField] PlayerFingertip[] _otherFingers;
    [Tooltip("Animator controlling the arm and hand movements")]
    [SerializeField] Animator _armAnimator;

    [Header("Grabbing settings")]
    [Tooltip("Speed at which the hand closes or opens during grab animation")]
    [SerializeField, Range(0.1f, 2f)] float _grabSpeed = 1f;

    [Header("Throwing settings")]
    [Tooltip("How many seconds of recent hand motion are averaged into the throw")]
    [SerializeField, Range(0.05f, 0.3f)] float _throwVelocitySampleWindow = 0.12f;
    [Tooltip("Multiplier applied to the thrown object's velocity based on hand movement")]
    [SerializeField, Range(0.1f, 2f)] float _thrownTrashSpeedMultiplier = 1f;
    [Tooltip("Maximum velocity that the trash can have upon being released from the hand")]
    [SerializeField, Range(0.1f, 10f)] float _thrownTrashVelocityLimit = 1f;
    [Tooltip("Releases slower than this are treated as drops instead of throws")]
    [SerializeField, Range(0f, 3f)] float _dropSpeedThreshold = 0.85f;
    [Tooltip("Minimum speed applied once a release counts as a throw")]
    [SerializeField, Range(0.1f, 8f)] float _minFlingSpeed = 2.4f;
    [SerializeField] bool _displayDebugLogTrashVelocityOnRelease = false;
    [Tooltip("Additional upward velocity applied when releasing a grabbed object")]
    [SerializeField, Range(0f, 2f)] float _thrownTrashBonusUpwardsVelocity = 1f;
    [Tooltip("How much heavy-item arm slowdown is undone at throw time (0 = keep slow throws, 1 = full compensation)")]
    [SerializeField, Range(0f, 1f)] float _heavyItemThrowCompensation = 0.7f;
    [Tooltip("Multiplier applied to averaged hand spin when the item is released")]
    [SerializeField, Range(0f, 2f)] float _thrownTrashAngularMultiplier = 1f;
    [SerializeField, Range(0f, 30f)] float _thrownTrashAngularVelocityLimit = 12f;

    [Header("Aim assist")]
    [Tooltip("If the throw already points near a bin, blend slightly toward that bin")]
    [SerializeField, Range(0f, 45f)] float _aimAssistConeDegrees = 28f;
    [SerializeField, Range(0f, 1f)] float _aimAssistStrength = 0.28f;
    [SerializeField] bool _preferMatchingBinForAimAssist = true;

    [Header("Collision ignore")]
    [SerializeField, Range(0f, 0.5f)] float _dropCollisionIgnoreDuration = 0.08f;
    [SerializeField, Range(0.05f, 1f)] float _throwCollisionIgnoreDuration = 0.35f;

    [Header("Audio Settings")]
    [Tooltip("Sound played when picking up the trash")]
    public AudioClip PickupSound;
    [Tooltip("Sound played when throwing the trash")]
    public AudioClip ThrowSound;
    [Tooltip("Grab sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _grabSoundVolume = 0.75f;
    [Tooltip("Throw sound volume multiplier")]
    [SerializeField, Range(0f, 1f)] private float _throwSoundVolume = 0.75f;

    public bool _isGrabbing = false;
    public bool _isTouching = false;
    bool _shouldGrab = false;
    float _currentGrab01;
    public PlayerFingertip[] _allFingers;
    GameObject _grabbedTrash = null;
    Transform _originalTrashParent;
    Vector3 _lastTrashPosition;
    Quaternion _lastTrashRotation = Quaternion.identity;
    readonly List<ThrowMotionSample> _throwMotionSamples = new();

    public Vector3 Position => GetCenterPosition();
    public bool IsGrabbing => _isGrabbing;
    public bool IsTouching => _isTouching;

    public event Action<Trash, bool, Vector3> OnTrashGrabbed;

    void Awake()
    {
        _allFingers = _otherFingers.Concat(new[] { _thumb }).ToArray();
    }

    void OnEnable()
    {
        foreach (PlayerFingertip finger in _allFingers)
            finger.OnFingerContact += HandleFingerContact;

        _palm.OnPalmContact += HandlePalmContact;
    }

    void HandleFingerContact(PlayerFingertip finger, Trash trash)
    {
        if (!_shouldGrab)
            return;

        if (_grabbedTrash != null)
            return;

        //If thumb touched trash, require any other finger to be touching it also to grab it
        if (finger == _thumb && !_otherFingers.Any(f => f.Contacts.Contains(trash)))
            return;

        //If any finger other than thumb touched trash, require the thumb to be touching it also to grab it
        if (finger != _thumb && !_thumb.Contacts.Contains(trash))
            return;

        GrabTrash(trash);
    }

    private void HandlePalmContact(PlayerPalm palm, Trash trash)
    {
        if (!_shouldGrab)
            return;

        if (_grabbedTrash != null)
            return;

        GrabTrash(trash);
    }

    public void CustomUpdate()
    {
        _currentGrab01 = Mathf.Clamp(_currentGrab01 + (_shouldGrab ? 1f : -1f) * _grabSpeed * Time.deltaTime, 0f, 1f);

        _isTouching = false;
        foreach (PlayerFingertip finger in _allFingers)
        {
            if (_shouldGrab && finger.Contacts.Any())
                _isTouching = true;
            else
                _armAnimator.SetFloat(finger.AnimatorVariable, _currentGrab01);
        }

        if (_shouldGrab && _palm.Contacts.Any())
            _isTouching = true;

        bool releasing = !_shouldGrab && _grabbedTrash != null;
        if (_grabbedTrash != null)
        {
            bool liftInjectingMotion = !releasing && IsGrabLiftMoving();
            if (liftInjectingMotion)
                ResetThrowMotionBaseline();
            else
                SampleThrowMotion();
        }

        if (releasing)
            ReleaseTrash();
    }

    void GrabTrash(Trash trash)
    {
        //Internal logic
        _isGrabbing = true;
        _grabbedTrash = trash.gameObject;

        //Parenting
        _originalTrashParent = _grabbedTrash.transform.parent;
        _grabbedTrash.transform.SetParent(transform);

        //Physics
        _grabbedTrash.GetComponent<Rigidbody>().isKinematic = true;

        ResetThrowMotionBaseline();
        _throwMotionSamples.Clear();

        //Broadcast event
        OnTrashGrabbed?.Invoke(trash, true, Vector3.zero);

        //Audio
        AudioManager.Instance.PlayAudio(PickupSound, _grabSoundVolume, AudioPlaybackContext.PlaybackPriority.Medium, transform.position, false, 1, AudioManager.AudioMixerType.SFX);
    }

    void ReleaseTrash()
    {
        ComputeOutgoingThrow(out Vector3 throwVelocity, out Vector3 throwAngularVelocity, out float collisionIgnoreDuration, out bool isThrow);

        //Parenting
        _grabbedTrash.transform.parent = _originalTrashParent;

        //Disable trash-hand collision for a moment
        StartCoroutine(TemporarilyIgnoreTrashCollisions(_grabbedTrash, collisionIgnoreDuration));

        //Physics and velocity
        Rigidbody trashRB = _grabbedTrash.GetComponent<Rigidbody>();
        trashRB.isKinematic = false;
        trashRB.linearVelocity = throwVelocity;
        trashRB.angularVelocity = throwAngularVelocity;

        //Broadcast event
        OnTrashGrabbed?.Invoke(_grabbedTrash.GetComponent<Trash>(), false, trashRB.linearVelocity);

        //Internal logic
        _isGrabbing = false;
        _grabbedTrash = null;
        _throwMotionSamples.Clear();

        //Audio
        AudioManager.Instance.PlayAudio(ThrowSound, _throwSoundVolume, AudioPlaybackContext.PlaybackPriority.Medium, transform.position, false, 1, AudioManager.AudioMixerType.SFX);

        if (_displayDebugLogTrashVelocityOnRelease)
            Debug.Log($"Released trash velocity: {trashRB.linearVelocity.magnitude:F2} ({(isThrow ? "throw" : "drop")})");
    }

    void SampleThrowMotion()
    {
        float dt = Time.deltaTime;
        Vector3 position = _grabbedTrash.transform.position;
        Quaternion rotation = _grabbedTrash.transform.rotation;

        if (dt > 0.0001f)
        {
            Vector3 linearVelocity = (position - _lastTrashPosition) / dt;
            Vector3 angularVelocity = GetAngularVelocity(_lastTrashRotation, rotation, dt);

            _throwMotionSamples.Add(new ThrowMotionSample
            {
                Time = Time.time,
                LinearVelocity = linearVelocity,
                AngularVelocity = angularVelocity
            });
        }

        float windowStart = Time.time - _throwVelocitySampleWindow;
        _throwMotionSamples.RemoveAll(sample => sample.Time < windowStart);

        _lastTrashPosition = position;
        _lastTrashRotation = rotation;
    }

    void ResetThrowMotionBaseline()
    {
        if (_grabbedTrash == null)
            return;

        _lastTrashPosition = _grabbedTrash.transform.position;
        _lastTrashRotation = _grabbedTrash.transform.rotation;
    }

    void ComputeOutgoingThrow(out Vector3 throwVelocity, out Vector3 throwAngularVelocity, out float collisionIgnoreDuration, out bool isThrow)
    {
        GetAveragedThrowMotion(out Vector3 linearVelocity, out Vector3 angularVelocity);

        linearVelocity *= _thrownTrashSpeedMultiplier * GetHeavyItemThrowCompensation();
        angularVelocity *= _thrownTrashAngularMultiplier;

        float speed = linearVelocity.magnitude;
        isThrow = speed >= _dropSpeedThreshold;

        if (!isThrow)
        {
            throwVelocity = linearVelocity;
            throwAngularVelocity = Vector3.ClampMagnitude(angularVelocity, _thrownTrashAngularVelocityLimit * 0.35f);
            collisionIgnoreDuration = _dropCollisionIgnoreDuration;
            return;
        }

        Vector3 direction = linearVelocity.normalized;
        speed = Mathf.Clamp(speed, _minFlingSpeed, _thrownTrashVelocityLimit);
        direction = ApplyAimAssist(direction, _grabbedTrash.GetComponent<Trash>());

        float throwStrength01 = Mathf.InverseLerp(_minFlingSpeed, _thrownTrashVelocityLimit, speed);
        throwVelocity = direction * speed + Vector3.up * throwStrength01 * _thrownTrashBonusUpwardsVelocity;
        throwAngularVelocity = Vector3.ClampMagnitude(angularVelocity, _thrownTrashAngularVelocityLimit);
        collisionIgnoreDuration = Mathf.Lerp(_dropCollisionIgnoreDuration, _throwCollisionIgnoreDuration, throwStrength01);
    }

    void GetAveragedThrowMotion(out Vector3 linearVelocity, out Vector3 angularVelocity)
    {
        linearVelocity = Vector3.zero;
        angularVelocity = Vector3.zero;

        if (_throwMotionSamples.Count == 0)
            return;

        float totalWeight = 0f;
        float windowStart = Time.time - _throwVelocitySampleWindow;

        foreach (ThrowMotionSample sample in _throwMotionSamples)
        {
            float weight = 1f + (sample.Time - windowStart) / Mathf.Max(_throwVelocitySampleWindow, 0.0001f);
            linearVelocity += sample.LinearVelocity * weight;
            angularVelocity += sample.AngularVelocity * weight;
            totalWeight += weight;
        }

        linearVelocity /= totalWeight;
        angularVelocity /= totalWeight;
    }

    float GetHeavyItemThrowCompensation()
    {
        if (PlayerManager.Instance == null)
            return 1f;

        float debuf = Mathf.Max(PlayerManager.Instance.Arm.ArmSpeedDebuf, 0.1f);
        return Mathf.Lerp(1f, 1f / debuf, _heavyItemThrowCompensation);
    }

    Vector3 ApplyAimAssist(Vector3 throwDirection, Trash trash)
    {
        if (_aimAssistStrength <= 0f || trash == null || TrashManager.Instance == null)
            return throwDirection;

        Vector3 origin = _grabbedTrash != null ? _grabbedTrash.transform.position : transform.position;
        TrashCan bestCan = null;
        float bestScore = float.PositiveInfinity;

        foreach (TrashCan trashCan in TrashManager.Instance.TrashCans)
        {
            if (trashCan == null)
                continue;

            Vector3 toCan = trashCan.ThrowAimPoint - origin;
            if (toCan.sqrMagnitude < 0.01f)
                continue;

            float angle = Vector3.Angle(throwDirection, toCan);
            if (angle > _aimAssistConeDegrees)
                continue;

            bool matching = trashCan.AcceptedType == trash.Type;
            if (_preferMatchingBinForAimAssist && matching)
                angle *= 0.5f;

            if (angle < bestScore)
            {
                bestScore = angle;
                bestCan = trashCan;
            }
        }

        if (bestCan == null)
            return throwDirection;

        Vector3 assistedDirection = (bestCan.ThrowAimPoint - origin).normalized;
        return Vector3.Slerp(throwDirection, assistedDirection, _aimAssistStrength).normalized;
    }

    static Vector3 GetAngularVelocity(Quaternion from, Quaternion to, float deltaTime)
    {
        Quaternion delta = to * Quaternion.Inverse(from);
        delta.ToAngleAxis(out float angleDegrees, out Vector3 axis);

        if (angleDegrees > 180f)
            angleDegrees -= 360f;

        if (axis.sqrMagnitude < 0.0001f || Mathf.Abs(angleDegrees) < 0.01f)
            return Vector3.zero;

        return axis.normalized * (angleDegrees * Mathf.Deg2Rad / deltaTime);
    }

    bool IsGrabLiftMoving()
    {
        return PlayerManager.Instance != null && PlayerManager.Instance.Arm.IsGrabLiftMoving;
    }

    IEnumerator TemporarilyIgnoreTrashCollisions(GameObject releasedObject, float duration)
    {
        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] objectColliders = releasedObject.GetComponentsInChildren<Collider>();

        foreach (Collider handCol in handColliders)
            foreach (Collider objCol in objectColliders)
                if (handCol && objCol) Physics.IgnoreCollision(handCol, objCol, true);

        yield return new WaitForSeconds(Mathf.Max(0f, duration));

        if (releasedObject == null)
            yield break;

        objectColliders = releasedObject.GetComponentsInChildren<Collider>();
        foreach (Collider handCol in handColliders)
            foreach (Collider objCol in objectColliders)
                if (handCol && objCol) Physics.IgnoreCollision(handCol, objCol, false);
    }

    void OnDisable()
    {
        foreach (PlayerFingertip finger in _allFingers)
            finger.OnFingerContact -= HandleFingerContact;

        if (_palm != null)
            _palm.OnPalmContact -= HandlePalmContact;
    }

    Vector3 GetCenterPosition()
    {
        Vector3 position = Vector3.zero;
        foreach (PlayerFingertip finger in _allFingers)
            position += finger.transform.position;
        return position / _allFingers.Length;
    }

    public void ShouldGrab(bool shouldGrab) => _shouldGrab = shouldGrab;
}
