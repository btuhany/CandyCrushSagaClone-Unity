using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class ObjectPool<T> : SingletonMonoBehaviour<ObjectPool<T>> where T : MonoBehaviour
    {
        [Header("Pool Config")]
        [SerializeField] protected T _prefab;
        [SerializeField] private int _size;
        [SerializeField] private int _expandingSize;
        private bool _isReady = false;

        private Queue<T> _pooledObjects;
        protected Transform _transform;
        protected override void Awake()
        {
            base.Awake();
            _transform = GetComponent<Transform>();
            PoolObjects();
        }
        private void ExpandPool(int size)
        {
            for (int i = 0; i < size; i++)
            {
                T newObj = Instantiate(_prefab, _transform);
                newObj.gameObject.SetActive(false);
                _pooledObjects.Enqueue(newObj);
            }
        }
        protected void PoolObjects()
        {
            _pooledObjects = new Queue<T>();
            for (int i = 0; i < _size; i++)
            {
                T newObj = Instantiate(_prefab, _transform);
                newObj.gameObject.SetActive(false);
                _pooledObjects.Enqueue(newObj);
            }
            _isReady = true;
        }
        public T GetObject(bool active = true)
        {
            if (!_isReady)
                PoolObjects();

            if (_pooledObjects.Count <= 0)
                ExpandPool(_expandingSize);

            T newObj = _pooledObjects.Dequeue();
            newObj.gameObject.SetActive(active);
            //newObj.transform.parent = null;
            return newObj;
        }
        public virtual void ReturnObject(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.gameObject.transform.SetParent(_transform);
            _pooledObjects.Enqueue(obj);
        }

    }

}
