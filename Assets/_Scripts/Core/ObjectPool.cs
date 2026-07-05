using System.Collections.Generic;
using UnityEngine;

namespace Godus.Core
{
    /// <summary>
    /// Generic object pool. All frequently-spawned objects
    /// (enemies, projectiles, hit effects, pickups) MUST be pooled.
    /// Never raw Instantiate/Destroy in gameplay code.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();

        public ObjectPool(T prefab, Transform parent = null, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarmCount; i++)
            {
                var instance = CreateNew();
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        /// <summary>
        /// Get an instance from the pool, or create a new one if empty.
        /// </summary>
        public T Get()
        {
            if (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }

            return CreateNew();
        }

        /// <summary>
        /// Return an instance to the pool.
        /// </summary>
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        /// <summary>
        /// Return all active instances. Caller is responsible for tracking active objects.
        /// </summary>
        public void ReturnAll(IEnumerable<T> activeObjects)
        {
            foreach (var obj in activeObjects)
            {
                Return(obj);
            }
        }

        private T CreateNew()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.name = $"{_prefab.name}_pooled_{_pool.Count}";
            return instance;
        }
    }
}
