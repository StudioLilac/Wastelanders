using UnityEngine;
using UnityEngine.UI;

public class MaterialTintFadeHandler : FadeHandlerBase
{
    [SerializeField] private ImageMaterialProvider provider;
    private Material _scrimMaterial;

    private readonly int _colorPropertyId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _scrimMaterial = provider.GetSharedMaterial();
    }

    protected override float CurrentAlpha => _scrimMaterial.GetColor(_colorPropertyId).a;

    protected override void SetAlpha(float alpha)
    {
        if (_scrimMaterial != null)
        {
            Color c = _scrimMaterial.GetColor(_colorPropertyId);
            c.a = alpha;
            _scrimMaterial.SetColor(_colorPropertyId, c);
        }
    }
}