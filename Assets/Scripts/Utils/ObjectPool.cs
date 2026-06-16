using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MidnightReturn.Utils
{
    // Generic MonoBehaviour pool wrapping Unity's built-in ObjectPool
    public class MonoPool<T> where T : MonoBehaviour
    {
        private readonly ObjectPool<T> _pool;
        private readonly Transform _root;

        public int Active   => _pool.CountActive;
        public int Inactive => _pool.CountInactive;

        public MonoPool(T prefab, Transform root = null, int defaultCapacity = 16, int maxSize = 200)
        {
            _root = root;
            _pool = new ObjectPool<T>(
                createFunc:     () => UnityEngine.Object.Instantiate(prefab, root),
                actionOnGet:    obj => obj.gameObject.SetActive(true),
                actionOnRelease: obj => obj.gameObject.SetActive(false),
                actionOnDestroy: obj => UnityEngine.Object.Destroy(obj.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize:         maxSize
            );
        }

        public T Get()               => _pool.Get();
        public void Release(T obj)   => _pool.Release(obj);
        public void Clear()          => _pool.Clear();
    }
}
