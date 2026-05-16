using UnityEngine;

namespace Project.Assets._Project._Scripts
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public abstract class SingletonEditMode<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }
            return _instance;
        }
    }

    protected virtual void OnValidate()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
#if UNITY_EDITOR
            // Immediately destroy duplicate in edit mode
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    DestroyImmediate(gameObject);
            };
#else
            Destroy(gameObject);
#endif
        }
    }
}

}
