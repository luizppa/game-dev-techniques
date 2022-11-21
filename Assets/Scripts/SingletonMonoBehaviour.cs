using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
  private static T instance = null;

  protected virtual void Awake()
  {
    if (FindObjectsOfType<T>().Length > 1)
    {
      Destroy(gameObject);
    }
    else
    {
      DontDestroyOnLoad(gameObject);
      instance = this as T;
    }
  }

  public static T Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindObjectOfType<T>();
        if (instance == null)
        {
          GameObject singleton = new GameObject();
          singleton.name = typeof(T).Name;
          instance = singleton.AddComponent<T>();
        }
      }
      return instance;
    }
  }
}
