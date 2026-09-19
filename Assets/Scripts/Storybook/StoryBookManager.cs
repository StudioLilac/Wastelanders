using UnityEngine;
using Storybook;

public class StoryBookManager : MonoBehaviour
{
    [SerializeField] private StoryBookSelectButton storyBookPrefab;
    [SerializeField] private Transform layoutManager;

    [System.Serializable]
    public struct StoryBookThumbnailMapping
    {
        public StorybookSceneEnum SceneEnum;
        public Sprite Thumbnail;
    }

    [SerializeField] private StoryBookThumbnailMapping[] thumbnails;

    void Start()
    {
        // Enumerate the 4 storybook scenes
        StorybookSceneEnum[] scenes = { 
            StorybookSceneEnum.Storybook1, 
            StorybookSceneEnum.Storybook2, 
            StorybookSceneEnum.Storybook3, 
            StorybookSceneEnum.Storybook4 
        };

        foreach (var sceneEnum in scenes)
        {
            StoryBookSelectButton b = Instantiate(storyBookPrefab, layoutManager);
            Sprite thumbnail = GetThumbnail(sceneEnum);
            b.Bind(sceneEnum, thumbnail);
        }
    }

    private Sprite GetThumbnail(StorybookSceneEnum sceneEnum)
    {
        if (thumbnails == null) return null;
        foreach (var mapping in thumbnails)
        {
            if (mapping.SceneEnum == sceneEnum) return mapping.Thumbnail;
        }
        return null;
    }
}
