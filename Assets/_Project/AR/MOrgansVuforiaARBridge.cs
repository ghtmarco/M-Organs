using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Vuforia Ground Plane bridge for 05_AR.
/// Activates the correct organ model based on AppState.SelectedOrgan.
/// Delegates hit test to Vuforia PlaneFinderBehaviour and keeps the Vuforia
/// ContentPositioningBehaviour wired to the scene's Ground Plane Stage.
/// Wire in Inspector:
///   planeFinder → ARCamera/Plane Finder
///   groundPlaneStage → Ground Plane Stage
///   heartModel, brainModel, stomacModel → organ children of Ground Plane Stage
/// PLACE button OnClick → MOrgansVuforiaARBridge.OnPlacePressed
/// </summary>
[DefaultExecutionOrder(-50)]
public class MOrgansVuforiaARBridge : MonoBehaviour
{
    [Header("Vuforia scene objects")]
    [SerializeField] GameObject planeFinder;     // ARCamera/Plane Finder
    [SerializeField] Transform  groundPlaneStage;

    [Header("Organ models (children of Ground Plane Stage)")]
    [SerializeField] GameObject heartModel;
    [SerializeField] GameObject brainModel;
    [SerializeField] GameObject stomacModel;

    [Header("Fallback placement")]
    [SerializeField] float placementDistance = 1.2f;
    [SerializeField] float initialPlacementScale = 0.02f;
    [SerializeField] float dragSensitivity = 0.0016f;
    [SerializeField] float pinchSensitivity = 0.006f;
    [SerializeField] float minScale = 0.005f;
    [SerializeField] float maxScale = 0.12f;

    bool _placed;
    GameObject _activeOrgan;
    Vector3 _baseStageScale = Vector3.one;
    public bool IsPlaced => _placed;

    void Awake()
    {
        DisableObserverAutoVisibility();
    }

    void Start()
    {
        DisableObserverAutoVisibility();
        EnsureVuforiaWiring();
        if (groundPlaneStage != null) _baseStageScale = groundPlaneStage.localScale;
        ActivateOrgan(AppState.Instance?.SelectedOrgan ?? "jantung");
    }

    void Update()
    {
        if (!_placed || groundPlaneStage == null) return;
        HandleTouchManipulation();
        HandleMouseManipulation();
    }

    void ActivateOrgan(string key)
    {
        if (key != "jantung" && key != "otak" && key != "lambung")
            Debug.LogWarning($"[MOrgansVuforiaARBridge] Unknown organ key: '{key}', defaulting to jantung.");

        _activeOrgan = key switch
        {
            "otak" => brainModel,
            "lambung" => stomacModel,
            _ => heartModel
        };

        if (heartModel  != null) heartModel.SetActive(false);
        if (brainModel  != null) brainModel.SetActive(false);
        if (stomacModel != null) stomacModel.SetActive(false);
    }

    public void OnPlacePressed()
    {
        EnsureVuforiaWiring();
        PlaceInFrontOfCamera();

        if (planeFinder == null) return;

        var planeFinderBehaviour = FindComponent(planeFinder, "Vuforia.PlaneFinderBehaviour");
        if (planeFinderBehaviour == null)
        {
            Debug.LogWarning("MOrgansVuforiaARBridge: Vuforia.PlaneFinderBehaviour is missing.");
            return;
        }

        InvokePerformHitTest(planeFinderBehaviour);

        _placed = true;
    }

    public void ResetPlacement()
    {
        _placed = false;
        if (_activeOrgan != null) _activeOrgan.SetActive(false);
    }

