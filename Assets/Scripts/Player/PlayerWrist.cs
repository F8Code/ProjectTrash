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
    [Tooltip("Additional upward velocity applied when releasing a grabbed object")]
    [SerializeField, Range(0.1f, 2f)] float _thrownTrashBonusUpwardsVelocity = 1f;

    bool _isGrabbing = false;
    float _currentGrab01;
    PlayerFingertip[] _allFingers;
    GameObject _grabbedTrash = null;
    Transform _originalTrashParent;
    Vector3 _lastTrashPosition, _trashVelocity;

    public event Action<bool> OnTrashGrabbed;

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

        _trashVelocity = (_grabbedTrash.transform.position - _lastTrashPosition) / Time.fixedDeltaTime;
        _lastTrashPosition = _grabbedTrash.transform.position;
    }

    void HandleFingerContact(PlayerFingertip finger, GameObject trash)
    {
        if (!_isGrabbing)
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
        _isGrabbing = InputManager.Instance.PlayerActions.Grab.ReadValue<float>() == 1f;
        _currentGrab01 = Mathf.Clamp(_currentGrab01 + (_isGrabbing ? 1f : -1f) * _grabSpeed * Time.deltaTime, 0f, 1f);

        foreach (PlayerFingertip finger in _allFingers)
        {
            if (_isGrabbing && finger.Contacts.Any())
                continue;

            _armAnimator.SetFloat(finger.AnimatorVariable, _currentGrab01);
        }

        if (!_isGrabbing && _grabbedTrash != null)
            ReleaseTrash();
    }

    void GrabTrash(GameObject trash)
    {
        _grabbedTrash = trash;
        _originalTrashParent = _grabbedTrash.transform.parent;
        _grabbedTrash.transform.SetParent(transform);
        _grabbedTrash.GetComponent<Rigidbody>().isKinematic = true;
        OnTrashGrabbed?.Invoke(true);
    }
    
    void ReleaseTrash()
    {
        _grabbedTrash.GetComponent<Rigidbody>().isKinematic = false;
        _grabbedTrash.GetComponent<Rigidbody>().linearVelocity = _trashVelocity * _thrownTrashSpeedMultiplier + Vector3.up * _thrownTrashBonusUpwardsVelocity;
        _grabbedTrash.transform.parent = _originalTrashParent;
        StartCoroutine(TemporarilyIgnoreTrashCollisions(_grabbedTrash));
        OnTrashGrabbed?.Invoke(false);
        _grabbedTrash = null;
    }

    IEnumerator TemporarilyIgnoreTrashCollisions(GameObject releasedObject)
    {
        Collider[] handColliders = GetComponentsInChildren<Collider>();
        Collider objectCollider = releasedObject.GetComponent<Collider>();

        foreach (Collider handCol in handColliders)
            Physics.IgnoreCollision(handCol, objectCollider, true);

        yield return new WaitForSeconds(0.25f);

        foreach (Collider handCol in handColliders)
            if (handCol && objectCollider)
                Physics.IgnoreCollision(handCol, objectCollider, false);
    }

    void OnDisable()
    {
        foreach (PlayerFingertip finger in _allFingers)
            finger.OnFingerContact -= HandleFingerContact;
    }
}
