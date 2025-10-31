using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class PlayerWrist : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerFingertip _thumb;
    [SerializeField] PlayerFingertip[] _otherFingers;
    [SerializeField] Animator _armAnimator;

    [Header("Grabbing settings")]
    [SerializeField, Range(0.1f, 2f)] float _grabSpeed = 1f;

    float _currentGrab01 = 0f;

    PlayerFingertip[] _allFingers;

    void Awake()
    {
        _allFingers = _otherFingers.Concat(new[] { _thumb }).ToArray();
    }

    void OnEnable()
    {
        foreach (PlayerFingertip finger in _allFingers)
        {
            finger.OnFingerContact += HandleFingerContact;
        }
    }

    void HandleFingerContact(PlayerFingertip finger, GameObject gameObject)
    {
        if (finger == _thumb)
        {
            if (_otherFingers.Any(f => f.Contacts.Contains(gameObject)))
                ; //Grab object
        }
        else
        {
            if (_thumb.Contacts.Contains(gameObject))
                ; //GrabObject
        }
    }

    public void CustomUpdate()
    {
        float grab = InputManager.Instance.PlayerActions.Grab.ReadValue<float>();
        _currentGrab01 = Mathf.Clamp(_currentGrab01 + (-1f + 2f * grab) * _grabSpeed * Time.deltaTime, 0f, 1f);

        foreach (PlayerFingertip finger in _allFingers)
        {
            _armAnimator.SetFloat(finger.AnimatorVariable, _currentGrab01);
        }
    }

    void OnDisable()
    {
        foreach (PlayerFingertip finger in _allFingers)
        {
            finger.OnFingerContact -= HandleFingerContact;
        }
    }
}
