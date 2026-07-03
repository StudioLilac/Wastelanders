using UnityEngine;

/**
 * Credit to adammyhre on github for this implementation of persistent singleton 
 */
public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    [Tooltip("if this is true, this singleton will auto detach if it finds itself parented on awake")]
    public bool UnparentOnAwake = true;

    public static bool HasInstance => instance != null;
    public static T Current => instance;

    protected static T instance;
    protected bool invalid = false;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
            }

            if (instance == null)
            {
                throw new System.InvalidOperationException(
                    $"{typeof(T).Name} has not been initialized. PersistentSingletons must be provided by " +
                    $"SceneInitializer (add it to SceneInitializerPrefabs and AlwaysPresentPrefabs/ScenePrefabs) " +
                    $"rather than being auto-created on first access.");
            }

            return instance;
        }
    }

    protected virtual void Awake() => InitializeSingleton();

    protected virtual void InitializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (UnparentOnAwake)
        {
            transform.SetParent(null);
        }

        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(transform.gameObject);
            enabled = true;
        }
        else
        {
            if (this != instance)
            {
                Destroy(this.gameObject);
                invalid = true;
            }
        }
    }
}