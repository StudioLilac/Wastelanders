using BountySystem;
using LevelSelectInformation;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Class that handles the rendering and scene management of bounty selection
public class BountySelect : MonoBehaviour
{
    [SerializeField] private Transform[] bountyGrid;
    [SerializeField] private BountyButton bountyButtonPrefab;
    [SerializeField] private BountyAssetDatabase bountyAssetDatabase;


    [SerializeField] private TMP_Text bountyTitle;
    [SerializeField] private TMP_Text bountySubtext;
    [SerializeField] private TMP_Text bountyDescription;
    [SerializeField] private TMP_Text bountyRewards;
    [SerializeField] private TMP_Text chooseBountyText;
    [SerializeField] private CrossFadeHandler crossFadeHandler;
    [SerializeField] private Sprite defaultBackground;
    [SerializeField] private float crossFadeDuration = 1f;

#nullable enable
    protected virtual void Awake()
    {
        BountyButton.BountyOnHoverEvent += OnBountyHover;
        BountyButton.BountyOnHoverEndEvent += OnBountyHoverEnd;
        ConstructBountyButtons();
    }

    void OnDestroy()
    {
        BountyButton.BountyOnHoverEvent -= OnBountyHover;
        BountyButton.BountyOnHoverEndEvent -= OnBountyHoverEnd;
    }

    public void ConstructBountyButtons()
    {
        var bountyCollection = BountyManager.Instance.SelectedBountyInformation?.BountyCollection;
        if (bountyCollection == null) return;

        int ind = 0;
        foreach (var bounty in bountyCollection)
        {
            BountyButton b = CreateButtonForBounty(bounty);
            b.transform.SetParent(bountyGrid[ind++]);
            b.transform.localPosition = Vector3.zero;
        }
    }

    private BountyButton CreateButtonForBounty(IBounties bounty)
    {
        BountyButton bountyButton = Instantiate(bountyButtonPrefab);
        bountyButton.Initialize(bounty, bountyAssetDatabase);
        return bountyButton;
    }

    public void OpenScene(string s)
    {
        FadeLevelIn(s);
    }

    public void StartLevel()
    {
        if (BountyManager.Instance.ActiveBounty != null)
        {
            OpenScene(BountyManager.Instance.ActiveBounty.SceneName);
        }
    }

    void FadeLevelIn(string levelName)
    {
        GameStateManager.Instance.LoadScene(levelName);
    }

    private void ClearPopupText()
    {
        bountyTitle.gameObject.SetActive(false);
        bountySubtext.gameObject.SetActive(false);
        bountyDescription.gameObject.SetActive(false);
        bountyRewards.gameObject.SetActive(false);

        chooseBountyText.gameObject.SetActive(true);
    }

    private void SetPopupText(IBounties? bounty)
    {
        if (bounty == null)
        {
            ClearPopupText();
            return;
        }

        bool completed = BountyManager.Instance.IsBountyCompleted(bounty);

        bountyTitle.SetText(bounty.BountyName);
        bountyTitle.gameObject.SetActive(true);

        bountySubtext.SetText(bounty.SubText);
        bountySubtext.gameObject.SetActive(true);

        bountyDescription.SetText(bounty.FlavourText);
        bountyDescription.gameObject.SetActive(true);

        bountyRewards.SetText(
            completed ?
            $"<color=#66AB51><size=150%>Bounty completed!</size>\nObtained </color>{bounty.Rewards}"
            :
            $"<color=#FC7B8C><size=150%>Rewards:</size></color>\n{bounty.Rewards}"
        );

        bountyRewards.gameObject.SetActive(true);

        chooseBountyText.gameObject.SetActive(false);
    }


    // We need to update the popup text
    private void OnBountyHover(IBounties bounty)
    {
        SetPopupText(bounty);
        StartCrossFadeBackground(bounty, crossFadeDuration);
    }

    // We need to remove the popup text, and replace it with the selected bounty text if 
    // a bounty is selected
    private void OnBountyHoverEnd(IBounties bounty)
    {
        SetPopupText(BountyManager.Instance.ActiveBounty);
        StartCrossFadeBackground(BountyManager.Instance.ActiveBounty, crossFadeDuration);
    }

    public void OnBackPressed()
    {
        new ClearBounty().Invoke();
        FadeLevelIn(SceneData.Get<SceneData.LevelSelect>().SceneName);
    }

    private void StartCrossFadeBackground(IBounties? bounty, float duration)
    {
        Sprite s = bounty != null ? bounty.GetBountyAssets(bountyAssetDatabase).Background : defaultBackground;
        crossFadeHandler.CrossFadeTo(s, duration);
    }
}
