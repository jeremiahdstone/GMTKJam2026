using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance { get; private set; }

    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 200;
    [Tooltip("A position outside the map for pooled objects to move to when released")]
    [SerializeField] private Vector2 pooledPosition = new Vector2(100f,100f);

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> prefabLookup = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public GameObject Spawn(GameObject prefab, Transform parent)
    {
        return Spawn(
            prefab,
            parent.position,
            prefab.transform.rotation,
            parent
        );
    }

    public GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolManager.Spawn was given a null prefab.");
            return null;
        }

        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
            pools[prefab] = pool;
        }

        GameObject obj = pool.Get();

        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);

        return obj;
    }

    public T Spawn<T>(
        T prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
        where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError(
                $"PoolManager.Spawn<{typeof(T).Name}> was given a null prefab."
            );

            return null;
        }

        GameObject obj = Spawn(
            prefab.gameObject,
            position,
            rotation,
            parent
        );

        return obj != null ? obj.GetComponent<T>() : null;
    }

    public void Release(GameObject obj)
    {
        if (obj == null)
            return;

        if (!prefabLookup.TryGetValue(obj, out GameObject prefab))
        {
            Debug.LogWarning(
                $"{obj.name} was not created by PoolManager. Destroying instead."
            );

            Destroy(obj);
            return;
        }

        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            Debug.LogWarning(
                $"Could not find pool for {obj.name}. Destroying instead."
            );

            prefabLookup.Remove(obj);
            Destroy(obj);
            return;
        }

        pool.Release(obj);
    }

    public void Release(GameObject obj, float delay)
    {
        if (obj == null)
            return;

        StartCoroutine(ReleaseAfterDelay(obj, delay));
    }

    public void Release(Component component)
    {
        if (component == null)
            return;

        Release(component.gameObject);
    }

    private IEnumerator ReleaseAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        Release(obj);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);

                prefabLookup[obj] = prefab;

                obj.SetActive(false);

                return obj;
            },

            actionOnGet: obj =>
            {
                obj.SetActive(true);
            },

            actionOnRelease: obj =>
            {
                obj.transform.SetParent(transform);
                obj.SetActive(false);
                obj.transform.position = pooledPosition;
            },

            actionOnDestroy: obj =>
            {
                prefabLookup.Remove(obj);
                Destroy(obj);
            },

            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
}