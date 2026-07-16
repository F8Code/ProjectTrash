using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPalm : MonoBehaviour
{
    [Header("Core settings")]
    [Tooltip("Trigger collider used for detecting trash in palm")]
    [SerializeField] Collider _detectionCollider;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 15f;
    [SerializeField] private LayerMask trashLayer;

    [Header("Telekinesis")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stopDistance = 0.5f;

    readonly HashSet<Trash> _contacts = new();

    public IReadOnlyCollection<Trash> Contacts => _contacts;

    public event Action<PlayerPalm, Trash> OnPalmContact;

    void Awake()
    {
        _detectionCollider.isTrigger = true;
    }

    void Update()
    {
        Debug.DrawLine(transform.position, -1 * transform.forward * rayDistance, Color.green, 1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        if (_contacts.Add(other.GetComponentInParent<Trash>()))
            OnPalmContact?.Invoke(this, other.GetComponentInParent<Trash>());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        _contacts.Remove(other.GetComponentInParent<Trash>());
    }

    //private RaycastHit _sellectedTrash;
    //private bool _trashDetected;
    //private Transform _trashTransform;

    //public bool CanPullTrash()
    //{
    //    Ray ray = new Ray(transform.position, -1 * transform.forward);
    //    return _trashDetected = Physics.Raycast(ray, out _sellectedTrash, rayDistance, trashLayer);
    //}

    //public void UpdateTrash()
    //{
    //    if (_trashDetected)
    //        _trashTransform = _sellectedTrash.transform;
    //    else 
    //        _trashTransform = null;
        
    //    var trashRigid = _trashTransform.GetComponent<Rigidbody>();
    //    trashRigid.useGravity = !_trashDetected;
    //    trashRigid.linearVelocity = Vector3.zero;
    //}

    //public void StartPull()
    //{
    //    if (_trashTransform != null)
    //    {
    //        Vector3 direction = transform.position - _trashTransform.position;

    //        if (direction.magnitude > stopDistance)
    //            _trashTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
    //        else
    //        {
    //            _trashTransform.position = transform.position;
    //            _trashDetected = false;
    //            UpdateTrash();
    //        }
    //    }
    //}
}
