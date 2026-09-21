using UnityEngine;
using Storybook;

public class StoryBookSelectButton : SceneSelectButton
{
    private class StoryBookDataWrapper : ISelectableSceneData
    {
        private StorybookSceneEnum sceneEnum;
        private Sprite thumbnailSprite;

        public StoryBookDataWrapper(StorybookSceneEnum sceneEnum, Sprite thumbnail)
        {
            this.sceneEnum = sceneEnum;
            this.thumbnailSprite = thumbnail;
        }

        public string Title => sceneEnum.ToString().Replace("Storybook", "Storybook ");
         
        public bool IsLocked => false;
        public string RequirementText => ""; 
        
        public Sprite Thumbnail => thumbnailSprite;

        public void OnClick() => GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.StorybookScene>().SceneName, payload: new StoryBookEntry(sceneEnum));
    }

    public void Bind(StorybookSceneEnum sceneEnum, Sprite thumbnail)
    {
        base.Bind(new StoryBookDataWrapper(sceneEnum, thumbnail));
    }
}
