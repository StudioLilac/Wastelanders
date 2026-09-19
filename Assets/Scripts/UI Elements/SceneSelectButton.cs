using UnityEngine;
using TMPro;
using UnityEngine.UI;

public interface ISelectableSceneData
{
    string Title { get; }
    bool IsLocked { get; }
    string RequirementText { get; }
    Sprite Thumbnail { get; }
    void OnClick();
}

public class SceneSelectButton : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected Image lockIndicator;
    [SerializeField] protected TMP_Text titleText;
    [SerializeField] protected TMP_Text requirementText;
    [SerializeField] protected Image thumbnail;
    [SerializeField] protected GameObject mask;

    [SerializeField] protected Outline outline;
    [SerializeField] protected Vector2 defaultOutline;
    [SerializeField] protected Vector2 hoverOutline;
    [SerializeField] protected Color defaultOutlineColor = Color.white;
    [SerializeField] protected Color hoverOutlineColor;
    [SerializeField] protected Color defaultTextColor = Color.white;
    [SerializeField] protected Color hoverTextColor;
    [SerializeField] protected GameObject hover;

    public void Bind(ISelectableSceneData data)
    {
        thumbnail.sprite = data.Thumbnail;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            data.OnClick();
        });
        
        SetLocked(data.IsLocked, data.Title);
        requirementText.SetText(data.RequirementText);
        SetHover(false);
    }

    public void SetHover(bool state)
    {
        if (!button.enabled || !button.interactable) return;
        hover.SetActive(state);
        outline.effectDistance = state ? hoverOutline : defaultOutline;
        outline.effectColor = state ? hoverOutlineColor : defaultOutlineColor;
        titleText.color = state ? hoverTextColor : defaultTextColor;
    }

    protected void SetTitle(string text)
    {
        titleText.SetText($"{text}");
    }

    protected void SetLocked(bool state, string title)
    {
        lockIndicator.enabled = state;
        button.interactable = !state;
        mask.SetActive(!state);
        SetTitle(state ? "???" : title);
    }
}
