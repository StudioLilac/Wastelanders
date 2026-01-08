using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuffIcons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FormerlySerializedAs("buffIcon")] public Image buffIconImage;
    public TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private RectTransform rectTransform;

    public string buffName;
    public int stacks;
    public Sprite buffIcon;
    public string buffDescription;
    private bool isHovering = false;
    private Camera cachedCam;
    
    public delegate void BuffIconHoveredHandler(string buffName, int stacks, Sprite buffIcon, bool hovered);
    public static event BuffIconHoveredHandler OnBuffIconHovered;

    void Awake()
    {
        cachedCam = Camera.main;
    }

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
        isHovering = true;
        OnBuffIconHovered?.Invoke(buffName, stacks, buffIcon, hovered: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ForceExit();
    }


    private void Update()
    {
        if (isHovering)
        {
            bool isMouseInside = RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                Input.mousePosition,
                cachedCam
            );

            if (!isMouseInside)
            {
                ForceExit();
            }
        }
    }

    private void ForceExit()
    {
        if (!isHovering) return;

        isHovering = false;
        OnBuffIconHovered?.Invoke(buffName, stacks, buffIcon, hovered: false);
    }
}
