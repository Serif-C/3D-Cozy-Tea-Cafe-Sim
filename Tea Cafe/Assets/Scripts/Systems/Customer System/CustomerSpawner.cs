using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : MonoBehaviour
{
    [Header("References")]
    private TimeManager timeManager;
    [SerializeField] private GameObject[] customerPrefabs;
    
    [Header("Spawner Settings")]
    [SerializeField] private Vector3 spawnRadius = new Vector3(5, 0, 5);
    [Tooltip("X = hour of day (0–24), Y = seconds between spawns (lower = more customers during rush)")]
    [SerializeField] private AnimationCurve spawnDelayByHour;
    [Tooltip("Safety clamp so curve can't produce 0 or negative delays")]
    [SerializeField] private float minAllowedDelay = 0.5f;

    private IObjectPool<GameObject> pool;
    private Coroutine spawnLoop;

    private void Awake()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void Start()
    {
        // Guard against empty setup
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("CustomerSpawner: No customerPrefabs assigned");
            enabled = false;
            return;
        }

        int defaultCapacity = CustomerManager.Instance.GetDefaultMax();
        int hardCap = CustomerManager.Instance.GetHardCapMax();

        pool = new ObjectPool<GameObject>(
            createFunc: CreateCustomer,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: hardCap
        );

        spawnLoop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnLoop != null) StopCoroutine(spawnLoop);
        spawnLoop = null;
    }

    private GameObject CreateCustomer()
    {
        int random = Random.Range(0, customerPrefabs.Length); // max exclusive is fine
        var prefab = customerPrefabs[random];

        // Instantiate a new instance for the pool
        var go = Instantiate(prefab, parent: null);

        go.SetActive(false);
        return go;
    }

    private void OnGet(GameObject obj)
    {
        obj.SetActive(true);

        CustomerManager.Instance.IncrementCurrentCustomer();

        // random spawn position
        var pos = transform.position + new Vector3(
            Random.Range(-spawnRadius.x, spawnRadius.x),
            0f,
            Random.Range(-spawnRadius.z, spawnRadius.z)
        );
        obj.transform.SetPositionAndRotation(pos, Quaternion.identity);

        // Hand the customer a release callback
        var brain = obj.GetComponent<CustomerBrain>();
        if (brain != null)
            brain.Init(ReleaseToPool);
    }

    private void OnRelease(GameObject obj)
    {
        obj.SetActive(false);

        CustomerManager.Instance.DecrementCurrentCustomer();

        // Clear callback (optional hygiene)
        var brain = obj.GetComponent<CustomerBrain>();
        if (brain != null)
            brain.Init(null);
    }

    private void OnDestroyItem(GameObject obj) => Destroy(obj);

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int max = CustomerManager.Instance.GetMaxNumCustomer();
            int active = 0;

            if (pool is ObjectPool<GameObject> concretePool)
                active = concretePool.CountAll - concretePool.CountInactive;


            // only spawn if we have room
            if (active < max)
            {
                pool.Get();
            }

            //  Dynamic spawn delay based on rush hour
            float hourOfDay = 0f;
            if (timeManager != null)
            {
                // Hour is an int 0–23, Minute is 0–59
                hourOfDay = timeManager.Hour + timeManager.Minute / 60f;
            }

            float delay = spawnDelayByHour != null ? spawnDelayByHour.Evaluate(hourOfDay) : 5f;

            // Clamp to avoid 0 or negative delays
            if (delay < minAllowedDelay)
                delay = minAllowedDelay;

            yield return new WaitForSeconds(delay);
        }
    }

    private void ReleaseToPool(GameObject go)
    {
        // Optional: guard against releasing something already inactive
        if (go != null)
            pool.Release(go);
    }
}
