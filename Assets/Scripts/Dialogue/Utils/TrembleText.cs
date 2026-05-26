using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextTrembleHelper : MonoBehaviour
{
    [Header("Tremble Settings")]
    [Tooltip("How far the letters move from their original position.")]
    [SerializeField] private float intensity = 2f;
    [Tooltip("How fast the letters shake.")]
    [SerializeField] private float speed = 20f;

    private TextMeshProUGUI _txtView;

    private void Awake()
    {
        _txtView = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _txtView.ForceMeshUpdate();
        var textInfo = _txtView.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            float noiseX = Mathf.PerlinNoise(i * 0.1f, Time.time * speed) - 0.5f;
            float noiseY = Mathf.PerlinNoise(i * 0.1f + 100f, Time.time * speed) - 0.5f; // offset Y seed

            Vector3 offset = new Vector3(noiseX, noiseY, 0) * intensity;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

            destinationVertices[vertexIndex + 0] += offset; // Bottom Left
            destinationVertices[vertexIndex + 1] += offset; // Top Left
            destinationVertices[vertexIndex + 2] += offset; // Top Right
            destinationVertices[vertexIndex + 3] += offset; // Bottom Right
        }

        for (int i = 0; i < textInfo.materialCount; i++)
        {
            if (textInfo.meshInfo[i].mesh != null)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _txtView.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}