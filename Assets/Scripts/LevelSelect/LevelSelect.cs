using LevelSelectInformation;
using System.Collections;
using UnityEngine;

public class LevelSelect : MonoBehaviour
{
    public void OnEnable()
    {
        StageInformation.StageInformationEvent += OpenScene;
        BountyInformation.BountyInformationEvent += OpenBountyByTypeName;
    }
    public void OnDisable()
    {
        StageInformation.StageInformationEvent -= OpenScene;
        BountyInformation.BountyInformationEvent -= OpenBountyByTypeName;
    }

    protected void OpenScene(string s)
    {
        FadeLevelIn(s);
    }

    private void OpenBountyByTypeName(BountyInformation bountyInformation)
    {
        BountyManager.Instance.SelectedBountyInformation = bountyInformation;
        FadeLevelIn(SceneData.Get<SceneData.ContractSelect>().SceneName);
    }

    public void DeckSelect()
    {
        OpenScene(SceneData.Get<SceneData.SelectionScreen>().SceneName);
    }

    public void MainMenu()
    {
        OpenScene(SceneData.Get<SceneData.MainMenu>().SceneName);
    }

    public void Tutorial()
    {
        OpenScene(SceneData.Get<SceneData.TutorialFight>().SceneName);
    }

    public void LevelSelectScene()
    {
        OpenScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
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

    public void PrincessFrogBounties()
    {
        OpenBountyByTypeName(BountyInformation.PRINCESS_FROG_BOUNTY);
    }

    void FadeLevelIn(string levelName)
    {
        GameStateManager.Instance.LoadScene(levelName);
    }
}