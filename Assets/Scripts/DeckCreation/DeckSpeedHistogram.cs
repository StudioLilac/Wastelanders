using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeckSpeedHistogram : MonoBehaviour
{
    [System.Serializable]
    public struct SpeedBar
    {
        public Image fillImage;
        public TMP_Text countText;
    }

    [Header("UI References")]
    [Tooltip("Bars for speeds 1 to 5. Index 0 corresponds to speed 1.")]
    [SerializeField] private SpeedBar[] speedBars; 
    [SerializeField] private TMP_Text cardCountText;
    [SerializeField] private int maxBarValue = 10;

    private void Start()
    {
        if (DeckSelectionManager.Instance != null)
        {
            DeckSelectionManager.Instance.PlayerActionDeckModifiedEvent += OnDeckModified;
            // Initialize with current deck
            UpdateHistogram();
        }
    }

    private void OnDestroy()
    {
        if (DeckSelectionManager.Instance != null)
        {
            DeckSelectionManager.Instance.PlayerActionDeckModifiedEvent -= OnDeckModified;
        }
    }

    private void OnDeckModified(int availablePoints)
    {
        UpdateHistogram();
    }

    public void UpdateHistogram()
    {
        if (DeckSelectionManager.Instance == null) return;

        List<ActionClass> deckCards = DeckSelectionManager.Instance.GetCurrentDeckCards();
        if (deckCards == null) return;

        int totalCards = deckCards.Count;
        if (cardCountText != null)
        {
            cardCountText.text = $"Cards: {totalCards}";
        }

        // We assume 5 speeds: 1, 2, 3, 4, 5
        int[] speedCounts = new int[5];

        foreach (var card in deckCards)
        {
            int speed = card.Speed;
            if (speed >= 1 && speed <= 5)
            {
                speedCounts[speed - 1]++;
            }
        }

        if (speedBars == null) return;

        for (int i = 0; i < speedBars.Length; i++)
        {
            if (i < 5)
            {
                float fillAmount = Mathf.Clamp01((float)speedCounts[i] / maxBarValue);
                if (speedBars[i].fillImage != null)
                {
                    speedBars[i].fillImage.fillAmount = fillAmount;
                }
                if (speedBars[i].countText != null)
                {
                    speedBars[i].countText.text = speedCounts[i].ToString();
                }
            }
        }
    }
}
