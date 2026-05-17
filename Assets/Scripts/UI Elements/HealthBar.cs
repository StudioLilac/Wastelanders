using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform myRectTransform;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Canvas healthCanvas;

    private int FadeSortingOrder => new GetFadeSortingOrder().Query() ?? 0;
    private string FadeSortingLayer => new GetFadeSortingLayer().Query() ?? string.Empty;
    private float FadeZValue => new GetFadeScreenZValue().Query() ?? 0f;

    public void Start()
    {
        healthCanvas.sortingLayerName = FadeSortingLayer;
        Vector3 myPosition = myRectTransform.localPosition;
        myPosition.z = FadeZValue + 0.5f; // Default health bar seems to be offset by z by this amount
        myRectTransform.localPosition = myPosition;
        DeEmphasize();
    }

    public void SetText(string str)
    {
        healthText.text = str;
    }

    public void setMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
    }

    public void setHealth(int health)
    {
        slider.value = health;
        healthText.text = health.ToString();
    }

    public void Emphasize()
    {
        healthCanvas.sortingOrder = FadeSortingOrder + 1;
    }
    public void DeEmphasize()
    {
        healthCanvas.sortingOrder = FadeSortingOrder - 5;
    }
}
