using UnityEngine;
using UnityEditor;

public class AddWavesOnScene
{
    private const string prefabPath = "Assets/MyPrefabs/Wave_Sample.prefab";

    [MenuItem("Tools/Spawn Waves")]
    private static void SpawnWaveObjects()
    {
        //1. Get selected Object
        GameObject selected = Selection.activeGameObject;
        if (selected == null )
        {
            Debug.LogError("No GameObject selected. Please select a plane/mesh in the scene.");
            return;
        }

        // Use Renderer to get *world-space* bounds (includes scale!)
        Renderer renderer = selected.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Selected object has no Renderer. Select a plane/mesh with a Renderer.");
            return;
        }

        //2. Load wave prefab
        GameObject wavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (wavePrefab == null)
        {
            Debug.LogError("Could not find wave prefab at path: " + prefabPath);
            return;
        }

        Bounds bounds = renderer.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        // 3. Grid settings (spacing in LOCAL space)
        float spacingX = 0.5f;
        float spacingZ = 0.5f;

        float width = max.x - min.x;
        float depth = max.z - min.z;

        // How big an area counts as "occupied"
        float checkRadius = 0.24f;

        // Parent all waves under this object to keep Hierarchy clean
        GameObject parent = new GameObject("Waves_" + selected.name);
        Undo.RegisterCreatedObjectUndo(parent, "Create Wave Grid");

        Collider planeCollider = selected.GetComponent<Collider>(); // so we can ignore it

        // 4. Loop over grid points in LOCAL space, then convert to WORLD space
        for (float x = min.x; x <= max.x; x += spacingX)
        {
            float randomHeight = Random.Range(0.05f, 0.49f);
            for (float z = min.z; z <= max.z; z += spacingZ)
            {
                // Start roughly on the plane surface
                Vector3 worldPos = new Vector3(x, bounds.center.y + randomHeight, z);

                // Raycast down to conform exactly to plane collider
                // (Useful if it's not perfectly flat or if need exact surface contact.)
                Ray ray = new Ray(worldPos + Vector3.up * 10f, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f))
                {
                    worldPos = hitInfo.point;
                }

                //Check if space is already occupied
                bool blocked = false;
                Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
                foreach (var col in hits)
                {
                    // Ignore the plane itself
                    if (planeCollider != null && col == planeCollider)
                        continue;

                    // Anything else here blocks this position
                    blocked = true;
                    break;
                }

                if (blocked)
                {
                    // Skip spawning here
                    continue;
                }

                // Instantiate wave prefab as proper prefab instance
                GameObject waveInstance =
                    (GameObject)PrefabUtility.InstantiatePrefab(wavePrefab);

                waveInstance.transform.position = worldPos;
                waveInstance.transform.SetParent(parent.transform);

                Undo.RegisterCreatedObjectUndo(waveInstance, "Spawn Wave");
            }
        }

        Selection.activeGameObject = parent;

        Debug.Log("Spawned wave grid (" + spacingX + " x " + spacingZ +
                  ") on mesh: " + selected.name);
    }
}
