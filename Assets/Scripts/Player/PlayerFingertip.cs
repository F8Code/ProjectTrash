using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFingertip : MonoBehaviour
{
    [Header("Core settings")]
    [Tooltip("Animator variable that controls this specific finger movement")]
    [SerializeField] string _animatorVariable;
    [Tooltip("Trigger collider used for detecting trash in this finger")]
    [SerializeField] Collider _detectionCollider;

    void Awake()
    {
        _detectionCollider.isTrigger = true;
    }

    readonly HashSet<Trash> _contacts = new();

    public string AnimatorVariable => _animatorVariable;
    public IReadOnlyCollection<Trash> Contacts => _contacts;

    public event Action<PlayerFingertip, Trash> OnFingerContact;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        if (_contacts.Add(other.GetComponentInParent<Trash>()))
            OnFingerContact?.Invoke(this, other.GetComponentInParent<Trash>());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        _contacts.Remove(other.GetComponentInParent<Trash>());
    }
}
