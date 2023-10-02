using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    [Header("Singleton Config")]
    [SerializeField] private bool _dontDestroyOnLoad;
    private static T instance;
    protected virtual void Awake()
    {
        SingletonThisObject();
    }
    protected virtual void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }
    protected void SingletonThisObject()
    {
        if(instance == null)
        {
            instance = this as T;
            if(_dontDestroyOnLoad)
                DontDestroyOnLoad(this);
        }
        else
        {
            Debug.LogWarning("An instance of " + typeof (T) + " already in the scene. New instance has been deleted.");
            Destroy(this.gameObject);
        }
    }
    public static T Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("There is no instance of + " + typeof(T));

            return instance;
        }
    }

}
