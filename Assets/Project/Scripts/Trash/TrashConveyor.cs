using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrashConveyor : MonoBehaviour
{
    const float CONVEYOR_SPEED_MODIFIER = 0.1f;
    const float CONVEYOR_FLING_MODIFIER = 1.5f;

    [Header("Conveyor settings")]
    [Tooltip("Speed at which trash moves in the conveyor local forward direction")]
    [SerializeField, Range(0.1f, 5f)] float _trashMovementSpeed = 1f;
    [Tooltip("If enabled, disables gravity effect of trash while on the conveyor")]
    [SerializeField] bool _disableTrashGravity = false;
    [Tooltip("Extra fling speed applied to trash when it leaves the conveyor")]
    [SerializeField, Range(0f, 2f)] float _trashFlingSpeed = 2f;

    HashSet<Rigidbody> _trash = new();
    HashSet<Rigidbody> _ignoredTrash = new();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trash = other.GetComponentInParent<Rigidbody>();

        if (_ignoredTrash.Contains(trash))
            return;
            
        if (_disableTrashGravity)
            trash.isKinematic = true;

        _trash.Add(trash);
    }

    public void CustomUpdate()
    {
        foreach (Rigidbody trashRB in _trash)
        {
            trashRB.position += transform.forward * _trashMovementSpeed * Time.fixedDeltaTime * CONVEYOR_SPEED_MODIFIER;
            if (!trashRB.isKinematic) trashRB.angularVelocity *= 0.9f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trashRB = other.GetComponentInParent<Rigidbody>();

        if (_ignoredTrash.Contains(trashRB))
            return;

        trashRB.isKinematic = false;
        trashRB.linearVelocity += transform.forward * _trashFlingSpeed * CONVEYOR_FLING_MODIFIER;

        _trash.Remove(trashRB);

        _ignoredTrash.Add(trashRB);
    }

    public void StopIgnoringTrash(Trash trash)
    {
        Rigidbody trashRB = trash.gameObject.GetComponent<Rigidbody>();
        _ignoredTrash.Remove(trashRB);
    }
}
