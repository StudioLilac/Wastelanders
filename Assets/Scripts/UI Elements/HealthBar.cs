using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform myRectTransform;
    public Slider slider;
    public TMP_Text healthText;
    public Canvas healthCanvas;

    private int FadeSortingOrder => CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER;
    private string FadeSortingLayer => CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;
    private float FadeZValue => CombatFadeScreenHandler.Instance.FADE_SCREEN_Z_VALUE;

    public void Start()
    {
        healthCanvas.sortingLayerName = FadeSortingLayer;
        Vector3 myPosition = myRectTransform.localPosition;
        myPosition.z = FadeZValue + 0.5f; // Default health bar seems to be offset by z by this amount
        myRectTransform.localPosition = myPosition;
        DeEmphasize();
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
