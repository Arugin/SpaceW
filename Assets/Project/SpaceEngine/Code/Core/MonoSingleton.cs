﻿using System.Linq;

using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor such as: <code>T instance = new T();</code>
/// To prevent that, add protected parameterless constructor to your singleton class.
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static bool UnsaveSingleton = false;

    private static T instance;

    private static object @lock = new object();

    public static T Instance
    {
        get
        {
            if (ApplicationIsQuitting)
            {
                Debug.LogWarning(string.Format("Singleton: Instance '{0}' already destroyed on application quit. Won't create again - returning null.", typeof(T).Name));

                return null;
            }

            lock (@lock)
            {
                if (UnsaveSingleton) return instance;

                if (instance == null)
                {
                    var instances = FindObjectsOfType(typeof(T)).ToList();

                    instance = (T)instances.FirstOrDefault();

                    if (instances.Count > 1)
                    {
                        Debug.LogError(string.Format("Singleton: Something went really wrong - there should never be more than 1 singleton! Found count: {0}", instances.Count));

                        return instance;
                    }

                    if (instance == null)
                    {
                        var singleton = new GameObject();
                        instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).Name + "_(MonoSingleton)";

                        DontDestroyOnLoad(singleton);

                        Debug.Log(string.Format("Singleton: An instance of '{0}' is needed in the scene, so '{1}' was created with DontDestroyOnLoad.", typeof(T).Name, singleton.name));
                    }
                    else
                    {
                        Debug.Log(string.Format("Singleton: Using instance already created: {0}", instance.gameObject.name));
                    }
                }

                return instance;
            }
        }
        protected set { instance = value; }
    }

    public static bool ApplicationIsQuitting { get; private set; }

    protected virtual bool UnstableSingleton()
    {
        return false;
    }

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    /// it will create a buggy ghost object that will stay on the Editor scene even after stopping playing the Application.
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (UnstableSingleton()) ApplicationIsQuitting = true;
    }
}