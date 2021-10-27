using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    /// <summary>
    /// Generic singleton type class to be derived 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        { 
            get 
            { 
                if(_instance==null)
                { 
                    _instance = FindObjectOfType<T>();
                if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                    return _instance;

            }
        }
        /// <summary>
        /// Make sure only one instance exists. 
        /// Existing instance wont get destroyed on scene loading
        /// </summary>
        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
