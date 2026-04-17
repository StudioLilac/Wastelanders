using UnityEngine;
using UnityEngine.UI;

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