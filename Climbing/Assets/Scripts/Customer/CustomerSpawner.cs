using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private Vector3 spawnRadius = new Vector3(5, 0, 5);

    private IObjectPool<GameObject> pool;
    private Coroutine spawnLoop;

    private void Awake()
    {
        // Guard against empty setup
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("CustomerSpawner: No customerPrefabs assigned.");
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
    }

    private void OnEnable()
    {
        spawnLoop ??= StartCoroutine(SpawnLoop(7, 15));
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
    }

    private void OnRelease(GameObject obj) => obj.SetActive(false);

    private void OnDestroyItem(GameObject obj) => Destroy(obj);

    private IEnumerator SpawnLoop(int minSeconds, int maxSeconds)
    {
        while (true)
        {
            // only spawn if we have room
            if (pool.CountInactive + ActiveCount() < CustomerManager.Instance.GetMaxNumCustomer())
            {
                var customer = pool.Get();

                // When your customer “leaves the cafe”, call:
                // pool.Release(customer);
            }

            yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));
        }
    }

    // Replace with your real active count if you track it
    private int ActiveCount() => 0;
}
