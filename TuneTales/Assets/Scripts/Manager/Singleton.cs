using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected bool WillBeDestroyed;
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            //if (!(this is DontDestroySingleton<T>))
            //    Debug.LogWarning("Destroying double Instances of '" + name + "'. Don't do this!");
            Destroy(gameObject);
            WillBeDestroyed = true;
            return;
        }
        Instance = this as T;
    }
    protected virtual void Start() { }
    protected virtual void Update() { }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
public abstract class DontDestroySingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
