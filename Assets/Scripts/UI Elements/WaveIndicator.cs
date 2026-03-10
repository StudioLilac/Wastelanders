using TMPro;
using UnityEngine;

namespace UI_Elements
{
    public class WaveIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentWave;
        [SerializeField] private TextMeshProUGUI totalWaves;

        public void Show(int cur, int max)
        {
            currentWave.text = cur.ToString();
            totalWaves.text = max.ToString();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}