    void PlaceInFrontOfCamera()
    {
        DisableObserverAutoVisibility();

        var arCamera = Camera.main;
        if (groundPlaneStage == null || arCamera == null) return;

        groundPlaneStage.gameObject.SetActive(true);
        groundPlaneStage.position = arCamera.transform.position + arCamera.transform.forward * placementDistance;
        groundPlaneStage.localScale = _baseStageScale * initialPlacementScale;
        Vector3 flatForward = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up);
        if (flatForward.sqrMagnitude > 0.001f)
            groundPlaneStage.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);

        if (_activeOrgan != null)
        {
            _activeOrgan.SetActive(true);
            SetVisibleComponents(_activeOrgan, true);
        }
        _placed = true;
    }

    void DisableObserverAutoVisibility()
    {
        if (groundPlaneStage == null) return;

        foreach (var behaviour in groundPlaneStage.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour != null && behaviour.GetType().Name == "DefaultObserverEventHandler")
                behaviour.enabled = false;
        }
    }

    static void SetVisibleComponents(GameObject root, bool visible)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = visible;

        foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            collider.enabled = visible;

        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            canvas.enabled = visible;
    }

    void EnsureVuforiaWiring()
    {
        if (planeFinder == null || groundPlaneStage == null) return;

        var contentPositioning = FindComponent(planeFinder, "Vuforia.ContentPositioningBehaviour");
        var anchorStage = FindComponent(groundPlaneStage.gameObject, "Vuforia.AnchorBehaviour");
        if (contentPositioning == null || anchorStage == null) return;

        var type = contentPositioning.GetType();
        var property = type.GetProperty("AnchorStage");
        if (property != null && property.CanWrite)
        {
            if (property.GetValue(contentPositioning) == null)
                property.SetValue(contentPositioning, anchorStage);
            return;
        }

        var field = type.GetField("AnchorStage");
        if (field != null && field.GetValue(contentPositioning) == null)
            field.SetValue(contentPositioning, anchorStage);
    }

    static Component FindComponent(GameObject owner, string fullTypeName)
    {
        if (owner == null) return null;
        foreach (var component in owner.GetComponents<Component>())
        {
            if (component != null && component.GetType().FullName == fullTypeName)
                return component;
        }
        return null;
    }

    static bool InvokePerformHitTest(Component planeFinderBehaviour)
    {
        try
        {
            var type = planeFinderBehaviour.GetType();
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            var methodWithPoint = type.GetMethod("PerformHitTest", new[] { typeof(Vector2) });
            if (methodWithPoint != null)
            {
                methodWithPoint.Invoke(planeFinderBehaviour, new object[] { screenCenter });
                return true;
            }

            var methodWithoutArgs = type.GetMethod("PerformHitTest", System.Type.EmptyTypes);
            if (methodWithoutArgs != null)
            {
                methodWithoutArgs.Invoke(planeFinderBehaviour, null);
                return true;
            }

            return false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MOrgansVuforiaARBridge] PerformHitTest invoke failed: {ex.Message}");
            return false;
        }
    }

    void HandleTouchManipulation()
    {
        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if (IsTouchOverUi(touch) || touch.phase != TouchPhase.Moved) return;
            DragStage(touch.deltaPosition);
            return;
        }

        if (Input.touchCount < 2) return;

        var a = Input.GetTouch(0);
        var b = Input.GetTouch(1);
        if (IsTouchOverUi(a) || IsTouchOverUi(b)) return;

        Vector2 prevA = a.position - a.deltaPosition;
        Vector2 prevB = b.position - b.deltaPosition;
        float previousDistance = Vector2.Distance(prevA, prevB);
        float currentDistance = Vector2.Distance(a.position, b.position);
        ScaleStage((currentDistance - previousDistance) * pinchSensitivity);

        Vector2 previousVector = prevB - prevA;
        Vector2 currentVector = b.position - a.position;
        float angle = Vector2.SignedAngle(previousVector, currentVector);
        groundPlaneStage.Rotate(Vector3.up, -angle, Space.World);
    }

    void HandleMouseManipulation()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButton(0))
        {
            DragStage(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 20f);
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f) ScaleStage(scroll * 0.08f);
    }

    void DragStage(Vector2 delta)
    {
        var arCamera = Camera.main;
        if (arCamera == null) return;

        Vector3 right = arCamera.transform.right;
        Vector3 up = arCamera.transform.up;
        groundPlaneStage.position += (right * delta.x + up * delta.y) * dragSensitivity;
    }

    void ScaleStage(float delta)
    {
        float current = groundPlaneStage.localScale.x / Mathf.Max(0.0001f, _baseStageScale.x);
        float next = Mathf.Clamp(current + delta, minScale, maxScale);
        groundPlaneStage.localScale = _baseStageScale * next;
    }

    static bool IsTouchOverUi(Touch touch)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }
}
