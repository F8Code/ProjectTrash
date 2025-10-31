using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFingertip : MonoBehaviour
{
    [SerializeField] string _animatorVariable;
    [SerializeField] Collider _detectionCollider;

    readonly HashSet<GameObject> _contacts = new();

    public string AnimatorVariable => _animatorVariable;
    public IReadOnlyCollection<GameObject> Contacts => _contacts;

    public event Action<PlayerFingertip, GameObject> OnFingerContact;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        if (_contacts.Add(other.gameObject))
            OnFingerContact?.Invoke(this, other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        _contacts.Remove(other.gameObject);
    }
}
