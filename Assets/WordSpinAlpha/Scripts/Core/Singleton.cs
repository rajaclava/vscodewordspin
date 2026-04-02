using UnityEngine;

namespace WordSpinAlpha.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance => _instance;

        protected virtual bool PersistAcrossScenes => true;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            if (PersistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
