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

    public Trash SpawnEnemy(TrashData data, Vector2 position)
    {
        Trash trash = _trashPool.Get();
        trash.transform.position = position;

        trash.Initialize(data);

        return trash;
    }

    public void DespawnEnemy(Trash trash)
    {
        _trashPool.ReturnToPool(trash);
    }
}
