using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PlayerWrist : MonoBehaviour
{
    [Header("References")]
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
    [Tooltip("Multiplier applied to the thrown object's velocity based on hand movement")]
    [SerializeField, Range(0.1f, 2f)] float _thrownTrashSpeedMultiplier = 1f;
    [Tooltip("Maximum velocity that the trash can have upon being released from the hand")]
    [SerializeField, Range(0.1f, 5f)] float _thrownTrashVelocityLimit = 1f;
    [SerializeField] bool _displayDebugLogTrashVelocityOnRelease = false;
    [Tooltip("Additional upward velocity applied when releasing a grabbed object")]
    [SerializeField, Range(0.1f, 2f)] float _thrownTrashBonusUpwardsVelocity = 1f;

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
    Vector3 _lastTrashPosition, _trashVelocity;

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
    }

    void FixedUpdate()
    {
        if (_grabbedTrash == null)
            return;

        Vector3 newVelocity = (_grabbedTrash.transform.position - _lastTrashPosition) / Time.fixedDeltaTime;
        _trashVelocity = newVelocity.magnitude > _trashVelocity.magnitude ? newVelocity : _trashVelocity * 0.9f;
        _lastTrashPosition = _grabbedTrash.transform.position;
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

        if (!_shouldGrab && _grabbedTrash != null)
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

        //Broadcast event
        OnTrashGrabbed?.Invoke(trash, true, Vector3.zero);

        //Audio
        AudioManager.Instance.PlayAudio(PickupSound, _grabSoundVolume, AudioPlaybackContext.PlaybackPriority.Medium, transform.position);
    }
    
    void ReleaseTrash()
    {
        //Parenting
        _grabbedTrash.transform.parent = _originalTrashParent;

        //Disable trash-hand collision for a moment
        StartCoroutine(TemporarilyIgnoreTrashCollisions(_grabbedTrash));
        
        //Physics and velocity
        Rigidbody trashRB = _grabbedTrash.GetComponent<Rigidbody>();
        trashRB.isKinematic = false;
        trashRB.linearVelocity = Vector3.ClampMagnitude(_trashVelocity * _thrownTrashSpeedMultiplier, _thrownTrashVelocityLimit) + Vector3.up * _thrownTrashBonusUpwardsVelocity;

        //Broadcast event
        OnTrashGrabbed?.Invoke(_grabbedTrash.GetComponent<Trash>(), false, trashRB.linearVelocity);

        //Internal logic
        _isGrabbing = false;
        _grabbedTrash = null;

        //Audio
        AudioManager.Instance.PlayAudio(ThrowSound, _throwSoundVolume, AudioPlaybackContext.PlaybackPriority.Medium, transform.position);
        
        //DEBUG
        if (_displayDebugLogTrashVelocityOnRelease) Debug.Log("Released trash velocity: " + trashRB.linearVelocity.magnitude);
    }

    IEnumerator TemporarilyIgnoreTrashCollisions(GameObject releasedObject)
    {
        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider[] objectColliders = releasedObject.GetComponentsInChildren<Collider>();

        foreach (Collider handCol in handColliders)
            foreach (Collider objCol in objectColliders)
                if (handCol && objCol) Physics.IgnoreCollision(handCol, objCol, true);

        yield return new WaitForSeconds(0.25f);

        foreach (Collider handCol in handColliders)
            foreach (Collider objCol in objectColliders)
                if (handCol && objCol) Physics.IgnoreCollision(handCol, objCol, false);
    }

    void OnDisable()
    {
        foreach (PlayerFingertip finger in _allFingers)
            finger.OnFingerContact -= HandleFingerContact;
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
