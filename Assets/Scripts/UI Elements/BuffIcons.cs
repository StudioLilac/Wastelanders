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
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, hovered: false);
    }
}
