using UnityEngine;

public class TrashFactory
{
    ObjectPool<Trash> _trashPool;

    public TrashFactory(Trash trashPrefab, int initialPoolSize = 20, Transform parentTransform = null)
    {
        GameObject poolParent = new GameObject(trashPrefab.name + "Pool");
        poolParent.transform.SetParent(parentTransform, false);

        _trashPool = new ObjectPool<Trash>(trashPrefab, initialPoolSize, poolParent.transform);
    }

    public Trash SpawnTrash(TrashData data, Vector3 position)
    {
        Trash trash = _trashPool.Get();
        trash.transform.position = position;
        Rigidbody trashRB = trash.GetComponent<Rigidbody>();
        trashRB.linearVelocity = Vector3.zero;
        trashRB.angularVelocity = Random.onUnitSphere;

        trash.Initialize(data);

        return trash;
    }

    public void DespawnTrash(Trash trash)
    {
        _trashPool.ReturnToPool(trash);
    }
}
