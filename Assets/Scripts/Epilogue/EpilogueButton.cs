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
    private int completedBounties;
    private int neededBounties;
    private EpilogueSceneData sceneData;


    public void Bind(EpilogueSceneData epilogueSceneData, EpilogueThumbnails thumbnails)
    {
        sceneData = epilogueSceneData;
        neededBounties = sceneData.BountyRequirement;
        thumbnail.sprite = epilogueSceneData.GetThumbnail(thumbnails);
        completedBounties = PrincessFrogBounties.Values.Count(b => BountyManager.Instance.IsBountyCompleted(b));
        button.onClick.AddListener(() =>
        {
            GameStateManager.Instance.LoadScene(sceneData.SceneData.SceneName);
        });
        SetLocked(neededBounties > completedBounties);
        UpdateRequirementText();
    }

    private void SetTitle(string text)
    {
        titleText.SetText($"{text}");
    }

    private void SetLocked(bool state)
    {
        lockIndicator.enabled = state;
        button.interactable = !state;
        SetTitle(state ? "???" : sceneData.EpilogueTitle);
    }

    private void UpdateRequirementText()
    {
        requirementText.SetText($"{completedBounties}/{neededBounties} bounties");
    }
}
