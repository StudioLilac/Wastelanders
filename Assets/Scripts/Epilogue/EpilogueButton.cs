using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class EpilogueButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image lockIndicator;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text requirementText;
    [SerializeField] private Image thumbnail;
    [SerializeField] private GameObject mask;
    private int completedBounties;
    private int neededBounties;
    private EpilogueSceneData sceneData;

    [SerializeField] private Outline outline;
    [SerializeField] private Vector2 defaultOutline;
    [SerializeField] private Vector2 hoverOutline;
    [SerializeField] private Color defaultOutlineColor = Color.white;
    [SerializeField] private Color hoverOutlineColor;
    [SerializeField] private Color defaultTextColor = Color.white;
    [SerializeField] private Color hoverTextColor;
    [SerializeField] private GameObject hover;

    public void Bind(EpilogueSceneData epilogueSceneData, EpilogueThumbnails thumbnails)
    {
        sceneData = epilogueSceneData;
        neededBounties = sceneData.BountyRequirement;
        thumbnail.sprite = epilogueSceneData.GetThumbnail(thumbnails);
        completedBounties = BountyManager.Instance.GetBountyProgress();
        button.onClick.AddListener(() =>
        {
            GameStateManager.Instance.LoadScene(sceneData.SceneData.SceneName);
        });
        SetLocked(neededBounties > completedBounties);
        UpdateRequirementText();
        SetHover(false);
    }

    public void SetHover(bool state) {
        if (!button.enabled || !button.interactable) return;
        hover.SetActive(state);
        outline.effectDistance = state ? hoverOutline : defaultOutline;
        outline.effectColor = state ? hoverOutlineColor : defaultOutlineColor;
        titleText.color = state ? hoverTextColor : defaultTextColor;
    }

    private void SetTitle(string text)
    {
        titleText.SetText($"{text}");
    }

    private void SetLocked(bool state)
    {
        lockIndicator.enabled = state;
        button.interactable = !state;
        mask.SetActive(!state);
        SetTitle(state ? "???" : sceneData.EpilogueTitle);
    }

    private void UpdateRequirementText()
    {
        float completed = Mathf.Min(completedBounties, neededBounties);
        requirementText.SetText($"{completed}/{neededBounties} bounties");
    }
}
