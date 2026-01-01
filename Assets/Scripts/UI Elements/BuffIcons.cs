using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuffIcons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FormerlySerializedAs("buffIcon")] public Image buffIconImage;
    public TextMeshProUGUI textMeshProUGUI;

    public string buffName;
    public int stacks;
    public Sprite buffIcon;
    public string buffDescription;
    
    public delegate void BuffIconHoveredHandler(string buffName, int stacks, Sprite buffIcon, bool hovered);
    public static event BuffIconHoveredHandler OnBuffIconHovered;

    public void SetText(string text)
    {
        textMeshProUGUI.text = text;
    }

    public void SetIcon(Sprite icon)
    {
        buffIconImage.sprite = icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, buffIcon, hovered: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnBuffIconHovered?.Invoke(buffName, stacks, buffIcon, hovered: false);
    }
}
