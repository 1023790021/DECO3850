using System;
using System.Collections.Concurrent;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
    private static UnityMainThreadDispatcher instance = null;

    // Called when the script instance is being loaded.
    void Awake()
    {
        // Ensure that the instance is assigned during the main thread.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Only if you want this object to persist across scene loads.
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Optional: Destroy any duplicate objects that might be created.
        }
    }
    
    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UnityMainThreadDispatcher>() ?? new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
        }
        return instance;
    }

    public void Enqueue(Action action)
    {
        queue.Enqueue(action);
    }

    void Update()
    {
        while (queue.TryDequeue(out var action))
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while executing an action on the main thread: " + ex);
            }
        }
    }
}
