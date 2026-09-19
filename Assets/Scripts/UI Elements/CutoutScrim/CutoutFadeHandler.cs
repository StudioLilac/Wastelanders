using UnityEngine;

public class CutoutFadeHandler : FadeHandlerBase
{
    [SerializeField] private ImageMaterialProvider provider;
    private Material _scrimMaterial;
    private readonly int _holeAlphaPropertyId = Shader.PropertyToID("_HoleAlpha");

    private void Awake()
    {
        _scrimMaterial = provider.GetSharedMaterial();
    }

    protected override float CurrentAlpha => _scrimMaterial.GetFloat(_holeAlphaPropertyId);
    protected override void SetAlpha(float alpha) => _scrimMaterial.SetFloat(_holeAlphaPropertyId, alpha);
}