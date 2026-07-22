using UnityEngine;
using UnityEngine.UI;

// This component provides a way to get a shared copied material instance for an Image,
// allowing multiple consumers to modify the same material instance without modifying the underlying asset.
public class ImageMaterialProvider : MonoBehaviour
{
    [SerializeField] private Image img;
    private Material _instanceMaterial;

    public Image Image => img;
    public Material GetSharedMaterial()
    {
        if (_instanceMaterial == null)
        {
            _instanceMaterial = new Material(img.material);
            img.material = _instanceMaterial;
        }

        return _instanceMaterial;
    }
}