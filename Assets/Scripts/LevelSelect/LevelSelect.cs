using LevelSelectInformation;
using System.Collections;
using Systems.Persistence;
using UnityEngine;
using static LevelSelectInformation.BountyInformation;

public class LevelSelect : MonoBehaviour
{
    public void Awake()
    {
        this.Subscribe<StageInformationEvent> (e => OpenScene(e.SceneName));
        this.Subscribe<BountyInformationEvent>(e => OpenScene(SceneData.Get<SceneData.ContractSelect>().SceneName));
    }

    public void PrincessFrogBounties()
    {
        new BountyInformationEvent(Get<PrincessFrogBounty>()).Invoke();
    }

    protected void OpenScene(string s)
    {
        FadeLevelIn(s);
    }
    public void DeckSelect()
    {
        OpenScene(SceneData.Get<SceneData.SelectionScreen>().SceneName);
    }

    public void MainMenu()
    {
        OpenScene(SceneData.Get<SceneData.MainMenu>().SceneName);
    }

    public void LevelSelectScene()
    {
        OpenScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
    }

    public void CreditsScene() 
    {
        OpenScene(SceneData.Get<SceneData.Credits>().SceneName);
    }

    public void ContractScene()
    {
        OpenScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
    }

    public void Epilogue()
    {
        OpenScene(SceneData.Get<SceneData.Epilogue>().SceneName);
    }
    
    
    public void PreBounty0()
    {
        OpenScene(SceneData.Get<SceneData.PreBounty0>().SceneName);
    }
    
    public void PreBounty2()
    {
        OpenScene(SceneData.Get<SceneData.PreBounty2>().SceneName);
    }

    void FadeLevelIn(string levelName)
    {
        if (new GetSaveSystemStatus().Query() is SaveStatus.Error)
        {
            new DisplayWarning(new PopupType.CustomPopup("Wow, I'm glad I added this fallback. The current save file has issues right now. \n Could you please try restarting the game or notifying the devs?")).Invoke();
            return;
        }
        GameStateManager.Instance.LoadScene(levelName);
    }
}