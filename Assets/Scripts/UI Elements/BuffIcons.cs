using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIcons : MonoBehaviour
{
    public Image buffIcon;
    public TextMeshProUGUI textMeshProUGUI;

    public string buffName;
    public int stacks;
    
    public delegate void BuffIconHoveredHandler(string buffName, int stacks, bool hovered);
    public static event BuffIconHoveredHandler OnBuffIconHovered;

    public void SetText(string text)
    {
        textMeshProUGUI.text = text;
    }

    public void SetIcon(Sprite icon)
    {
        buffIcon.sprite = icon;
    }

    // potential issue with several bufficons calling the same static event...
    public void OnMouseEnter()
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: true);
        Debug.Log("hovered");
    }

    public void OnMouseExit()
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: false);
        Debug.Log("unhovered");
    }
}
