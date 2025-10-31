using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TeaShop.Systems.Building
{
    public class BuildModeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera buildCamera;
        [SerializeField] private PlacementGrid grid;
        [SerializeField] private PlacementRegistry registry;
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference placeAction;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private PlacementValidator validator;

        [Header("State")]
        [SerializeField] private bool buildModeEnabled = false;
        [SerializeField] private PlaceableItemConfig selectedItem;

        private GameObject ghost;
        private bool placeTriggeredThisFrame;

        private void OnEnable()
        {
            if (pointAction != null) pointAction.action.Enable();
            if (placeAction != null)
            {
                placeAction.action.Enable();
                placeAction.action.performed += OnPlacePerformed;
            }
            if (cancelAction != null)
            {
                cancelAction.action.Enable();
                cancelAction.action.performed += OnCancelPerformed;
            }
        }

        private void OnDisable()
        {
            if (placeAction != null)
            {
                placeAction.action.performed -= OnPlacePerformed;
                placeAction.action.Disable();
            }
            if (cancelAction != null)
            {
                cancelAction.action.performed -= OnCancelPerformed;
                cancelAction.action.Disable();
            }
            if (pointAction != null) pointAction.action.Disable();
        }

        private void OnPlacePerformed(InputAction.CallbackContext ctx)
        {
            placeTriggeredThisFrame = true;
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            SetBuildMode(false);
        }

        private void Awake()
        {
            if (buildCamera == null) buildCamera = Camera.main;
            if (grid == null) grid = FindFirstObjectByType<PlacementGrid>();
            if (registry == null) registry = FindFirstObjectByType<PlacementRegistry>();
            if (validator == null) validator = FindFirstObjectByType<PlacementValidator>();
        }

        private void Start()
        {
            SetBuildMode(true);
        }

        private void Update()
        {
            if (!buildModeEnabled) return;
            if (selectedItem == null) return;

            // 1) Update ghost from pointer position action
            UpdateGhostPositionFromAction();

            // 2) If "Place" was triggered this frame, try to place (after UI check)
            if (placeTriggeredThisFrame)
            {
                placeTriggeredThisFrame = false;

                // If pointer is over UI, ignore
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                TryPlace();
            }
        }

        private void SetBuildMode(bool enabled)
        {
            buildModeEnabled = enabled;

            if (enabled)
                RebuildGhost();
            else
                DestroyGhost();
        }

        public void SelectItem(PlaceableItemConfig item)
        {
            selectedItem = item;
            if (buildModeEnabled)
                RebuildGhost();
        }


        private void UpdateGhostPositionFromAction()
        {
            if (ghost == null) return;
            if (buildCamera == null) return;

            Vector2 screenPos = Vector2.zero;

            // Prefer the bound action value; fall back to current pointer if missing
            if (pointAction != null)
            {
                screenPos = pointAction.action.ReadValue<Vector2>();
            }
            else if (Pointer.current != null)
            {
                // Works for mouse or pen; new Input System API (no legacy Input)
                screenPos = Pointer.current.position.ReadValue();
            }

            Ray ray = buildCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                Vector3 snapped = grid != null ? grid.SnapToGrid(hit.point) : hit.point;
                ghost.transform.position = snapped;
            }
        }

        private void TryPlace()
        {
            if (ghost == null || selectedItem == null) return;

            Vector3 pos = ghost.transform.position;
            Quaternion rot = ghost.transform.rotation;

            if (validator != null)
            {
                bool ok = validator.CanPlaceAt(selectedItem, pos, rot);
                if (!ok) return; // silently fail for now (later show a red ghost)
            }

            GameObject go = Instantiate(selectedItem.Prefab, pos, rot);
            PlaceableInstance inst = go.GetComponent<PlaceableInstance>();
            if (inst == null) go.AddComponent<PlaceableInstance>();
            inst.Init(selectedItem);

            if (registry != null) registry.Register(inst);
        }

        private void RebuildGhost()
        {
            DestroyGhost();
            if (selectedItem == null) return;

            ghost = Instantiate(selectedItem.Prefab);
            SetLayerRecursively(ghost, LayerMask.NameToLayer("Ignore Raycast")); // Avoid self-hits
            ApplyGhostMaterial(ghost, 0.5f);
        }

        private void DestroyGhost()
        {
            if (ghost != null) Destroy(ghost);
            ghost = null;
        }

        private void ApplyGhostMaterial(GameObject root, float alpha)
        {
            Renderer[] rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                Material m = rends[i].material;
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }
        }

        private void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            for (int i = 0; i < root.transform.childCount; i++)
            {
                SetLayerRecursively(root.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}

