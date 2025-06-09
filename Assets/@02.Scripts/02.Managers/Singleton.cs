using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T mInstance;

    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<T>();
                if (mInstance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    mInstance = obj.AddComponent<T>();
                }
            }
            
            return mInstance;
        }
    }
    
    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this as T;
            DontDestroyOnLoad(gameObject);
            
            // 씬 전환시 호출되는 액션 메서드 할당
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);
}