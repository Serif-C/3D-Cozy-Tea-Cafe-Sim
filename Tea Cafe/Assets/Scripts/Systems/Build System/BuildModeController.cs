using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

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
        [SerializeField] private BuildSaveAdapter saver;

        [Header("Ghost Materials")]
        [SerializeField] private Material ghostValidMaterial;
        [SerializeField] private Material ghostInvalidMaterial;

        [Header("Raycast Masks")]
        [SerializeField] private LayerMask placementSurfaceMask;   // floor / build plane
        [SerializeField] private LayerMask selectableMask = ~0;    // optionally limit what can be selected

        [Header("State")]
        [SerializeField] private bool buildModeEnabled = false;
        [SerializeField] private PlaceableItemConfig selectedItem;

        private PlaceableInstance selectedInst;     // currently selected placed object
        private bool dragging = false;
        private Vector3 dragStartPosition;
        private Quaternion dragStartRotation;

        private GameObject ghost;
        private bool placeTriggeredThisFrame;

        private TileEdge _wallEdge;
        private int _wallFaceFlip = 0; // 0 or 180

        private readonly Dictionary<Transform, int> cachedLayers = new Dictionary<Transform, int>(64);

        private void Awake()
        {
            if (buildCamera == null) buildCamera = Camera.main;
            if (grid == null) grid = FindFirstObjectByType<PlacementGrid>();
            if (registry == null) registry = FindFirstObjectByType<PlacementRegistry>();
            if (validator == null) validator = FindFirstObjectByType<PlacementValidator>();
            if (saver == null) saver = FindFirstObjectByType<BuildSaveAdapter>();
        }

        private void Start()
        {
            SetBuildMode(buildModeEnabled); // respect the serialized toggle
        }

        private void OnEnable()
        {
            if (pointAction != null) pointAction.action.Enable();

            Bind(placeAction, OnPlacePerformed);
            Bind(cancelAction, OnCancelPerformed);
            Bind(selectAction, OnSelectPerformed);
            Bind(deleteAction, OnDeletePerformed);
            Bind(rotateAction, OnRotatePerformed);
        }

        private void OnDisable()
        {
            Unbind(placeAction, OnPlacePerformed);
            Unbind(cancelAction, OnCancelPerformed);
            Unbind(selectAction, OnSelectPerformed);
            Unbind(deleteAction, OnDeletePerformed);
            Unbind(rotateAction, OnRotatePerformed);

            if (pointAction != null) pointAction.action.Disable();
        }

        private void Bind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
        {
            if (r == null) return;
            r.action.Enable();
            r.action.performed += cb;
        }

        private void Unbind(InputActionReference r, System.Action<InputAction.CallbackContext> cb)
        {
            if (r == null) return;
            r.action.performed -= cb;
            r.action.Disable();
        }

        private void Update()
        {
            if (!buildModeEnabled) return;

            // Dragging takes priority
            if (dragging && selectedInst != null)
            {
                //Vector3 snap = GetSnappedPointerWorldOnSurface();
                //selectedInst.transform.position = snap;

                int yaw;
                var cfg = selectedInst.GetConfig();
                Vector3 snap = GetSnappedPointerWorldOnSurface(cfg, out yaw);
                selectedInst.transform.position = snap;
                selectedInst.transform.rotation = Quaternion.Euler(0, yaw, 0);

                return;
            }

            UpdateGhostPositionAndValidity();

            if (placeTriggeredThisFrame)
            {
                placeTriggeredThisFrame = false;

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                TryPlace();
            }
        }

        private void OnPlacePerformed(InputAction.CallbackContext ctx)
        {
            // 1) If dragging, confirm the move
            if (dragging && selectedInst != null)
            {
                TryDropSelected();
                return;
            }

            // 2) If no ghost but we have an item selected, enter place mode now
            if (ghost == null && selectedItem != null)
            {
                EnterPlaceMode(selectedItem);
                return; // next click will place
            }

            // 3) Only place when we actually have an item/ghost
            if (selectedItem == null || ghost == null) return;

            placeTriggeredThisFrame = true;
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            if (dragging && selectedInst != null)
            {
                CancelDragAndRevert();
                return;
            }

            if (ghost != null)
            {
                EnterEditMode();  // just clear ghost, stay in build mode
                return;
            }

            SetBuildMode(false); // leave build mode entirely
        }

        private void OnSelectPerformed(InputAction.CallbackContext ctx)
        {
            if (!buildModeEnabled) return;

            // Defer UI check to Update if you kept that warning workaround, otherwise:
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // selecting cancels place mode
            if (ghost != null) EnterEditMode();

            var hit = RaycastSelectablePlaced();
            if (hit.inst == null) return;

            SelectInstance(hit.inst);
            BeginDragSelected();
        }

        private void OnRotatePerformed(InputAction.CallbackContext ctx)
        {
            // If we’re handling a WALL, rotate = flip face (180)
            bool isWallGhost = (ghost != null && selectedItem != null && selectedItem.Category == PlaceableCategory.Wall);
            bool isWallDragging = (selectedInst != null && dragging && selectedInst.GetConfig() != null && selectedInst.GetConfig().Category == PlaceableCategory.Wall);

            if (isWallGhost || isWallDragging)
            {
                _wallFaceFlip = (_wallFaceFlip == 0) ? 180 : 0;
                return; // rotation will be reapplied by snapping update
            }

            // Otherwise keep old behavior (+90)
            if (selectedInst != null && dragging)
            {
                selectedInst.transform.rotation = Quaternion.Euler(0, selectedInst.transform.eulerAngles.y + 90f, 0);
            }
            else if (ghost != null)
            {
                ghost.transform.rotation = Quaternion.Euler(0, ghost.transform.eulerAngles.y + 90f, 0);
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
            saver?.SaveNow();
            Deselect();
        }

        //private Vector3 GetSnappedPointerWorld()
        //{
        //    if (buildCamera == null) return Vector3.zero;

        //    Vector2 screenPos = pointAction != null
        //        ? pointAction.action.ReadValue<Vector2>()
        //        : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

        //    if (Physics.Raycast(buildCamera.ScreenPointToRay(screenPos), out var hit, 500f))
        //    {
        //        return grid != null ? grid.SnapToGrid(hit.point) : hit.point;
        //    }

        //    return Vector3.zero;
        //}

        // ---------- Mode management ----------
        private void SetBuildMode(bool enabled)
        {
            buildModeEnabled = enabled;

            if (!buildModeEnabled)
            {
                // leaving build mode: clean up
                dragging = false;
                selectedInst = null;
                DestroyGhost();
                cachedLayers.Clear();
                return;
            }
            EnterEditMode();
        }

        // Choose an item in the UI
        public void SelectItem(PlaceableItemConfig item)
        {
            selectedItem = item;
            if (buildModeEnabled) RebuildGhost();
        }

        private void EnterPlaceMode(PlaceableItemConfig cfg)
        {
            selectedItem = cfg;
            EnsureGhost();
        }
        private void EnterEditMode()
        {
            DestroyGhost();
        }

        // ---------- Ghost ----------

        private void EnsureGhost()
        {
            if (selectedItem == null || ghost != null) return;
            
            ghost = Instantiate(selectedItem.Prefab);
            CacheAndSetLayerRecursively(ghost.transform, LayerMask.NameToLayer("Ignore Raycast"));
            ApplyGhostMaterial(ghost, ghostValidMaterial);
        }

        private void RebuildGhost()
        {
            DestroyGhost();
            EnsureGhost();
        }

        private void DestroyGhost()
        {
            if (ghost == null) return;
            
            Destroy(ghost);
            ghost = null;
        }

        private void UpdateGhostPositionAndValidity()
        {
            if (ghost == null || buildCamera == null) return;

            //Vector3 snap = GetSnappedPointerWorldOnSurface();
            //ghost.transform.position = snap;

            int yaw;
            Vector3 snap = GetSnappedPointerWorldOnSurface(selectedItem, out yaw);
            ghost.transform.position = snap;
            ghost.transform.rotation = Quaternion.Euler(0, yaw, 0);

            bool ok = true;
            if (validator != null && selectedItem != null)
            {
                ok = validator.CanPlaceAt(selectedItem, snap, ghost.transform.rotation, ignore: null);
            }

            ApplyGhostMaterial(ghost, ok ? ghostValidMaterial : ghostInvalidMaterial);
        }

        private void ApplyGhostMaterial(GameObject root, Material mat)
        {
            if (mat == null) return;

            var rends = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++)
            {
                var rend = rends[i];
                var mats = rend.sharedMaterials;
                for (int m = 0; m < mats.Length; m++) mats[m] = mat;
                rend.sharedMaterials = mats;
            }
        }

        // ---------- Placement / Dragging ----------

        private void TryPlace()
        {
            if (ghost == null || selectedItem == null) return;

            int price = selectedItem.PriceCents;
            int current = PlayerManager.Instance.walletBalance;

            if (current < price)
            {
                Debug.Log("Not enough money");
                return;
            }

            Vector3 pos = ghost.transform.position;
            Quaternion rot = ghost.transform.rotation;

            if (validator != null)
            {
                if (!validator.CanPlaceAt(selectedItem, pos, rot, ignore: null))
                    return;
            }

            GameObject go = Instantiate(selectedItem.Prefab, pos, rot);

            var inst = go.GetComponent<PlaceableInstance>();
            if (inst == null) inst = go.AddComponent<PlaceableInstance>();
            inst.Init(selectedItem);

            registry?.Register(inst);

            PlayerManager.Instance.SetWalletBalance(current - price);

            saver?.SaveNow();

            EnterEditMode();
        }

        private void BeginDragSelected()
        {
            if (selectedInst == null) return;

            dragging = true;
            dragStartPosition = selectedInst.transform.position;
            dragStartRotation = selectedInst.transform.rotation;

            // Cache original layers and set Ignore Raycast
            cachedLayers.Clear();
            CacheAndSetLayerRecursively(selectedInst.transform, LayerMask.NameToLayer("Ignore Raycast"));
        }

        private void TryDropSelected()
        {
            if (selectedInst == null) return;

            var cfg = selectedInst.GetConfig();
            if (cfg != null && validator != null)
            {
                if (!validator.CanPlaceAt(cfg, selectedInst.transform.position, selectedInst.transform.rotation, ignore: selectedInst))
                {
                    CancelDragAndRevert();
                    return;
                }
            }

            dragging = false;
            RestoreCachedLayers();

            saver?.SaveNow();
        }

        private void CancelDragAndRevert()
        {
            if (selectedInst == null) return;

            selectedInst.transform.position = dragStartPosition;
            selectedInst.transform.rotation = dragStartRotation;

            dragging = false;
            RestoreCachedLayers();
        }

        private void Deselect()
        {
            selectedInst = null;
        }

        private void SelectInstance(PlaceableInstance inst)
        {
            selectedInst = inst;
        }

        // ---------- Raycasting ----------
        private Vector3 GetSnappedPointerWorldOnSurface()
        {
            if (buildCamera == null) return Vector3.zero;

            Vector2 screenPos = pointAction != null
                ? pointAction.action.ReadValue<Vector2>()
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            var ray = buildCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 500f, placementSurfaceMask, QueryTriggerInteraction.Ignore))
            {
                return grid != null ? grid.SnapToGrid(hit.point) : hit.point;
            }

            return Vector3.zero;
        }

        private Vector3 GetSnappedPointerWorldOnSurface(PlaceableItemConfig cfg, out int desiredYaw)
        {
            desiredYaw = 0;

            if (buildCamera == null) return Vector3.zero;

            Vector2 screenPos = pointAction != null
                ? pointAction.action.ReadValue<Vector2>()
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            Ray ray = buildCamera.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out var hit, 500f, placementSurfaceMask, QueryTriggerInteraction.Ignore))
                return Vector3.zero;

            if (grid == null) return hit.point;

            // WALL: strict tile-edge placement
            if (cfg != null && cfg.Category == PlaceableCategory.Wall)
            {
                Vector3 snap = grid.SnapWallToTileEdge(hit.point, out _wallEdge);
                int baseYaw = PlacementGrid.BaseYawForEdge(_wallEdge);

                desiredYaw = (baseYaw + _wallFaceFlip) % 360;
                return snap;
            }

            // NON-WALL: normal center snapping
            desiredYaw = Mathf.RoundToInt((ghost != null ? ghost.transform.eulerAngles.y : 0f) / 90f) * 90;
            return grid.SnapToCellCenter(hit.point);
        }

        private (PlaceableInstance inst, RaycastHit hit) RaycastSelectablePlaced()
        {
            if (buildCamera == null) return (null, default);

            Vector2 screenPos = pointAction != null
                ? pointAction.action.ReadValue<Vector2>()
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            var ray = buildCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 500f, selectableMask, QueryTriggerInteraction.Ignore))
            {
                var inst = hit.collider.GetComponentInParent<PlaceableInstance>();
                return (inst, hit);
            }

            return (null, default);
        }

        // ---------- Layer caching ----------

        private void CacheAndSetLayerRecursively(Transform root, int layer)
        {
            if (root == null) return;

            var stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (!cachedLayers.ContainsKey(t))
                    cachedLayers[t] = t.gameObject.layer;

                t.gameObject.layer = layer;

                for (int i = 0; i < t.childCount; i++)
                    stack.Push(t.GetChild(i));
            }
        }
        private void RestoreCachedLayers()
        {
            foreach (var kvp in cachedLayers)
            {
                if (kvp.Key != null)
                    kvp.Key.gameObject.layer = kvp.Value;
            }
            cachedLayers.Clear();
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

