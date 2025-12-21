using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffIcons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buffIcon;
    public TextMeshProUGUI textMeshProUGUI;

    public string buffName;
    public int stacks;
    
    public delegate void BuffIconHoveredHandler(string buffName, int stacks, bool hovered);
    public static event BuffIconHoveredHandler OnBuffIconHovered;

    private void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SetText(string text)
    {
        textMeshProUGUI.text = text;
    }

    public void SetIcon(Sprite icon)
    {
        buffIcon.sprite = icon;
    }

    // potential issue with several bufficons invoking the same static event...
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Pointer world pos: {eventData.pointerCurrentRaycast.worldPosition}");
        Debug.Log($"BuffIcon actual position: {transform.position}");
        Debug.Log($"BuffIcon RectTransform world corners:");
    
        RectTransform rt = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++)
        {
            Debug.Log($"  Corner {i}: {corners[i]}");
        }
    
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: false);
    }
}
