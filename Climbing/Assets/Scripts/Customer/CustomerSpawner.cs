using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private Vector3 spawnRadius = new Vector3(5, 0, 5);
    [SerializeField] private int minSpawnTime = 7;
    [SerializeField] private int maxSpawnTime = 15;

    private IObjectPool<GameObject> pool;
    private Coroutine spawnLoop;

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

        spawnLoop = StartCoroutine(SpawnLoop(minSpawnTime, maxSpawnTime)); // start AFTER pool exists
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
        var go = Instantiate(prefab);
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

    private IEnumerator SpawnLoop(int minSeconds, int maxSeconds)
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

            yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));
        }
    }

    private void ReleaseToPool(GameObject go)
    {
        // Optional: guard against releasing something already inactive
        if (go != null)
            pool.Release(go);
    }
}
