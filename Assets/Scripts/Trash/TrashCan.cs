using System;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Trigger collider used to detect trash that fell inside of the bin")]
    [SerializeField] Collider _trashDetectionCollider;

    [Header("Core settings")]
    [Tooltip("Type of trash that is accepted by this can")]
    [SerializeField] TrashType _acceptedTrash;

    public event Action<Trash, int> OnTrashCollected;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Trash trash = other.GetComponent<Trash>();

        OnTrashCollected?.Invoke(trash, (trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score);

        Debug.Log($"Trash {trash.Name} collected. Score: {(trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score}");
    }
}
