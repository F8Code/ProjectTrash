using System.Collections.Generic;
using UnityEngine;

public class TrashConveyor : MonoBehaviour
{
    const float CONVEYOR_SPEED_MODIFIER = 0.1f;

    [Header("Conveyor settings")]
    [Tooltip("Speed at which trash moves in the conveyor local forward direction")]
    [SerializeField, Range(0.1f, 5f)] float _trashMovementSpeed = 1f;
    [Tooltip("If enabled, disables gravity effect of trash while on the conveyor")]
    [SerializeField] bool _disableTrashGravity = false;
    [Tooltip("Extra fling speed applied to trash when it leaves the conveyor")]
    [SerializeField, Range(0f, 2f)] float _trashFlingSpeed = 2f;

    HashSet<Rigidbody> _trash = new();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trash = other.GetComponentInParent<Rigidbody>();

        if (_disableTrashGravity)
            trash.isKinematic = true;

        trash.MovePosition(trash.position);

        _trash.Add(trash);
    }

    public void CustomUpdate()
    {
        Debug.Log(name + " " + _trash.Count);
        foreach(Rigidbody trash in _trash)
        {
            trash.position += transform.forward * _trashMovementSpeed * Time.fixedDeltaTime * CONVEYOR_SPEED_MODIFIER;
            if (!trash.isKinematic) trash.angularVelocity = Vector3.zero;
        }
            
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Rigidbody trash = other.GetComponentInParent<Rigidbody>();

        trash.isKinematic = false;
        trash.linearVelocity += transform.forward * _trashFlingSpeed;

        _trash.Remove(trash);
    }
}
