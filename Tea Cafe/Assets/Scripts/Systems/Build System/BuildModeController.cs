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
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference deleteAction;
        [SerializeField] private InputActionReference rotateAction;
        [SerializeField] private PlacementValidator validator;

        [Header("State")]
        [SerializeField] private bool buildModeEnabled = false;
        [SerializeField] private PlaceableItemConfig selectedItem;
        private PlaceableInstance selectedInst;     // currently selected placed object
        private bool dragging = false;
        private Vector3 dragStartPosition;
        private Quaternion dragStartRotation;

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
            if (selectAction != null)
            {
                selectAction.action.Enable();
                selectAction.action.performed += OnSelectPerformed;
            }
            if (deleteAction != null)
            {
                deleteAction.action.Enable();
                deleteAction.action.performed += OnDeletePerformed;
            }
            if (rotateAction != null)
            {
                rotateAction.action.Enable();
                rotateAction.action.performed += OnRotatePerformed;
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

            if (selectAction != null)
            {
                selectAction.action.performed -= OnSelectPerformed;
                selectAction.action.Disable();
            }
            if (deleteAction != null)
            {
                deleteAction.action.performed -= OnDeletePerformed;
                deleteAction.action.Disable();
            }
            if (rotateAction != null)
            {
                rotateAction.action.performed -= OnRotatePerformed;
                rotateAction.action.Disable();
            }
        }

        private void OnPlacePerformed(InputAction.CallbackContext ctx)
        {
            // If dragging a selected instance, drop it here (validate + save)
            if (dragging && selectedInst != null)
            {
                TryDropSelected();
                return;
            }

            placeTriggeredThisFrame = true; // placing from ghost (existing flow)
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            SetBuildMode(false);
        }

        private void OnSelectPerformed(InputAction.CallbackContext ctx)
        {
            // ignore UI clicks
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // If we're placing a new item, selection will place it (existing flow)
            if (selectedItem != null && ghost != null)
                return;

            // Try pick an existing placed object
            var hit = RaycastPlaced();
            if (hit.inst != null)
            {
                SelectInstance(hit.inst);

                // Begin dragging immediately
                dragging = true;
                dragStartPosition = selectedInst.transform.position;
                dragStartRotation = selectedInst.transform.rotation;

                // make the selected ignore raycast while dragging so we don't hit itself
                SetLayerRecursively(selectedInst.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
            }
        }

        private void OnRotatePerformed(InputAction.CallbackContext ctx)
        {
            if (selectedInst != null && dragging)
            {
                selectedInst.transform.rotation = Quaternion.Euler(0, selectedInst.transform.eulerAngles.y + 90f, 0);
            }
        }

        private void OnDeletePerformed(InputAction.CallbackContext ctx)
        {
            if (selectedInst == null) return;

            // if dragging, cancel drag first
            if (dragging)
            {
                CancelDragAndRevert();
                return;
            }

            // delete selected item
            registry.UnRegister(selectedInst); // fires Removed event (seating bridge etc.)
            Destroy(selectedInst.gameObject);

            // persist
            var saver = FindFirstObjectByType<BuildSaveAdapter>();
            if (saver != null) saver.SaveNow();

            Deselect();
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

            // --- DRAGGING MOVE ------------------------------------------------------
            if (dragging && selectedInst != null)
            {
                // Follow pointer with grid snap
                Vector3 snap = GetSnappedPointerWorld();
                selectedInst.transform.position = snap;
                return; // While dragging ignore placement ghost
            }
            // -----------------------------------------------------------------------

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

        private Vector3 GetSnappedPointerWorld()
        {
            if (buildCamera == null) return Vector3.zero;

            Vector2 screenPos = pointAction != null
                ? pointAction.action.ReadValue<Vector2>()
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            if (Physics.Raycast(buildCamera.ScreenPointToRay(screenPos), out var hit, 500f))
            {
                return grid != null ? grid.SnapToGrid(hit.point) : hit.point;
            }

            return Vector3.zero;
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

            int price = selectedItem.PriceCents;
            int current = PlayerManager.Instance.walletBalance;

            if (current < price)
            {
                // TO:DO: UI feedback "Not enough money"
                Debug.Log("Not enough money");
                return;
            }

            Vector3 pos = ghost.transform.position;
            Quaternion rot = ghost.transform.rotation;

            if (validator != null)
            {
                bool ok = validator.CanPlaceAt(selectedItem, pos, rot);
                if (!ok) return; // silently fail for now (later show a red ghost)
            }

            GameObject go = Instantiate(selectedItem.Prefab, pos, rot);
            PlaceableInstance inst = go.GetComponent<PlaceableInstance>();
            if (inst == null) inst = go.AddComponent<PlaceableInstance>(); //  assign it!
            inst.Init(selectedItem);

            if (registry != null) registry.Register(inst);                 //  now non-null

            // Spend wallet balance
            int newBalance = PlayerManager.Instance.walletBalance - price;
            PlayerManager.Instance.SetWalletBalance(newBalance);

            // persist immediately
            // (can also be done via BuildSaveAdapter OnEnable method which I am doing)
            var saver = FindFirstObjectByType<BuildSaveAdapter>();
            if (saver != null) saver.SaveNow();
        }

        private void TryDropSelected()
        {
            if (selectedInst == null) return;

            Vector3 pos = selectedInst.transform.position;
            Quaternion rot = selectedInst.transform.rotation;

            // validate new spot using the same PlacementValidator
            if (validator != null && selectedInst.GetConfig() != null)
            {
                if (!validator.CanPlaceAt(selectedInst.GetConfig(), pos, rot))
                {
                    // invalid -> revert
                    CancelDragAndRevert();
                    return;
                }
            }

            // commit: restore layer, stop dragging
            dragging = false;
            SetLayerRecursively(selectedInst.gameObject, 0); // Default (or store previous if needed)

            // persist
            var saver = FindFirstObjectByType<BuildSaveAdapter>();
            if (saver != null) saver.SaveNow();
        }

        private void CancelDragAndRevert()
        {
            if (selectedInst == null) return;

            selectedInst.transform.position = dragStartPosition;
            selectedInst.transform.rotation = dragStartRotation;
            dragging = false;
            SetLayerRecursively(selectedInst.gameObject, 0);
        }

        private void Deselect()
        {
            // (optional) remove highlight if you add one
            selectedInst = null;
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

        private (PlaceableInstance inst, RaycastHit hit) RaycastPlaced()
        {
            if (buildCamera == null) return (null, default);
            Vector2 screenPos = pointAction != null
                ? pointAction.action.ReadValue<Vector2>()
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            if (Physics.Raycast(buildCamera.ScreenPointToRay(screenPos), out var hit, 500f))
            {
                var inst = hit.collider.GetComponentInParent<PlaceableInstance>();
                return (inst, hit);
            }
            return (null, default);
        }

        private void SelectInstance(PlaceableInstance inst)
        {
            selectedInst = inst;
            // Optional: highlight visuals (outline/alpha). You already have ApplyGhostMaterial;
            // you could make a lightweight "selected" highlight if you want.
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

