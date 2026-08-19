using UnityEngine;
using UnityEngine.UI;

public interface IScrimTarget
{
    bool IsValid { get; }
    Rect GetScreenRect(Camera mainCamera);
}

public class ScreenCutoutScrim : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private ImageMaterialProvider provider;
    private Material _scrimMaterial;
    private readonly int _cutoutPropertyId = Shader.PropertyToID("_CutoutRect");
    private Camera _mainCamera;

    private IScrimTarget _currentTarget;
    private Rect _currentHoleScreenRect;

    private void Awake()
    {
        _scrimMaterial = provider.GetSharedMaterial();
        _mainCamera = Camera.main;
    }

    public void SetBlocking(bool blocking)
    {
        provider.Image.raycastTarget = blocking;
    }

    public void SetTarget(IScrimTarget target)
    {
        _currentTarget = target;
    }

    public void ClearTarget()
    {
        _currentTarget = null;
        _currentHoleScreenRect = Rect.zero;
        _scrimMaterial.SetVector(_cutoutPropertyId, Vector4.zero);
    }

    private void LateUpdate()
    {
        if (_currentTarget != null && _currentTarget.IsValid)
        {
            _currentHoleScreenRect = _currentTarget.GetScreenRect(_mainCamera);
            ApplyCutoutToShader(_currentHoleScreenRect);
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (_currentTarget == null) return true;
        return !_currentHoleScreenRect.Contains(screenPoint);
    }

    private void ApplyCutoutToShader(Rect rect)
    {
        float minX = rect.xMin / Screen.width;
        float minY = rect.yMin / Screen.height;
        float maxX = rect.xMax / Screen.width;
        float maxY = rect.yMax / Screen.height;

        _scrimMaterial.SetVector(_cutoutPropertyId, new Vector4(minX, minY, maxX, maxY));
    }
}

public class UITarget : IScrimTarget
{
    private readonly RectTransform _rectTransform;
    private readonly Camera _uiCamera; 
    private readonly float _paddingPercent;

    public UITarget(RectTransform rectTransform, Canvas canvas, float paddingPercent = 0f)
    {
        _rectTransform = rectTransform;
        _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        _paddingPercent = paddingPercent;
    }

    public bool IsValid => _rectTransform != null;

    public Rect GetScreenRect(Camera mainCamera)
    {
        Vector3[] worldCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(worldCorners);

        Vector2 bl = RectTransformUtility.WorldToScreenPoint(_uiCamera, worldCorners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(_uiCamera, worldCorners[2]);

        float baseWidth = tr.x - bl.x;
        float baseHeight = tr.y - bl.y;
        float padX = baseWidth * _paddingPercent;
        float padY = baseHeight * _paddingPercent;

        return new Rect(
            bl.x - padX,
            bl.y - padY,
            baseWidth + (padX * 2),
            baseHeight + (padY * 2)
        );
    }
}

public record SpriteTarget(SpriteRenderer SpriteRenderer) : IScrimTarget
{
    public bool IsValid => SpriteRenderer != null;

    public Rect GetScreenRect(Camera mainCamera)
    {
        Bounds bounds = SpriteRenderer.bounds;
        Vector3 screenMin = mainCamera.WorldToScreenPoint(bounds.min);
        Vector3 screenMax = mainCamera.WorldToScreenPoint(bounds.max);

        return new Rect(screenMin.x, screenMin.y, screenMax.x - screenMin.x, screenMax.y - screenMin.y);
    }
}

public record WorldPointTarget(Transform Transform, Vector2 BoxSizePixels) : IScrimTarget
{
    public bool IsValid => Transform != null;

    public Rect GetScreenRect(Camera mainCamera)
    {
        Vector3 center = mainCamera.WorldToScreenPoint(Transform.position);
        float halfX = BoxSizePixels.x / 2f;
        float halfY = BoxSizePixels.y / 2f;

        return new Rect(center.x - halfX, center.y - halfY, BoxSizePixels.x, BoxSizePixels.y);
    }
}