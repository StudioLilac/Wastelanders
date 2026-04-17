using UnityEngine;
using UnityEngine.UI;

public class ScreenCutoutScrim : MonoBehaviour
{
    [SerializeField] private ImageMaterialProvider provider;
    private Material _scrimMaterial;
    private readonly int _cutoutPropertyId = Shader.PropertyToID("_CutoutRect");
    private Camera _mainCamera;

    [Header("Current Target Context")]
    private RectTransform _targetUI;
    private SpriteRenderer _targetSprite;
    private Transform _targetWorldPoint;
    private Vector2 _worldBoxSizePixels;

    private void Awake()
    {
        _scrimMaterial = provider.GetSharedMaterial();
        _mainCamera = Camera.main;
    }

    public void SetBlocking(bool blocking)
    {
        provider.Image.raycastTarget = blocking;
    }

    public void SetTargetUI(RectTransform uiElement)
    {
        ClearTargets();
        _targetUI = uiElement;
    }

    public void SetTargetSprite(SpriteRenderer sprite)
    {
        ClearTargets();
        _targetSprite = sprite;
    }

    public void SetTargetWorldPoint(Transform worldObject, Vector2 boxSizePixels)
    {
        ClearTargets();
        _targetWorldPoint = worldObject;
        _worldBoxSizePixels = boxSizePixels;
    }

    public void ClearTargets()
    {
        _targetUI = null;
        _targetSprite = null;
        _targetWorldPoint = null;
        _scrimMaterial.SetVector(_cutoutPropertyId, Vector4.zero);
    }

    private void LateUpdate()
    {
        if (_targetUI != null)
        {
            UpdateCutoutForUI();
        }
        else if (_targetSprite != null)
        {
            UpdateCutoutForSprite();
        }
        else if (_targetWorldPoint != null)
        {
            UpdateCutoutForWorldPoint();
        }
    }
    

    private void UpdateCutoutForUI()
    {
        // GetWorldCorners returns the 4 corners of the UI element.
        // Index 0 is Bottom-Left, Index 2 is Top-Right.
        // For 'Screen Space - Overlay' canvases, these are exact screen pixel coordinates.
        Vector3[] corners = new Vector3[4];
        _targetUI.GetWorldCorners(corners);

        ApplyCutoutToShader(
            minXPixels: corners[0].x,
            minYPixels: corners[0].y,
            maxXPixels: corners[2].x,
            maxYPixels: corners[2].y
        );
    }

    private void UpdateCutoutForSprite()
    {
        if (_mainCamera == null) return;
         
        Bounds bounds = _targetSprite.bounds;
         
        Vector3 screenMin = _mainCamera.WorldToScreenPoint(bounds.min);
        Vector3 screenMax = _mainCamera.WorldToScreenPoint(bounds.max);

        ApplyCutoutToShader(screenMin.x, screenMin.y, screenMax.x, screenMax.y);
    }

    private void UpdateCutoutForWorldPoint()
    {
        if (_mainCamera == null) return;
         
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetWorldPoint.position);

        ApplyCutoutToShader(
            minXPixels: screenPos.x - (_worldBoxSizePixels.x / 2f),
            minYPixels: screenPos.y - (_worldBoxSizePixels.y / 2f),
            maxXPixels: screenPos.x + (_worldBoxSizePixels.x / 2f),
            maxYPixels: screenPos.y + (_worldBoxSizePixels.y / 2f)
        );
    }

    /// Normalizes the pixel coordinates to 0.0-1.0 Viewport space and sends to shader.
    private void ApplyCutoutToShader(float minXPixels, float minYPixels, float maxXPixels, float maxYPixels)
    {
        float padding = 0f;

        float minX = (minXPixels - padding) / Screen.width;
        float minY = (minYPixels - padding) / Screen.height;
        float maxX = (maxXPixels + padding) / Screen.width;
        float maxY = (maxYPixels + padding) / Screen.height;

        _scrimMaterial.SetVector(_cutoutPropertyId, new Vector4(minX, minY, maxX, maxY));
    }
}