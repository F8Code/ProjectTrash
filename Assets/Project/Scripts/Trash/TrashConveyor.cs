using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrashConveyor : MonoBehaviour
{
    const float MAXIMUM_TRASH_ANGULAR_VELOCITY = 3f;

    [Header("Conveyor settings")]
    [Tooltip("Speed at which trash moves in the conveyor local forward direction")]
    [SerializeField, Range(0.1f, 5f)] float _trashMovementSpeed = 1f;
    [Tooltip("If enabled, disables gravity effect of trash while on the conveyor")]
    [SerializeField] bool _disableTrashGravity = false;
    [Tooltip("Extra fling speed applied to trash when it leaves the conveyor")]
    [SerializeField, Range(0f, 2f)] float _trashFlingSpeed = 2f;

    Dictionary<Rigidbody, float> _trash = new();
    HashSet<Rigidbody> _ignoredTrash = new();

    void OnEnable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed += ManageIgnoredTrash;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trasRB = other.GetComponentInParent<Rigidbody>();

        if (_ignoredTrash.Contains(trasRB))
            return;
            
        if (_disableTrashGravity)
            trasRB.isKinematic = true;

        if (!_trash.ContainsKey(trasRB))
            _trash.Add(trasRB, 0f);
    }

    public void CustomUpdate()
    {
        foreach (Rigidbody trashRB in _trash.Keys.ToList())
        {
            trashRB.position += transform.forward * (_trashMovementSpeed + _trash[trashRB]) * Time.deltaTime;

            if (trashRB.isKinematic) continue;

            if (Vector3.Dot(trashRB.linearVelocity, transform.forward) <= 0f)
                _trash[trashRB] += Time.deltaTime * _trashMovementSpeed;
            else
                _trash[trashRB] = 0f;

            trashRB.angularVelocity = Vector3.ClampMagnitude(trashRB.angularVelocity, MAXIMUM_TRASH_ANGULAR_VELOCITY);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trashRB = other.GetComponentInParent<Rigidbody>();

        if (!_trash.ContainsKey(trashRB))
            return;

        trashRB.isKinematic = false;
        trashRB.linearVelocity += transform.forward * _trashFlingSpeed;

        _trash.Remove(trashRB);
    }

    void ManageIgnoredTrash(Trash trash, bool isGrabbed, Vector3 velocity)
    {
        Rigidbody trashRB = trash.gameObject.GetComponent<Rigidbody>();

        if (isGrabbed)
        {
            _ignoredTrash.Add(trashRB);
            _trash.Remove(trashRB);
        }
        else
        {
            _ignoredTrash.Remove(trashRB);
        }      
    }
    
    void OnDisable()
    {
        PlayerManager.Instance.Arm.Wrist.OnTrashGrabbed += ManageIgnoredTrash;
    }
}
