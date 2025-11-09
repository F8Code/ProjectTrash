using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("Core settings")]
    [Tooltip("Type of trash that is accepted by this can")]
    [SerializeField] TrashType _acceptedTrash;
    HashSet<Trash> _ignoredTrash = new();

    public event Action<Trash, int> OnTrashCollected;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != GameConstants.Layer.Trash)
            return;

        Trash trash = other.GetComponentInParent<Trash>();

        if (_ignoredTrash.Contains(trash)) return;

        _ignoredTrash.Add(trash);
        OnTrashCollected?.Invoke(trash, (trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score);

        Debug.Log($"Trash {trash.Name} collected. Score: {(trash.Type == _acceptedTrash ? 1 : -1) * (int)trash.Score}");
    }

    public void StopIgnoringTrash(Trash trash) => _ignoredTrash.Remove(trash);
}
