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

    [Header("Normal Spawn Times")]
    [SerializeField] private int minSpawnTime = 7;
    [SerializeField] private int maxSpawnTime = 15;

    [Header("Rush Hour Spawn Times")]
    [SerializeField] private int rushMinSpawnTime = 2;
    [SerializeField] private int rushMaxSpawnTime = 5;
    [SerializeField] private int rushStartTime = 12;
    [SerializeField] private int rushEndTime = 15;

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

        pool = new ObjectPool<GameObject>(
            createFunc: CreateCustomer,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true,
            defaultCapacity: CustomerManager.Instance.GetDefaultMax(),
            maxSize: CustomerManager.Instance.GetMaxNumCustomer()
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
                var customer = pool.Get();

            }

            //  Dynamic spawn delay based on rush hour
            int currentHour;
            if (timeManager != null) currentHour = timeManager.Hour;
            else currentHour = 0;

            bool isRushHour = currentHour >= rushStartTime && currentHour <= rushEndTime;

            int minDelay = isRushHour ? rushMinSpawnTime : rushMaxSpawnTime;
            int maxDelay = isRushHour ? rushMaxSpawnTime : rushMinSpawnTime;

            float waitSeconds = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(waitSeconds);
        }
    }

    private void ReleaseToPool(GameObject go)
    {
        // Optional: guard against releasing something already inactive
        if (go != null)
            pool.Release(go);
    }
}
