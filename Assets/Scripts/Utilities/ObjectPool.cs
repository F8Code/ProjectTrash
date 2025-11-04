using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    readonly Queue<T> _objects = new();
    readonly T _prefab;
    readonly Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _objects.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_objects.Count == 0)
        {
            T newObject = Object.Instantiate(_prefab, _parent);
            newObject.gameObject.SetActive(false);
            _objects.Enqueue(newObject);
        }

        T obj = _objects.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        if (_objects.Contains(obj))
            return;

        obj.gameObject.SetActive(false);
        _objects.Enqueue(obj);
    }
}