using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class EpilogueButton : SceneSelectButton
{
    private class EpilogueSceneDataWrapper : ISelectableSceneData
    {
        private EpilogueSceneData sceneData;
        private EpilogueThumbnails thumbnails;
        private int completedBounties;

        public EpilogueSceneDataWrapper(EpilogueSceneData sceneData, EpilogueThumbnails thumbnails)
        {
            this.sceneData = sceneData;
            this.thumbnails = thumbnails;
            this.completedBounties = BountyManager.Instance.GetBountyProgress();
        }

        public string Title => sceneData.EpilogueTitle;
        
        public bool IsLocked => sceneData.BountyRequirement > completedBounties;
        
        public string RequirementText
        {
            get
            {
                float completed = Mathf.Min(completedBounties, sceneData.BountyRequirement);
                return $"{completed}/{sceneData.BountyRequirement} bounties";
            }
        }
        
        public Sprite Thumbnail => sceneData.GetThumbnail(thumbnails);
        
        public void OnClick()
        {
            GameStateManager.Instance.LoadScene(sceneData.SceneData.SceneName);
        }
    }

    public void Bind(EpilogueSceneData epilogueSceneData, EpilogueThumbnails thumbnails)
    {
        base.Bind(new EpilogueSceneDataWrapper(epilogueSceneData, thumbnails));
    }
}
