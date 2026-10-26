using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPalm : MonoBehaviour
{
    [Header("Core settings")]
    [Tooltip("Trigger collider used for detecting trash in palm")]
    [SerializeField] Collider _detectionCollider;

    readonly HashSet<Trash> _contacts = new();

    public IReadOnlyCollection<Trash> Contacts => _contacts;

    public event Action<PlayerPalm, Trash> OnPalmContact;

    void Awake()
    {
        if (_detectionCollider != null)
            _detectionCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Trash trash = other.GetComponentInParent<Trash>();
        if (trash != null && _contacts.Add(trash))
            OnPalmContact?.Invoke(this, trash);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        _contacts.Remove(other.GetComponentInParent<Trash>());
    }
}
